import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Song } from '../models/song.model';
import { FavoritesService } from './favorites.service';
import { SongService } from './song.service';
import { AuthService } from './auth.service';

export interface CurrentTrack {
  name: string;
  artist: string;
  coverImageUrl?: string;
}

export interface PlayableSong {
  songId: number;
  songTitle: string;
  artistName: string;
  audioFileUrl: string;
  coverImageUrl?: string | null;
  durationSeconds: number;
}

@Injectable({
  providedIn: 'root'
})
export class PlayerService {
  private audio?: HTMLAudioElement;
  private currentSong: PlayableSong | null = null;
  private playlist: PlayableSong[] = [];
  private currentIndex = -1;
  private favoriteSongIds: Set<number> = new Set();
  private playCountTracked: Set<number> = new Set(); // Track songs that already counted play
  private maxPlayedTime: Map<number, number> = new Map(); // Track max played time for each song
  private playingPlaylistId: number | null = null; // Track which playlist is currently playing
  private previousTime: Map<number, number> = new Map(); // Track previous time for each song to detect seeks
  private accumulatedListenTime: Map<number, number> = new Map(); // Track accumulated listen time for each song

  // State observables
  private currentTrackSubject = new BehaviorSubject<CurrentTrack>({
    name: 'Chọn bài hát để phát',
    artist: 'Nghệ sĩ'
  });
  private isPlayingSubject = new BehaviorSubject<boolean>(false);
  private currentTimeSubject = new BehaviorSubject<number>(0);
  private durationSubject = new BehaviorSubject<number>(0);
  private progressSubject = new BehaviorSubject<number>(0);
  private volumeSubject = new BehaviorSubject<number>(70);
  private currentSongIdSubject = new BehaviorSubject<number | null>(null);
  private isFavoriteSubject = new BehaviorSubject<boolean>(false);
  private errorSubject = new BehaviorSubject<string | null>(null);
  private bufferedProgressSubject = new BehaviorSubject<number>(0);
  private playCountUpdatedSubject = new BehaviorSubject<{ songId: number; playCount: number } | null>(null);

  // Public observables
  currentTrack$ = this.currentTrackSubject.asObservable();
  isPlaying$ = this.isPlayingSubject.asObservable();
  currentTime$ = this.currentTimeSubject.asObservable();
  duration$ = this.durationSubject.asObservable();
  progress$ = this.progressSubject.asObservable();
  volume$ = this.volumeSubject.asObservable();
  currentSongId$ = this.currentSongIdSubject.asObservable();
  isFavorite$ = this.isFavoriteSubject.asObservable();
  error$ = this.errorSubject.asObservable();
  bufferedProgress$ = this.bufferedProgressSubject.asObservable();
  playCountUpdated$ = this.playCountUpdatedSubject.asObservable();

  constructor(
    private favoritesService: FavoritesService,
    private songService: SongService,
    private authService: AuthService
  ) {
    this.initAudio();
    this.loadFavorites();
  }

  private initAudio(): void {
    if (this.audio) {
      return;
    }

    this.audio = new Audio();
    this.audio.addEventListener('timeupdate', () => this.onTimeUpdate());
    this.audio.addEventListener('progress', () => this.onProgress());
    this.audio.addEventListener('ended', () => this.onEnded());
    this.audio.addEventListener('loadedmetadata', () => this.onLoadedMetadata());
    this.audio.addEventListener('error', () => {
      this.errorSubject.next('Lỗi khi phát nhạc');
    });
  }

  private onTimeUpdate(): void {
    if (!this.audio || !this.currentSong) return;
    
    const currentTime = this.audio.currentTime;
    const duration = this.audio.duration || this.durationSubject.value;
    const progress = duration ? (currentTime / duration) * 100 : 0;

    this.currentTimeSubject.next(currentTime);
    this.durationSubject.next(duration);
    this.progressSubject.next(progress);

    // Track play count when user listens to 30% of song
    this.trackPlayCount(currentTime, duration);
  }

