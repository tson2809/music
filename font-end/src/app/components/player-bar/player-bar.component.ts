import { Component, EventEmitter, Input, Output, ViewChild, ElementRef, OnChanges, SimpleChanges, OnInit, OnDestroy, ChangeDetectorRef, HostListener, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FavoritesService } from '../../services/favorites.service';
import { PlaylistsService, Playlist } from '../../services/playlists.service';
import { Subject, takeUntil } from 'rxjs';

export interface CurrentTrack {
  name: string;
  artist: string;
  coverImageUrl?: string;
  audioUrl?: string;
  songId?: number;
}

@Component({
  selector: 'app-player-bar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './player-bar.component.html',
  styleUrls: ['./player-bar.component.css']
})
export class PlayerBarComponent implements OnChanges, OnInit, OnDestroy, AfterViewInit {
  @ViewChild('audioPlayer') audioPlayer?: ElementRef<HTMLAudioElement>;
  @ViewChild('addToPlaylistButton') addToPlaylistButton?: ElementRef<HTMLButtonElement>;
  @ViewChild('playlistPopupContent') playlistPopupContent?: ElementRef<HTMLDivElement>;
  
  private destroy$ = new Subject<void>();
  
  @Input() currentTrack: CurrentTrack = {
    name: 'Chọn bài hát để phát',
    artist: 'Nghệ sĩ'
  };
  @Input() progress = 0;
  @Input() volume = 70;
  @Input() isPlaying = false;
  @Input() currentTime = 0;
  @Input() duration = 0;
  @Input() currentSongId: number | null = null;
  @Input() isFavorite: boolean = false;
  @Input() bufferedProgress = 0;
  @Input() showLyrics: boolean = false;
  @Output() playPause = new EventEmitter<void>();
  @Output() seek = new EventEmitter<number>();
  @Output() volumeChange = new EventEmitter<number>();
  @Output() replay = new EventEmitter<void>();
  @Output() shuffle = new EventEmitter<void>();
  @Output() muteToggle = new EventEmitter<void>();
  @Output() previous = new EventEmitter<void>();
  @Output() next = new EventEmitter<void>();
  @Output() toggleFavorite = new EventEmitter<void>();
  @Output() toggleLyrics = new EventEmitter<void>();
  @Output() timeUpdate = new EventEmitter<number>();
  @Output() durationChange = new EventEmitter<number>();
  
  muted = false;
  private lastVolumeBeforeMute = this.volume;
  
  // Playlist popup
  showPlaylistPopup = false;
  playlists: Playlist[] = [];
  isLoadingPlaylists = false;
  playlistSongMap = new Map<number, boolean>(); // Map playlistId -> hasSong
  hasPlaylists = false;
  popupTop = 0;
  popupLeft = 0;

  constructor(
    private favoritesService: FavoritesService,
    private playlistsService: PlaylistsService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Subscribe to favorite changes to keep isFavorite in sync
    this.favoritesService.favoriteSongIds$.pipe(takeUntil(this.destroy$)).subscribe(favoriteIds => {
      if (this.currentSongId) {
        const newFavoriteStatus = favoriteIds.has(this.currentSongId);
        if (this.isFavorite !== newFavoriteStatus) {
          this.isFavorite = newFavoriteStatus;
          console.log('PlayerBar: favorite status updated from service:', this.isFavorite);
          this.cdr.detectChanges();
        }
      }
    });

    // Subscribe to playlist creation events
    this.playlistsService.onPlaylistCreated.pipe(takeUntil(this.destroy$)).subscribe(() => {
      // Khi có playlist mới được tạo, cập nhật lại hasPlaylists
      this.checkHasPlaylists();
    });

    // Check if user has playlists
    this.checkHasPlaylists();
  }

  ngAfterViewInit(): void {
    // Position popup after view init
  }

