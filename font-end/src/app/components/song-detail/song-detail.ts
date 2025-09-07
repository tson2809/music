import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, takeUntil, combineLatest } from 'rxjs';
import { SongService } from '../../services/song.service';
import { AuthService } from '../../services/auth.service';
import { PlayerService } from '../../services/player.service';
import { FavoritesService } from '../../services/favorites.service';
import { User } from '../../models/user.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { PlayerBarComponent, CurrentTrack } from '../player-bar/player-bar.component';

interface SongDetail {
  songId: number;
  songTitle: string;
  artistId: number;
  artistName: string;
  albumId?: number;
  albumTitle?: string;
  genreId?: number;
  genreName?: string;
  audioFileUrl: string;
  coverImageUrl?: string;
  durationSeconds: number;
  releaseDate?: string;
  lyrics?: string;
  playCount: number;
  likeCount: number;
  createdAt: string;
}

@Component({
  selector: 'app-song-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent, SearchHeaderComponent, PlayerBarComponent],
  templateUrl: './song-detail.html',
  styleUrls: ['./song-detail.css']
})
export class SongDetailComponent implements OnInit, OnDestroy {
  song: SongDetail | null = null;
  songId: number = 0;
  loading = false;
  errorMessage = '';

  // Processed lyrics for display (ẩn timestamp nếu có)
  displayLyrics: string | null = null;
  
  // For layout components
  currentUser: User | null = null;
  currentTrack: CurrentTrack = {
    name: 'Chọn bài hát để phát',
    artist: 'Nghệ sĩ'
  };
  progress = 0;
  volume = 70;
  