  private trackPlayCount(currentTime: number, duration: number): void {
    if (!this.currentSong) return;

    const songId = this.currentSong.songId;
    
    // Sử dụng duration từ currentSong nếu audio duration chưa có
    const songDuration = duration || this.currentSong.durationSeconds;
    if (!songDuration || songDuration <= 0) {
      return;
    }

    const thirtyPercent = songDuration * 0.3;
    
    // Kiểm tra user đã đăng nhập chưa
    if (!this.authService.isAuthenticated()) {
      return;
    }

    const previousTime = this.previousTime.get(songId) ?? 0;
    
    // Phát hiện seek: nếu currentTime < previousTime hoặc nhảy quá lớn (> 2 giây)
    // thì reset lại accumulated listen time từ vị trí seek
    if (currentTime < previousTime || (currentTime - previousTime) > 2) {
      // Đây là seek, reset accumulated time về 0 và bắt đầu đếm từ vị trí seek
      this.accumulatedListenTime.set(songId, 0);
      this.previousTime.set(songId, currentTime);
      return;
    }

    // Nếu đang phát bình thường (currentTime >= previousTime)
    if (currentTime >= previousTime && this.isPlayingSubject.value) {
      // Tính thời gian đã nghe trong khoảng này
      const timeListened = currentTime - previousTime;
      const accumulated = (this.accumulatedListenTime.get(songId) || 0) + timeListened;
      this.accumulatedListenTime.set(songId, accumulated);
    }

    // Cập nhật previousTime
    this.previousTime.set(songId, currentTime);

    // Chỉ tăng view khi thời gian đã nghe tích lũy >= 30% và chưa track
    const accumulatedTime = this.accumulatedListenTime.get(songId) || 0;
    if (accumulatedTime >= thirtyPercent && !this.playCountTracked.has(songId)) {
      this.playCountTracked.add(songId);
      
      // Gọi API để update play count với thời gian đã nghe tích lũy
      this.songService.trackPlay(songId, Math.floor(accumulatedTime)).subscribe({
        next: (response) => {
          if (response && typeof response.playCount === 'number') {
            this.playCountUpdatedSubject.next({
              songId,
              playCount: response.playCount
            });
          }
        },
        error: (error) => {
          console.error('Error tracking play count:', error);
          // Nếu lỗi, remove khỏi set để có thể thử lại
          this.playCountTracked.delete(songId);
        }
      });
    }
  }

  private onProgress(): void {
    if (!this.audio) return;
    
    const buffered = this.audio.buffered;
    const duration = this.audio.duration || this.durationSubject.value;
    
    if (buffered.length > 0 && duration > 0) {
      // Lấy buffered range cuối cùng (thường là range lớn nhất)
      let maxBufferedEnd = 0;
      for (let i = 0; i < buffered.length; i++) {
        maxBufferedEnd = Math.max(maxBufferedEnd, buffered.end(i));
      }
      const bufferedProgress = (maxBufferedEnd / duration) * 100;
      this.bufferedProgressSubject.next(bufferedProgress);
    } else {
      this.bufferedProgressSubject.next(0);
    }
  }

  private onEnded(): void {
    this.playNext();
  }

  private onLoadedMetadata(): void {
    if (this.audio?.duration) {
      this.durationSubject.next(this.audio.duration);
    }
  }

  private loadFavorites(): void {
    this.favoritesService.getFavoriteSongs().subscribe({
      next: (favorites) => {
        this.favoriteSongIds = new Set(favorites.map(f => f.songId));
        this.updateFavoriteStatus();
      },
      error: (error) => {
        console.error('Error loading favorites:', error);
      }
    });
  }

  private updateFavoriteStatus(): void {
    if (this.currentSong) {
      const isFavorite = this.favoriteSongIds.has(this.currentSong.songId);
      this.isFavoriteSubject.next(isFavorite);
    }
  }

  // Set which playlist is currently playing (called from playlist components)
  setPlayingPlaylistId(playlistId: number | null): void {
    this.playingPlaylistId = playlistId;
  }

  // Get which playlist is currently playing
  getPlayingPlaylistId(): number | null {
    return this.playingPlaylistId;
  }

  // Convert Song to PlayableSong
  private toPlayableSong(song: Song | PlayableSong): PlayableSong {
    return {
      songId: song.songId,
      songTitle: song.songTitle,
      artistName: song.artistName,
      audioFileUrl: song.audioFileUrl,
      coverImageUrl: song.coverImageUrl,
      durationSeconds: song.durationSeconds
    };
  }

