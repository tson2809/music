import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { Song, SongListResponse } from '../../models/song.model';
import { SongService, RecentlyPlayedItem, RecentlyPlayedResponse } from '../../services/song.service';
import { PlayerService } from '../../services/player.service';
import { LyricsService } from '../../services/lyrics.service';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { Subscription, combineLatest } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, NgIf, NgFor, RouterModule, SidebarComponent, SearchHeaderComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  songs: Song[] = [];
  isLoadingSongs = false;
  songError: string | null = null;
  recentlyPlayed: RecentlyPlayedItem[] = [];
  isLoadingRecentlyPlayed = false;
  showAllRecentlyPlayed = false;
  currentSongId: number | null = null;
  isPlaying = false;
  showLyrics = false;
  lyrics: string | null = null;
  lyricsLines: Array<{ text: string; time: number }> = [];
  hasTimestamps = false; // Track if lyrics have timestamps
  isLoadingLyrics = false;
  currentTrackName = '';
  currentArtistName = '';
  currentTime = 0;
  duration = 0;
  activeLineIndex = -1;
  private loadedLyricsSongId: number | null = null;
  private recentlyPlayedReloadTimer: any = null;
  private lastTrackedSongId: number | null = null;

  private subscriptions = new Subscription();

  constructor( 
    private authService: AuthService,
    private router: Router,
    private songService: SongService,
    private playerService: PlayerService,
    public lyricsService: LyricsService
  ) {}

  ngOnInit() {
    this.authService.authState$.subscribe(state => {
      const previousUser = this.currentUser;
      this.currentUser = state.user;
      // Load recently played when user logs in
      if (state.user && !previousUser) {
        this.loadRecentlyPlayed();
      } else if (!state.user) {
        this.recentlyPlayed = [];
      }
    });
    
    // Subscribe to player state
    const playerSub = combineLatest([
      this.playerService.currentSongId$,
      this.playerService.isPlaying$,
      this.playerService.currentTrack$,
      this.playerService.currentTime$,
      this.playerService.duration$
    ]).subscribe(([songId, playing, track, time, dur]) => {
      const previousSongId = this.currentSongId;
      this.currentSongId = songId;
      this.isPlaying = playing;
      this.currentTrackName = track.name;
      this.currentArtistName = track.artist;
      this.currentTime = time;
      this.duration = dur;
      
      // Reload recently played when a new song starts playing
      if (songId && songId !== previousSongId && this.currentUser) {
        // Clear any existing timer
        if (this.recentlyPlayedReloadTimer) {
          clearTimeout(this.recentlyPlayedReloadTimer);
        }
        
        // Optimistic update: Add song to recently played immediately
        if (track.name && track.artist) {
          const existingIndex = this.recentlyPlayed.findIndex(item => item.type === 'song' && item.id === songId);
          if (existingIndex >= 0) {
            // If song already exists, remove it first
            this.recentlyPlayed.splice(existingIndex, 1);
          }
          
          // Add new item at the beginning
          const newItem: RecentlyPlayedItem = {
            type: 'song',
            id: songId,
            title: track.name,
            subtitle: track.artist,
            imageUrl: track.coverImageUrl,
            playedAt: new Date().toISOString()
          };
          this.recentlyPlayed.unshift(newItem);
          
          // Limit to 20 items
          if (this.recentlyPlayed.length > 20) {
            this.recentlyPlayed = this.recentlyPlayed.slice(0, 20);
          }
        }
        
        // Reload from server in background after a delay to sync with backend
        this.recentlyPlayedReloadTimer = setTimeout(() => {
          this.loadRecentlyPlayed(true); // Pass true to indicate silent reload
        }, 2000); // Wait 2 seconds for backend to process
      }
      
      // Load lyrics if showing lyrics panel and song changed
      if (this.showLyrics && songId && songId !== previousSongId) {
        this.loadLyrics(songId, dur);
      } else if (!songId) {
        this.lyrics = null;
        this.lyricsLines = [];
        this.hasTimestamps = false;
        this.activeLineIndex = -1;
        this.loadedLyricsSongId = null;
      }
      
      // Update active line based on current time (only if lyrics have timestamps)
      if (this.showLyrics && this.hasTimestamps && this.lyricsLines.length > 0) {
        this.updateActiveLine(time);
      }
    });
    this.subscriptions.add(playerSub);

    // Subscribe to lyrics service
    const lyricsSub = this.lyricsService.showLyrics$.subscribe(show => {
      this.showLyrics = show;
      if (show && this.currentSongId) {
        // Always load lyrics when showing panel, even if already loaded
        // This ensures lyrics are displayed when reopening the panel
        this.loadLyrics(this.currentSongId, this.duration, true);
      } else if (!show) {
        // Clear lyrics when hiding panel, but keep loadedLyricsSongId
        // so we can reload quickly if needed
        this.lyrics = null;
        this.lyricsLines = [];
        this.hasTimestamps = false;
        this.activeLineIndex = -1;
      }
    });
    this.subscriptions.add(lyricsSub);
    
    this.loadSongs();
    // Load recently played only if user is logged in
    if (this.currentUser) {
      this.loadRecentlyPlayed();
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    if (this.recentlyPlayedReloadTimer) {
      clearTimeout(this.recentlyPlayedReloadTimer);
    }
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  formatDuration(seconds?: number | null) {
    if (seconds === null || seconds === undefined || Number.isNaN(seconds)) {
      return '--:--';
    }
    const minutes = Math.floor(seconds / 60);
    const remaining = Math.floor(seconds % 60);
    return `${minutes}:${remaining.toString().padStart(2, '0')}`;
  }

  onSongCardClick(song: Song): void {
    // Navigate to song detail page
    this.router.navigate(['/songs', song.songId]);
  }

  onSongPlayClick(event: Event, song: Song): void {
    // Ngăn event bubbling để không trigger onSongCardClick
    event.stopPropagation();
    
    // Kiểm tra đăng nhập trước khi phát nhạc
    if (!this.currentUser) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: '/home' }
      });
      return;
    }

    const currentSongId = this.playerService.getCurrentSongId();
    if (currentSongId === song.songId) {
      this.playerService.togglePlayback();
      return;
    }
    // Play song with the full songs list as playlist
    this.playerService.playSong(song, this.songs);
    // Reset playingPlaylistId because we're playing from home page, not from a specific playlist
    this.playerService.setPlayingPlaylistId(null);
  }

  private loadSongs() {
    this.isLoadingSongs = true;
    this.songService.getSongs(1, 50).subscribe({
      next: (response: SongListResponse) => {
        const songs = response?.songs ?? [];
        this.songs = Array.isArray(songs) ? songs : [];
        this.isLoadingSongs = false;
        this.songError = null;
        if (this.songs.length === 0) {
          this.songError = 'Chưa có bài hát nào để phát.';
        }
      },
      error: () => {
        this.songError = 'Không thể tải danh sách bài hát.';
        this.isLoadingSongs = false;
      }
    });
  }

  private loadLyrics(songId: number, duration: number = 0, forceReload: boolean = false) {
    if (!forceReload && this.loadedLyricsSongId === songId && this.lyrics) {
      // Already loaded for this song and lyrics are available
      return;
    }

    // Reset lyrics when loading new song
    this.lyrics = null;
    this.lyricsLines = [];
    this.hasTimestamps = false;
    this.activeLineIndex = -1;
    this.loadedLyricsSongId = null;
    this.isLoadingLyrics = true;
    
    this.songService.getSongDetail(songId).subscribe({
      next: (response) => {
        this.lyrics = response?.lyrics || null;
        if (this.lyrics) {
          // Use duration from playerService if available, otherwise from response
          const songDuration = duration > 0 ? duration : (response?.durationSeconds || 0);
          // Parse lyrics - support both LRC format and plain text
          const parseResult = this.parseLyrics(this.lyrics, songDuration);
          this.lyricsLines = parseResult.lines;
          this.hasTimestamps = parseResult.hasTimestamps;
        } else {
          this.lyricsLines = [];
          this.hasTimestamps = false;
        }
        this.loadedLyricsSongId = songId;
        this.isLoadingLyrics = false;
      },
      error: () => {
        this.lyrics = null;
        this.lyricsLines = [];
        this.hasTimestamps = false;
        this.activeLineIndex = -1;
        this.loadedLyricsSongId = null;
        this.isLoadingLyrics = false;
      }
    });
  }

  private parseLyrics(lyricsText: string, duration: number): { lines: Array<{ text: string; time: number }>; hasTimestamps: boolean } {
    const trimmedText = lyricsText.trim();
    
    // Try to parse as JSON format (Spotify/Musixmatch style)
    try {
      const jsonData = JSON.parse(trimmedText);
      if (Array.isArray(jsonData) && jsonData.length > 0) {
        const parsedLines: Array<{ text: string; time: number }> = [];
        let hasTimestamps = false;
        
        for (const item of jsonData) {
          if (typeof item === 'object' && item !== null) {
            let time = 0;
            let text = '';
            let hasTime = false;
            
            // Support multiple JSON formats:
            // Format 1: { "text": "...", "time": 5.5 } (time in seconds)
            // Format 2: { "text": "...", "startTime": 5500 } (time in milliseconds)
            // Format 3: { "text": "...", "start": 5.5 } (time in seconds)
            
            if (item.text || item.lyrics) {
              text = (item.text || item.lyrics).trim();
            }
            
            if (item.time !== undefined) {
              time = typeof item.time === 'number' ? item.time : parseFloat(item.time);
              hasTime = true;
            } else if (item.startTime !== undefined) {
              // Convert milliseconds to seconds
              time = typeof item.startTime === 'number' ? item.startTime / 1000 : parseFloat(item.startTime) / 1000;
              hasTime = true;
            } else if (item.start !== undefined) {
              time = typeof item.start === 'number' ? item.start : parseFloat(item.start);
              hasTime = true;
            }
            
            if (text) {
              parsedLines.push({ text, time });
              if (hasTime) {
                hasTimestamps = true;
              }
            }
          }
        }
        
        if (parsedLines.length > 0) {
          // Sort by time and return
          return { 
            lines: parsedLines.sort((a, b) => a.time - b.time),
            hasTimestamps: hasTimestamps
          };
        }
      }
    } catch (e) {
      // Not JSON format, continue to check other formats
    }
    
    // Check if it's LRC format (has timestamps like [00:12.34] or [0:12.34])
    // Support both [mm:ss.xx] and [m:ss.xx] formats
    const lines = lyricsText.split('\n');
    const parsedLines: Array<{ text: string; time: number }> = [];
    // Pattern supports: [mm:ss.xx] or [m:ss.xx] or [mm:s.xx] or [m:s.xx]
    const lrcPattern = /^\[(\d{1,2}):(\d{1,2})\.(\d{2})\](.*)$/;
    let hasLrcFormat = false;
    
    for (const line of lines) {
      const trimmedLine = line.trim();
      if (!trimmedLine) continue; // Skip completely empty lines
      
      const match = trimmedLine.match(lrcPattern);
      if (match) {
        hasLrcFormat = true;
        const minutes = parseInt(match[1], 10);
        const seconds = parseInt(match[2], 10);
        const centiseconds = parseInt(match[3], 10);
        const time = minutes * 60 + seconds + centiseconds / 100;
        const text = match[4].trim();
        // Include lines even if text is empty (for pauses in music)
        // But we'll use a placeholder or skip them - actually, let's skip empty text lines
        if (text) {
          parsedLines.push({ text, time });
        }
      }
    }
    
    if (hasLrcFormat) {
      // Sort by time and return
      return {
        lines: parsedLines.sort((a, b) => a.time - b.time),
        hasTimestamps: true
      };
    }
    
    // If not JSON or LRC format, return empty lines array (no timestamps)
    // We'll display plain text lyrics instead
    return {
      lines: [],
      hasTimestamps: false
    };
  }

  private updateActiveLine(currentTime: number): void {
    // Only update if lyrics have timestamps
    if (!this.hasTimestamps || this.lyricsLines.length === 0) {
      this.activeLineIndex = -1;
      return;
    }

    // Find the line that should be active based on timestamp
    let newActiveIndex = -1;
    
    for (let i = this.lyricsLines.length - 1; i >= 0; i--) {
      if (currentTime >= this.lyricsLines[i].time) {
        newActiveIndex = i;
        break;
      }
    }
    
    if (newActiveIndex !== this.activeLineIndex && newActiveIndex >= 0) {
      this.activeLineIndex = newActiveIndex;
      // Scroll to active line
      this.scrollToActiveLine();
    }
  }

  onLyricsLineClick(line: { text: string; time: number }): void {
    // Only allow seeking if lyrics have timestamps and there's a current song
    if (!this.hasTimestamps || !this.currentSongId) {
      return;
    }
    
    // Seek to the timestamp of the clicked line
    this.playerService.seekTo(line.time);
  }

  private scrollToActiveLine(): void {
    // Use setTimeout to ensure DOM is updated
    setTimeout(() => {
      const activeElement = document.querySelector('.lyrics-line.active');
      if (activeElement) {
        activeElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
    }, 100);
  }

  private loadRecentlyPlayed(silent: boolean = false): void {
    if (!this.currentUser) {
      return;
    }

    // Only show loading indicator if not silent reload
    if (!silent) {
      this.isLoadingRecentlyPlayed = true;
    }
    
    this.songService.getRecentlyPlayed(20).subscribe({
      next: (response: RecentlyPlayedResponse) => {
        // Only update if we got valid data
        if (response.items && response.items.length > 0) {
          this.recentlyPlayed = response.items;
        }
        // Don't reset showAllRecentlyPlayed to avoid screen flicker
        // Only reset on initial load (when silent is false)
        if (!silent) {
          this.showAllRecentlyPlayed = false;
        }
        this.isLoadingRecentlyPlayed = false;
      },
      error: () => {
        // On error, keep the optimistic update
        // Only clear if it wasn't a silent reload
        if (!silent) {
          this.recentlyPlayed = [];
          this.showAllRecentlyPlayed = false;
        }
        this.isLoadingRecentlyPlayed = false;
      }
    });
  }

  onRecentlyPlayedItemClick(item: RecentlyPlayedItem): void {
    if (item.type === 'song') {
      this.router.navigate(['/songs', item.id]);
    } else if (item.type === 'artist') {
      this.router.navigate(['/artists', item.id]);
    } else if (item.type === 'album') {
      this.router.navigate(['/albums', item.id]);
    } else if (item.type === 'playlist') {
      this.router.navigate(['/playlists', item.id]);
    }
  }

  onRecentlyPlayedPlayClick(event: Event, item: RecentlyPlayedItem): void {
    // Ngăn event bubbling để không trigger onRecentlyPlayedItemClick
    event.stopPropagation();
    
    // Chỉ xử lý cho songs
    if (item.type !== 'song') {
      return;
    }
    
    // Kiểm tra đăng nhập trước khi phát nhạc
    if (!this.currentUser) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: '/home' }
      });
      return;
    }

    const currentSongId = this.playerService.getCurrentSongId();
    if (currentSongId === item.id) {
      this.playerService.togglePlayback();
      return;
    }

    // Fetch song detail để có đầy đủ thông tin Song object
    this.songService.getSongDetail(item.id).subscribe({
      next: (songDetail) => {
        if (songDetail) {
          // Convert songDetail to Song format
          const song: Song = {
            songId: songDetail.songId,
            songTitle: songDetail.songTitle,
            artistId: songDetail.artistId,
            artistName: songDetail.artistName,
            albumId: songDetail.albumId,
            albumTitle: songDetail.albumTitle,
            coverImageUrl: songDetail.coverImageUrl,
            audioFileUrl: songDetail.audioFileUrl,
            durationSeconds: songDetail.durationSeconds,
            playCount: songDetail.playCount || 0,
            likeCount: songDetail.likeCount || 0,
            genreId: songDetail.genreId,
            genreName: songDetail.genreName,
            releaseDate: songDetail.releaseDate,
            lyrics: songDetail.lyrics,
            createdAt: songDetail.createdAt || new Date().toISOString()
          };
          
          // Play song with the full songs list as playlist
          this.playerService.playSong(song, this.songs);
          // Reset playingPlaylistId because we're playing from home page, not from a specific playlist
          this.playerService.setPlayingPlaylistId(null);
        }
      },
      error: () => {
        console.error('Failed to load song details');
      }
    });
  }

  onRecentlyPlayedArtistClick(event: Event, item: RecentlyPlayedItem): void {
    // Ngăn event bubbling
    event.stopPropagation();
    
    // Chỉ xử lý cho songs - cần fetch song detail để lấy artistId
    if (item.type !== 'song') {
      return;
    }

    // Fetch song detail để lấy artistId
    this.songService.getSongDetail(item.id).subscribe({
      next: (songDetail) => {
        if (songDetail && songDetail.artistId) {
          this.router.navigate(['/artists', songDetail.artistId]);
        }
      },
      error: () => {
        console.error('Failed to load song details');
      }
    });
  }

  getDisplayedRecentlyPlayed(): RecentlyPlayedItem[] {
    if (this.showAllRecentlyPlayed || this.recentlyPlayed.length <= 5) {
      return this.recentlyPlayed;
    }
    return this.recentlyPlayed.slice(0, 5);
  }

  hasMoreRecentlyPlayed(): boolean {
    return this.recentlyPlayed.length > 5;
  }

  toggleShowAllRecentlyPlayed(): void {
    this.showAllRecentlyPlayed = !this.showAllRecentlyPlayed;
  }

  getImageUrl(imageUrl?: string): string {
    if (!imageUrl) {
      return 'https://via.placeholder.com/200x200/e0e0e0/666?text=No+Image';
    }
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl;
    }
    if (imageUrl.startsWith('images/')) {
      return `https://localhost:5001/${imageUrl}`;
    }
    return imageUrl;
  }
}

