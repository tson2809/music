import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FavoritesService, FavoriteSong } from '../../services/favorites.service';
import { AuthService } from '../../services/auth.service';
import { PlayerService } from '../../services/player.service';
import { User } from '../../models/user.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { Subject, combineLatest, takeUntil } from 'rxjs';

@Component({
  selector: 'app-favorites',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent, SearchHeaderComponent],
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.css']
})
export class FavoritesComponent implements OnInit, OnDestroy {
  favoriteSongs: FavoriteSong[] = [];
  loading = false;
  errorMessage = '';
  
  // For layout components
  currentUser: User | null = null;
  
  // Player state
  currentSongId: number | null = null;
  isPlaying = false;
  private destroy$ = new Subject<void>();

  constructor(
    private favoritesService: FavoritesService,
    private authService: AuthService,
    private router: Router,
    private playerService: PlayerService
  ) {}

  ngOnInit(): void {
    // Get current user
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
    });

    // Subscribe to player state
    combineLatest([
      this.playerService.currentSongId$,
      this.playerService.isPlaying$
    ]).pipe(takeUntil(this.destroy$)).subscribe(([songId, playing]) => {
      this.currentSongId = songId;
      this.isPlaying = playing;
    });

    this.loadFavoriteSongs();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadFavoriteSongs(): void {
    this.loading = true;
    this.errorMessage = '';

    // Debug: Log current user
    const currentUser = this.authService.getCurrentUser();
    console.log('Current user:', currentUser);
    console.log('User ID:', currentUser?.userId);

    this.favoritesService.getFavoriteSongs().subscribe({
      next: (songs) => {
        console.log('Favorite songs received:', songs);
        console.log('Number of favorite songs:', songs.length);
        this.favoriteSongs = songs;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading favorite songs:', error);
        console.error('Error status:', error.status);
        console.error('Error message:', error.error);
        this.errorMessage = error.error?.message || `Không thể tải danh sách yêu thích (Status: ${error.status})`;
        this.loading = false;
      }
    });
  }

  removeFromFavorites(song: FavoriteSong): void {
    if (!confirm(`Bạn có chắc chắn muốn xóa "${song.songTitle}" khỏi danh sách yêu thích?`)) {
      return;
    }

    this.favoritesService.removeFromFavorites(song.songId).subscribe({
      next: () => {
        // Remove from local array
        this.favoriteSongs = this.favoriteSongs.filter(s => s.songId !== song.songId);
        // Refresh player service favorites
        this.playerService.refreshFavorites();
      },
      error: (error) => {
        console.error('Error removing from favorites:', error);
        this.errorMessage = error.error?.message || 'Không thể xóa khỏi yêu thích';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  formatDuration(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${minutes}:${secs.toString().padStart(2, '0')}`;
  }

  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  playSong(song: FavoriteSong): void {
    // Kiểm tra xem có đang phát bài này không
    if (this.currentSongId === song.songId && this.isPlaying) {
      // Nếu đang phát, thì pause
      this.playerService.togglePlayback();
      return;
    }

    if (this.currentSongId === song.songId && !this.isPlaying) {
      // Nếu đang pause bài này, thì resume
      this.playerService.togglePlayback();
      return;
    }

    // Convert FavoriteSong to Song-like object for PlayerService
    const playableSong = {
      songId: song.songId,
      songTitle: song.songTitle,
      artistName: song.artistName,
      audioFileUrl: song.audioFileUrl,
      coverImageUrl: song.coverImageUrl,
      durationSeconds: song.durationSeconds
    };
    // Play song with the favorite songs list as the playlist
    this.playerService.playSong(playableSong, this.favoriteSongs);
    // Reset playingPlaylistId because we're playing from favorites page, not from a specific playlist
    this.playerService.setPlayingPlaylistId(null);
  }

  isPlayingSong(songId: number): boolean {
    return this.currentSongId === songId && this.isPlaying;
  }

  navigateToArtist(song: FavoriteSong, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/artists', song.artistId]);
  }

  navigateToSong(song: FavoriteSong, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.router.navigate(['/songs', song.songId]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

