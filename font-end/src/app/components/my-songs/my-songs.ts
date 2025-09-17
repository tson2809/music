import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { SongService } from '../../services/song.service';
import { AuthService } from '../../services/auth.service';
import { PlayerService } from '../../services/player.service';
import { User } from '../../models/user.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { Subscription, combineLatest } from 'rxjs';

@Component({
  selector: 'app-my-songs',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SidebarComponent, SearchHeaderComponent],
  templateUrl: './my-songs.html',
  styleUrls: ['./my-songs.css']
})
export class MySongsComponent implements OnInit, OnDestroy {
  mySongs: any[] = [];
  currentPage = 1;
  pageSize = 20;
  totalPages = 0;
  totalCount = 0;
  selectedStatus: number | null = null; // null = all, 0 = Pending, 1 = Approved, 2 = Rejected
  
  isLoading = false;
  errorMessage = '';

  // Layout components
  currentUser: User | null = null;

  // Player state (từ PlayerService)
  currentSongId: number | null = null;
  isPlaying = false;

  private subscriptions = new Subscription();

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

    // Subscribe to player state từ PlayerService
    const playerSub = combineLatest([
      this.playerService.currentSongId$,
      this.playerService.isPlaying$
    ]).subscribe(([songId, playing]) => {
      this.currentSongId = songId;
      this.isPlaying = playing;
    });
    this.subscriptions.add(playerSub);

    this.loadMySongs();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadMySongs(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    const statusFilter = this.selectedStatus !== null ? this.selectedStatus : undefined;
    
    this.songService.getMySongs(this.currentPage, this.pageSize, statusFilter).subscribe({
      next: (response) => {
        this.mySongs = response.songs;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading my songs:', error);
        this.errorMessage = 'Không thể tải danh sách bài hát của bạn';
        this.isLoading = false;
      }
    });
  }

  onStatusFilterChange(): void {
    this.currentPage = 1;
    this.loadMySongs();
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Pending': return 'Chờ duyệt';
      case 'Approved': return 'Đã duyệt';
      case 'Rejected': return 'Bị từ chối';
      default: return status;
    }
  }

  getStatusClass(status: string): string {
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
      this.loadMySongs();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadMySongs();
    }
  }

  formatDuration(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
  }

  getCountByStatus(status: number): number {
    if (!this.mySongs) return 0;
    
    const statusMap: { [key: number]: string } = {
      0: 'Pending',
      1: 'Approved',
      2: 'Rejected'
    };
    
    return this.mySongs.filter(song => song.approvalStatus === statusMap[status]).length;
  }

  onSongPlayClick(song: any): void {
    // Kiểm tra đăng nhập trước khi phát nhạc
    if (!this.currentUser) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: '/my-songs' }
      });
      return;
    }

    // Chỉ cho phát bài đã được duyệt
    if (song.approvalStatus !== 'Approved') {
      return;
    }

    const currentSongId = this.playerService.getCurrentSongId();
    if (currentSongId === song.songId) {
      // Nếu đang phát bài này, toggle play/pause
      this.playerService.togglePlayback();
      return;
    }

    // Phát bài hát với danh sách bài hát đã duyệt làm playlist
    const approvedSongs = this.mySongs.filter(s => s.approvalStatus === 'Approved');
    this.playerService.playSong(song, approvedSongs);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
