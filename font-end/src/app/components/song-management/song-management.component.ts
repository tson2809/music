import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { SongService } from '../../services/song.service';
import { AuthService } from '../../services/auth.service';
import { PlayerService } from '../../services/player.service';
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
  playCount: number;
  likeCount: number;
  createdAt: string;
  approvalStatus?: string;
  approvedAt?: string;
  rejectionReason?: string;
}

interface SongListResponse {
  songs: SongDetail[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Component({
  selector: 'app-song-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SidebarComponent, SearchHeaderComponent, PlayerBarComponent],
  templateUrl: './song-management.component.html',
  styleUrls: ['./song-management.component.css']
})
export class SongManagementComponent implements OnInit {
  songs: SongDetail[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalPages = 0;
  totalCount = 0;
  
  // Search and Filter
  searchQuery = '';
  genres: any[] = [];
  selectedGenreId: number | null = null;
  
  // For layout components
  currentUser: User | null = null;
  progress = 0;
  volume = 70;
  currentTrack: CurrentTrack = {
    name: 'Chọn bài hát để phát',
    artist: 'Nghệ sĩ'
  };
  currentSongId: number | null = null;
  isPlaying = false;

  constructor(
    private songService: SongService,
    private authService: AuthService,
    private playerService: PlayerService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Get current user
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
    });

    // Subscribe to player state
    this.playerService.currentSongId$.subscribe(songId => {
      this.currentSongId = songId;
    });

    this.playerService.isPlaying$.subscribe(playing => {
      this.isPlaying = playing;
    });

    // Load genres
    this.loadGenres();

    // Load songs
    this.loadSongs();
  }

  loadSongs(): void {
    this.loading = true;
    this.errorMessage = '';
    
    this.songService.getAllSongs(this.currentPage, this.pageSize, this.searchQuery, this.selectedGenreId).subscribe({
      next: (response: SongListResponse) => {
        this.songs = response.songs;
        this.totalPages = response.totalPages;
        this.totalCount = response.totalCount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading songs:', error);
        this.errorMessage = 'Không thể tải danh sách bài hát';
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadSongs();
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.currentPage = 1;
    this.loadSongs();
  }

  loadGenres(): void {
    this.songService.getGenres().subscribe({
      next: (genres) => {
        this.genres = genres;
      },
      error: (error) => {
        console.error('Error loading genres:', error);
      }
    });
  }

  onGenreChange(): void {
    this.currentPage = 1;
    this.loadSongs();
  }

  deleteSong(song: SongDetail): void {
    if (!confirm(`Bạn có chắc chắn muốn xóa bài hát "${song.songTitle}"?`)) {
      return;
    }

    this.songService.deleteSong(song.songId).subscribe({
      next: (response) => {
        this.successMessage = 'Xóa bài hát thành công!';
        this.loadSongs();
        
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (error) => {
        console.error('Error deleting song:', error);
        this.errorMessage = error.error?.message || 'Không thể xóa bài hát';
        
        setTimeout(() => {
          this.errorMessage = '';
        }, 3000);
      }
    });
  }

  formatDuration(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${minutes}:${secs.toString().padStart(2, '0')}`;
  }

  formatDate(dateString?: string): string {
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

  getStatusLabel(status?: string): string {
    switch (status) {
      case 'Pending': return 'Chờ duyệt';
      case 'Approved': return 'Đã duyệt';
      case 'Rejected': return 'Bị từ chối';
      default: return status || 'N/A';
    }
  }

  getStatusClass(status?: string): string {
    switch (status) {
      case 'Pending': return 'status-pending';
      case 'Approved': return 'status-approved';
      case 'Rejected': return 'status-rejected';
      default: return '';
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadSongs();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadSongs();
    }
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.loadSongs();
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);
    
    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  onPlayClick(song: SongDetail): void {
    if (this.currentSongId === song.songId && this.isPlaying) {
      this.playerService.togglePlayback();
    } else {
      this.playerService.playSong({
        songId: song.songId,
        songTitle: song.songTitle,
        artistName: song.artistName,
        audioFileUrl: song.audioFileUrl,
        coverImageUrl: song.coverImageUrl,
        durationSeconds: song.durationSeconds
      });
    }
  }
}