  // Set playlist and play a song
  playSong(song: Song | PlayableSong, playlist?: (Song | PlayableSong)[]): void {
    const playableSong = this.toPlayableSong(song);
    
    if (!playableSong.audioFileUrl) {
      this.errorSubject.next('Bài hát không có đường dẫn audio hợp lệ.');
      return;
    }

    // Update playlist if provided
    if (playlist && playlist.length > 0) {
      this.playlist = playlist.map(s => this.toPlayableSong(s));
      this.currentIndex = this.playlist.findIndex(s => s.songId === playableSong.songId);
      // Note: playingPlaylistId should be set by the component that calls playSong
      // We don't reset it here to allow components to set it explicitly
    } else if (this.playlist.length === 0 || !this.playlist.find(s => s.songId === playableSong.songId)) {
      // If no playlist provided and current song not in playlist, create a single-item playlist
      // This means song is played from elsewhere (home, favorites, etc.), so reset playingPlaylistId
      this.playlist = [playableSong];
      this.currentIndex = 0;
      this.playingPlaylistId = null; // Reset because not playing from a specific playlist
    } else {
      // Song is already in playlist, just update index
      // Keep playingPlaylistId as is (might be from a playlist)
      this.currentIndex = this.playlist.findIndex(s => s.songId === playableSong.songId);
    }

    this.currentSong = playableSong;
    this.errorSubject.next(null);

    // Reset play count tracking mỗi khi phát bài (kể cả phát lại cùng bài)
    // Reset max played time và playCountTracked để có thể track lại
    this.maxPlayedTime.set(playableSong.songId, 0);
    this.playCountTracked.delete(playableSong.songId);
    this.accumulatedListenTime.set(playableSong.songId, 0);
    this.previousTime.set(playableSong.songId, 0);

    // Gọi track-play ngay khi bắt đầu phát để tạo listening history (không tăng view nếu backend kiểm tra duration)
    if (this.authService.isAuthenticated()) {
      this.songService.trackPlay(playableSong.songId, 0).subscribe({
        next: (response) => {
          if (response && typeof response.playCount === 'number') {
            this.playCountUpdatedSubject.next({
              songId: playableSong.songId,
              playCount: response.playCount
            });
          }
        },
        error: (error) => {
          console.error('Error tracking play start:', error);
        }
      });
    }

    // Update track info
    this.currentTrackSubject.next({
      name: playableSong.songTitle,
      artist: playableSong.artistName,
      coverImageUrl: playableSong.coverImageUrl ?? undefined
    });

    this.currentSongIdSubject.next(playableSong.songId);
    this.updateFavoriteStatus();

    // Setup audio
    if (!this.audio) {
      this.initAudio();
    }

    if (this.audio) {
      this.audio.src = playableSong.audioFileUrl;
      this.audio.currentTime = 0;
      this.durationSubject.next(playableSong.durationSeconds || 0);
      this.audio.volume = this.volumeSubject.value / 100;
      this.bufferedProgressSubject.next(0); // Reset buffered progress
      
      this.audio.play()
        .then(() => {
          this.isPlayingSubject.next(true);
        })
        .catch(() => {
          this.isPlayingSubject.next(false);
          this.errorSubject.next('Không thể phát bài hát.');
        });
    }
  }

  togglePlayback(): void {
    if (!this.audio) {
      return;
    }

    if (this.isPlayingSubject.value) {
      this.audio.pause();
      this.isPlayingSubject.next(false);
    } else {
      this.audio.play().then(() => {
        this.isPlayingSubject.next(true);
      }).catch(() => {
        this.errorSubject.next('Không thể tiếp tục phát bài hát.');
      });
    }
  }

  seekTo(positionInSeconds: number): void {
    if (!this.audio || Number.isNaN(positionInSeconds)) {
      return;
    }

    this.audio.currentTime = positionInSeconds;
    this.currentTimeSubject.next(positionInSeconds);
    
    // Reset accumulated listen time khi seek để chỉ đếm từ vị trí seek
    if (this.currentSong) {
      this.accumulatedListenTime.set(this.currentSong.songId, 0);
      this.previousTime.set(this.currentSong.songId, positionInSeconds);
    }
  }

  changeVolume(newVolume: number): void {
    this.volumeSubject.next(newVolume);
    if (this.audio) {
      this.audio.volume = newVolume / 100;
    }
  }

  toggleMute(): void {
    if (!this.audio) {
      return;
    }
    const currentVolume = this.volumeSubject.value;
    if (this.audio.volume > 0) {
      this.audio.volume = 0;
    } else {
      this.audio.volume = currentVolume / 100;
    }
  }

  replayCurrentTrack(): void {
    if (!this.audio || !this.currentSong) {
      return;
    }

    // Reset play count tracking khi phát lại
    this.maxPlayedTime.set(this.currentSong.songId, 0);
    this.playCountTracked.delete(this.currentSong.songId);
    this.accumulatedListenTime.set(this.currentSong.songId, 0);
    this.previousTime.set(this.currentSong.songId, 0);

    this.audio.currentTime = 0;
    this.currentTimeSubject.next(0);
    this.progressSubject.next(0);

    this.audio.play().then(() => {
      this.isPlayingSubject.next(true);
    }).catch(() => {
      this.errorSubject.next('Không thể phát lại bài hát.');
    });
  }