  // Player state
  currentSongId: number | null = null;
  isPlaying = false;
  isLiked = false; // Trạng thái like (không phải favorites)
  isFavorited = false; // Trạng thái yêu thích
  
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private songService: SongService,
    private authService: AuthService,
    private playerService: PlayerService,
    private favoritesService: FavoritesService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
      if (this.currentUser) {
        // Load favorites when user is logged in
        this.favoritesService.loadFavorites();
      }
    });

    // Subscribe to favorite changes
    this.favoritesService.favoriteSongIds$.pipe(takeUntil(this.destroy$)).subscribe(favoriteIds => {
      if (this.songId) {
        this.isFavorited = favoriteIds.has(this.songId);
        console.log('Favorite status updated from service:', this.isFavorited);
      }
    });

    this.route.params.subscribe(params => {
      this.songId = +params['id'];
      if (this.songId) {
        this.loadSong();
        // Check if this song is favorited
        this.isFavorited = this.favoritesService.isFavorite(this.songId);
      }
    });

    // Subscribe to player state
    combineLatest([
      this.playerService.currentSongId$,
      this.playerService.isPlaying$
    ]).pipe(takeUntil(this.destroy$)).subscribe(([songId, playing]) => {
      this.currentSongId = songId;
      this.isPlaying = playing;
      
      if (this.song && songId === this.song.songId) {
        this.currentTrack = {
          name: this.song.songTitle,
          artist: this.song.artistName
        };
      }
    });

    // Cập nhật playCount khi PlayerService báo đã update
    this.playerService.playCountUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(update => {
        if (update && this.song && update.songId === this.song.songId) {
          this.song.playCount = update.playCount;
          this.cdr.markForCheck();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadSong(): void {
    this.loading = true;
    this.errorMessage = '';

    this.songService.getSongDetail(this.songId).subscribe({
      next: (response) => {
        this.song = response;
        this.loading = false;
        
        if (this.song) {
          this.currentTrack = {
            name: this.song.songTitle,
            artist: this.song.artistName
          };
          
          // Load like status nếu đã đăng nhập
          if (this.currentUser) {
            this.loadLikeStatus();
          }
        }
      },
      error: (error) => {
        console.error('Error loading song:', error);
        this.loading = false;
        this.errorMessage = error.error?.message || 'Không thể tải thông tin bài hát';
      }
    });
  }

  /**
   * Dùng cho template: trả về lyrics đã ẩn timestamp nếu có.
   * Nếu không có timestamp thì trả về nguyên văn.
   */
  getLyricsForDisplay(): string {
    const raw = this.song?.lyrics || '';
    if (!raw) return '';

    // Regex bắt các timestamp dạng [mm:ss.xx] hoặc [m:ss.xx] ở bất kỳ vị trí nào trong dòng
    const pattern = /\[\d{1,2}:\d{1,2}\.\d{2}\]/g;
    const stripped = raw.replace(pattern, '');

    return stripped;
  }

  /**
   * Ẩn phần timestamp trong lyrics nếu có (format LRC: [mm:ss.xx] hoặc [m:ss.xx])
   * Trả về chuỗi lyrics chỉ còn phần chữ.
   */
  private processLyrics(rawLyrics: string): string {
    if (!rawLyrics) return '';

    const lines = rawLyrics.split('\n');
    const lrcPattern = /^\[(\d{1,2}):(\d{1,2})\.(\d{2})\](.*)$/;

    let hasTimestamp = false;
    const processedLines = lines.map(line => {
      const match = line.match(lrcPattern);
      if (match) {
        hasTimestamp = true;
        // Phần lời là group 4, có thể rỗng (khoảng nghỉ)
        return match[4].trim();
      }
      return line;
    });

    // Nếu không có timestamp nào thì trả về nguyên bản
    if (!hasTimestamp) {
      return rawLyrics;
    }

    // Nếu có timestamp, join lại, vẫn giữ dòng trống/khoảng nghỉ
    return processedLines.join('\n');
  }

  loadLikeStatus(): void {
    if (!this.song || !this.currentUser) {
      this.isLiked = false;
      return;
    }

    this.songService.checkIfSongIsLiked(this.song.songId).subscribe({
      next: (isLiked) => {
        this.isLiked = isLiked;
      },
      error: (error) => {
        console.error('Error loading like status:', error);
        // Nếu lỗi (có thể do chưa đăng nhập), set isLiked = false
        this.isLiked = false;
      }
    });
  }

  onToggleLike(): void {
    if (!this.song) {
      return;
    }

    if (!this.currentUser) {
      // Nếu chưa đăng nhập, redirect đến login
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: `/songs/${this.songId}` }
      });
      return;
    }

    // Toggle like (tăng/giảm likeCount)
    if (this.isLiked) {
      // Unlike - giảm likeCount
      this.songService.unlikeSong(this.song.songId).subscribe({
        next: (response) => {
          console.log('Unlike success:', response); // DEBUG
          this.isLiked = false;
          if (this.song && response.likeCount !== undefined) {
            this.song.likeCount = response.likeCount;
          } else if (this.song) {
            this.song.likeCount = Math.max(0, this.song.likeCount - 1);
          }
        },
        error: (error) => {
          console.error('Error unliking song:', error); // DEBUG
          this.errorMessage = error.error?.message || 'Không thể bỏ thích bài hát. Vui lòng thử lại!';
          setTimeout(() => this.errorMessage = '', 3000);
          // Nếu lỗi, reload like status
          this.loadLikeStatus();
        }
      });
    } else {
      // Like - tăng likeCount
      this.songService.likeSong(this.song.songId).subscribe({
        next: (response) => {
          console.log('Like success:', response); // DEBUG
          this.isLiked = true;
          if (this.song && response.likeCount !== undefined) {
            this.song.likeCount = response.likeCount;
          } else if (this.song) {
            this.song.likeCount = (this.song.likeCount || 0) + 1;
          }
        },
        error: (error) => {
          console.error('Error liking song:', error); // DEBUG
          this.errorMessage = error.error?.message || 'Không thể thích bài hát. Vui lòng thử lại!';
          setTimeout(() => this.errorMessage = '', 3000);
          // Nếu lỗi (có thể đã like rồi), reload like status
          this.loadLikeStatus();
        }
      });
    }
  }

  onToggleFavorite(): void {
    console.log('onToggleFavorite called, current isFavorited:', this.isFavorited);
    
    if (!this.song) {
      console.log('No song loaded');
      return;
    }

    if (!this.currentUser) {
      console.log('User not logged in, redirecting to login');
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: `/songs/${this.songId}` }
      });
      return;
    }

    if (this.isFavorited) {
      // Remove from favorites
      console.log('Removing from favorites, songId:', this.song.songId);
      this.favoritesService.removeFromFavorites(this.song.songId).subscribe({
        next: () => {
          console.log('Successfully removed from favorites');
        },
        error: (error) => {
          console.error('Error removing from favorites:', error);
          this.errorMessage = error.error?.message || 'Không thể bỏ yêu thích. Vui lòng thử lại!';
          setTimeout(() => this.errorMessage = '', 3000);
        }
      });
    } else {
      // Add to favorites
      console.log('Adding to favorites, songId:', this.song.songId);
      this.favoritesService.addToFavorites(this.song.songId).subscribe({
        next: () => {
          console.log('Successfully added to favorites');
        },
        error: (error) => {
          console.error('Error adding to favorites:', error);
          this.errorMessage = error.error?.message || 'Không thể thêm vào yêu thích. Vui lòng thử lại!';
          setTimeout(() => this.errorMessage = '', 3000);
        }
      });
    }
  }

  onPlayClick(): void {
    if (!this.song) return;
    
    if (this.currentSongId === this.song.songId && this.isPlaying) {
      this.playerService.togglePlayback();
    } else {
      this.playerService.playSong({
        songId: this.song.songId,
        songTitle: this.song.songTitle,
        artistName: this.song.artistName,
        audioFileUrl: this.song.audioFileUrl,
        coverImageUrl: this.song.coverImageUrl,
        durationSeconds: this.song.durationSeconds
      });
    }
  }

  formatDuration(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
  }

  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
