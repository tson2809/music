import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { SongService } from '../../services/song.service';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { PlayerBarComponent, CurrentTrack } from '../player-bar/player-bar.component';

@Component({
  selector: 'app-song-approval',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SidebarComponent, SearchHeaderComponent, PlayerBarComponent],
  templateUrl: './song-approval.html',
  styleUrls: ['./song-approval.css']
})
export class SongApprovalComponent implements OnInit {
  pendingSongs: any[] = [];
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;
  searchText = '';
  
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  
  // Modal cho rejection
  showRejectModal = false;
  selectedSongId: number | null = null;
  rejectionReason = '';

  // Layout components
  currentUser: User | null = null;
  currentTrack: CurrentTrack = {
    name: 'Chọn bài hát để phát',
    artist: 'Nghệ sĩ'
  };

  constructor(
    private songService: SongService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Get current user
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
    });

    this.loadPendingSongs();
  }

  loadPendingSongs(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.songService.getPendingSongs(this.currentPage, this.pageSize, this.searchText).subscribe({
      next: (response) => {
        this.pendingSongs = response.songs;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading pending songs:', error);
        this.errorMessage = 'Không thể tải danh sách bài hát chờ duyệt';
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadPendingSongs();
  }

  clearSearch(): void {
    this.searchText = '';
    this.currentPage = 1;
    this.loadPendingSongs();
  }

  approveSong(songId: number): void {
    if (!confirm('Bạn có chắc chắn muốn duyệt bài hát này?')) {
      return;
    }

    this.songService.approveSong(songId).subscribe({
      next: () => {
        this.successMessage = 'Duyệt bài hát thành công!';
        setTimeout(() => this.successMessage = '', 3000);
        this.loadPendingSongs(); // Reload danh sách
      },
      error: (error) => {
        console.error('Error approving song:', error);
        this.errorMessage = 'Không thể duyệt bài hát';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  openRejectModal(songId: number): void {
    this.selectedSongId = songId;
    this.rejectionReason = '';
    this.showRejectModal = true;
  }

  closeRejectModal(): void {
    this.showRejectModal = false;
    this.selectedSongId = null;
    this.rejectionReason = '';
  }

  confirmReject(): void {
    if (!this.selectedSongId) return;

    this.songService.rejectSong(this.selectedSongId, this.rejectionReason).subscribe({
      next: () => {
        this.successMessage = 'Từ chối bài hát thành công!';
        setTimeout(() => this.successMessage = '', 3000);
        this.closeRejectModal();
        this.loadPendingSongs(); // Reload danh sách
      },
      error: (error) => {
        console.error('Error rejecting song:', error);
        this.errorMessage = 'Không thể từ chối bài hát';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadPendingSongs();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadPendingSongs();
    }
  }

  formatDuration(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