  playNext(): void {
    if (this.playlist.length === 0 || this.currentIndex === -1) {
      this.isPlayingSubject.next(false);
      this.progressSubject.next(0);
      this.currentTimeSubject.next(0);
      return;
    }

    const nextIndex = (this.currentIndex + 1) % this.playlist.length;
    const nextSong = this.playlist[nextIndex];

    if (nextSong && nextSong.songId !== this.currentSong?.songId) {
      this.playSong(nextSong);
    } else {
      this.isPlayingSubject.next(false);
    }
  }

  playPrevious(): void {
    if (this.playlist.length === 0 || this.currentIndex === -1) {
      this.isPlayingSubject.next(false);
      this.progressSubject.next(0);
      this.currentTimeSubject.next(0);
      return;
    }

    const prevIndex = this.currentIndex === -1
      ? 0
      : (this.currentIndex - 1 + this.playlist.length) % this.playlist.length;
    const prevSong = this.playlist[prevIndex];

    if (prevSong && prevSong.songId !== this.currentSong?.songId) {
      this.playSong(prevSong);
    } else {
      this.isPlayingSubject.next(false);
    }
  }

  shufflePlay(): void {
    if (this.playlist.length === 0) {
      this.errorSubject.next('Không có bài hát để phát ngẫu nhiên.');
      return;
    }

    const availableSongs = this.playlist.filter(song => song.songId !== this.currentSong?.songId);
    if (availableSongs.length === 0) {
      return;
    }

    const randomIndex = Math.floor(Math.random() * availableSongs.length);
    const randomSong = availableSongs[randomIndex];
    this.playSong(randomSong);
  }

  async toggleFavorite(): Promise<void> {
    if (!this.currentSong) {
      return;
    }

    const songId = this.currentSong.songId;
    const isCurrentlyFavorite = this.favoriteSongIds.has(songId);

    if (isCurrentlyFavorite) {
      // Remove from favorites
      this.favoritesService.removeFromFavorites(songId).subscribe({
        next: () => {
          this.favoriteSongIds.delete(songId);
          this.isFavoriteSubject.next(false);
        },
        error: (error) => {
          console.error('Error removing from favorites:', error);
          this.errorSubject.next('Không thể xóa khỏi yêu thích');
        }
      });
    } else {
      // Add to favorites
      this.favoritesService.addToFavorites(songId).subscribe({
        next: () => {
          this.favoriteSongIds.add(songId);
          this.isFavoriteSubject.next(true);
        },
        error: (error) => {
          console.error('Error adding to favorites:', error);
          this.errorSubject.next('Không thể thêm vào yêu thích');
        }
      });
    }
  }

  // Get current values (for components that need synchronous access)
  getCurrentTrack(): CurrentTrack {
    return this.currentTrackSubject.value;
  }

  getIsPlaying(): boolean {
    return this.isPlayingSubject.value;
  }

  getCurrentTime(): number {
    return this.currentTimeSubject.value;
  }

  getDuration(): number {
    return this.durationSubject.value;
  }

  getProgress(): number {
    return this.progressSubject.value;
  }

  getVolume(): number {
    return this.volumeSubject.value;
  }

  getCurrentSongId(): number | null {
    return this.currentSongIdSubject.value;
  }

  getIsFavorite(): boolean {
    return this.isFavoriteSubject.value;
  }

  getBufferedProgress(): number {
    return this.bufferedProgressSubject.value;
  }

  // Refresh favorites (call this when favorites might have changed)
  refreshFavorites(): void {
    this.favoritesService.getFavoriteSongs().subscribe({
      next: (favorites) => {
        this.favoriteSongIds = new Set(favorites.map(f => f.songId));
        this.updateFavoriteStatus();
      },
      error: (error) => {
        console.error('Error refreshing favorites:', error);
      }
    });
  }

  // Stop playback (used when logging out)
  stop(): void {
    if (this.audio) {
      this.audio.pause();
      this.audio.currentTime = 0;
    }
    this.isPlayingSubject.next(false);
    this.currentTimeSubject.next(0);
    this.progressSubject.next(0);
    this.bufferedProgressSubject.next(0);
    this.currentSong = null;
    this.currentSongIdSubject.next(null);
    this.playingPlaylistId = null; // Reset playing playlist
    this.currentTrackSubject.next({
      name: 'Chọn bài hát để phát',
      artist: 'Nghệ sĩ'
    });
  }
}

