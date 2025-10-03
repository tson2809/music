import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AlbumService } from '../../services/album.service';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { PlayerBarComponent, CurrentTrack } from '../player-bar/player-bar.component';

interface AlbumDetail {
  albumId: number;
  albumTitle: string;
  artistId: number;
  artistName: string;
  releaseDate?: string;
  albumType: string;
  coverImageUrl?: string;
  totalTracks: number;
  durationSeconds: number;
  createdAt: string;
}

interface AlbumListResponse {
  albums: AlbumDetail[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Component({
  selector: 'app-admin-album-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SidebarComponent, SearchHeaderComponent, PlayerBarComponent],
  templateUrl: './admin-album-management.component.html',
  styleUrls: ['./admin-album-management.component.css']
})
export class AdminAlbumManagementComponent implements OnInit {
  albums: AlbumDetail[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalPages = 0;
  totalCount = 0;
  
  // Search
  searchQuery = '';
  
  // For layout components
  currentUser: User | null = null;
  progress = 0;
  volume = 70;
  currentTrack: CurrentTrack = {
    name: 'Chọn bài hát để phát',
    artist: 'Nghệ sĩ'
  };

  constructor(
    private albumService: AlbumService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Get current user
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
    });

    this.loadAlbums();
  }

  loadAlbums(): void {
    this.loading = true;
    this.errorMessage = '';
    
    this.albumService.getAllAlbums(this.currentPage, this.pageSize, this.searchQuery).subscribe({
      next: (response: AlbumListResponse) => {
        this.albums = response.albums;
        this.totalPages = response.totalPages;
        this.totalCount = response.totalCount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading albums:', error);
        this.errorMessage = 'Không thể tải danh sách album';
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadAlbums();
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.currentPage = 1;
    this.loadAlbums();
  }

  deleteAlbum(album: AlbumDetail): void {
    if (!confirm(`Bạn có chắc chắn muốn xóa album "${album.albumTitle}"? Tất cả bài hát trong album cũng sẽ bị xóa.`)) {
      return;
    }

    this.albumService.deleteAlbumByAdmin(album.albumId).subscribe({
      next: (response) => {
        this.successMessage = 'Xóa album thành công!';
        this.loadAlbums();
        
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (error) => {
        console.error('Error deleting album:', error);
        this.errorMessage = error.error?.message || 'Không thể xóa album';
        
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

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadAlbums();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadAlbums();
    }
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.loadAlbums();
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
}