  checkHasPlaylists() {
    this.playlistsService.getPlaylists().subscribe({
      next: (playlists) => {
        this.hasPlaylists = playlists.length > 0;
      },
      error: () => {
        this.hasPlaylists = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Log all changes for debugging
    if (changes['isFavorite']) {
      console.log('PlayerBar: isFavorite changed from', changes['isFavorite'].previousValue, 'to', changes['isFavorite'].currentValue);
    }

    // When track changes, load new audio
    if (changes['currentTrack'] && this.currentTrack.audioUrl) {
      this.loadAudio();
    }

    // When isPlaying changes, play or pause
    if (changes['isPlaying'] && this.audioPlayer) {
      if (this.isPlaying) {
        this.audioPlayer.nativeElement.play().catch(err => {
          console.error('Error playing audio:', err);
        });
      } else {
        this.audioPlayer.nativeElement.pause();
      }
    }

    // When volume changes, update audio volume
    if (changes['volume'] && this.audioPlayer) {
      this.audioPlayer.nativeElement.volume = this.volume / 100;
    }
  }

  loadAudio(): void {
    if (!this.audioPlayer || !this.currentTrack.audioUrl) return;
    
    const audio = this.audioPlayer.nativeElement;
    audio.src = this.currentTrack.audioUrl;
    audio.volume = this.volume / 100;
    audio.load();
    
    if (this.isPlaying) {
      audio.play().catch(err => console.error('Error playing audio:', err));
    }
  }

  onAudioTimeUpdate(event: Event): void {
    const audio = event.target as HTMLAudioElement;
    this.timeUpdate.emit(audio.currentTime);
  }

  onAudioDurationChange(event: Event): void {
    const audio = event.target as HTMLAudioElement;
    if (audio.duration && !isNaN(audio.duration)) {
      this.durationChange.emit(audio.duration);
    }
  }

  onAudioEnded(): void {
    this.next.emit();
  }

  get volumeIcon(): string {
    if (this.muted || this.volume <= 0) {
      return 'fa-volume-xmark';
    } else if (this.volume <= 30) {
      return 'fa-volume-off';
    } else if (this.volume <= 60) {
      return 'fa-volume-low';
    }
    return 'fa-volume-high';
  }

  get playIcon() {
    return this.isPlaying ? 'pause' : 'play';
  }

  formatTime(seconds: number) {
    if (!seconds || Number.isNaN(seconds)) {
      return '0:00';
    }
    const minutes = Math.floor(seconds / 60);
    const remainder = Math.floor(seconds % 60);
    return `${minutes}:${remainder.toString().padStart(2, '0')}`;
  }

  onRangeChange(event: Event) {
    const value = Number((event.target as HTMLInputElement).value);
    if (this.audioPlayer) {
      this.audioPlayer.nativeElement.currentTime = value;
    }
    this.seek.emit(value);
  }

  onVolumeChange(event: Event) {
    const value = Number((event.target as HTMLInputElement).value);
    if (!this.muted || value > 0) {
      this.lastVolumeBeforeMute = value;
    }
    this.muted = value === 0;
    this.volumeChange.emit(value);
  }

  onReplayClick() {
    this.replay.emit();
  }

  onShuffleClick() {
    this.shuffle.emit();
  }

  onMuteToggle() {
    if (this.muted) {
      this.muted = false;
      this.volumeChange.emit(this.lastVolumeBeforeMute || this.volume);
    } else {
      this.muted = true;
      this.lastVolumeBeforeMute = this.volume;
      this.muteToggle.emit();
    }
  }

  onPreviousClick() {
    this.previous.emit();
  }

  onNextClick() {
    this.next.emit();
  }

  onLikeClick() {
    if (this.currentSongId) {
      this.toggleFavorite.emit();
    }
  }

  onToggleLyrics() {
    this.toggleLyrics.emit();
  }

  onAddToPlaylistClick(event: Event) {
    event.stopPropagation();
    if (!this.currentSongId) return;
    
    // Calculate popup position near the button
    if (this.addToPlaylistButton) {
      const buttonRect = this.addToPlaylistButton.nativeElement.getBoundingClientRect();
      this.popupLeft = buttonRect.left;
      this.popupTop = buttonRect.top - 10; // Position above the button
    }
    
    this.showPlaylistPopup = true;
    this.loadPlaylists();
  }

  closePlaylistPopup() {
    this.showPlaylistPopup = false;
    this.playlists = [];
    this.playlistSongMap.clear();
  }

  onPopupBackdropClick(event: Event) {
    if ((event.target as HTMLElement).classList.contains('playlist-popup')) {
      this.closePlaylistPopup();
    }
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: KeyboardEvent) {
    if (this.showPlaylistPopup) {
      this.closePlaylistPopup();
    }
  }

  loadPlaylists() {
    this.isLoadingPlaylists = true;
    this.playlistsService.getPlaylists().subscribe({
      next: (playlists) => {
        this.playlists = playlists;
        this.hasPlaylists = playlists.length > 0;
        // Check which playlists already have this song
        if (this.currentSongId) {
          this.checkPlaylistsForSong(playlists);
        }
        this.isLoadingPlaylists = false;
        
        // Adjust popup position after content loads
        setTimeout(() => {
          this.adjustPopupPosition();
        }, 0);
      },
      error: (error) => {
        console.error('Error loading playlists:', error);
        this.isLoadingPlaylists = false;
      }
    });
  }

  adjustPopupPosition() {
    if (!this.addToPlaylistButton || !this.playlistPopupContent) return;
    
    const buttonRect = this.addToPlaylistButton.nativeElement.getBoundingClientRect();
    const popupRect = this.playlistPopupContent.nativeElement.getBoundingClientRect();
    
    // Position popup above the button, aligned to the right
    this.popupTop = buttonRect.top - popupRect.height - 10;
    this.popupLeft = buttonRect.right - popupRect.width;
    
    // Ensure popup doesn't go off screen
    if (this.popupLeft < 10) {
      this.popupLeft = 10;
    }
    if (this.popupTop < 10) {
      this.popupTop = buttonRect.bottom + 10; // Show below if not enough space above
    }
  }

  checkPlaylistsForSong(playlists: Playlist[]) {
    // Load each playlist detail to check if song exists
    playlists.forEach(playlist => {
      this.playlistsService.getPlaylist(playlist.playlistId).subscribe({
        next: (detail) => {
          const hasSong = detail.songs.some(song => song.songId === this.currentSongId);
          this.playlistSongMap.set(playlist.playlistId, hasSong);
        },
        error: () => {
          this.playlistSongMap.set(playlist.playlistId, false);
        }
      });
    });
  }

  playlistHasSong(playlistId: number): boolean {
    return this.playlistSongMap.get(playlistId) || false;
  }

  onPlaylistItemClick(playlist: Playlist) {
    if (!this.currentSongId) return;

    const hasSong = this.playlistHasSong(playlist.playlistId);
    
    if (hasSong) {
      // Remove song from playlist
      this.playlistsService.removeSongFromPlaylist(playlist.playlistId, this.currentSongId).subscribe({
        next: () => {
          // Update the map to reflect the song is no longer in the playlist
          this.playlistSongMap.set(playlist.playlistId, false);
          // Update song count
          playlist.songCount = Math.max(0, playlist.songCount - 1);
        },
        error: (error) => {
          console.error('Error removing song from playlist:', error);
        }
      });
    } else {
      // Add song to playlist
      this.playlistsService.addSongToPlaylist(playlist.playlistId, this.currentSongId).subscribe({
        next: () => {
          // Update the map to reflect the song is now in the playlist
          this.playlistSongMap.set(playlist.playlistId, true);
          // Update song count
          playlist.songCount++;
        },
        error: (error) => {
          console.error('Error adding song to playlist:', error);
        }
      });
    }
  }
}

