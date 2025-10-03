import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AlbumService } from '../../services/album.service';
import { SongService } from '../../services/song.service';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { Album, AlbumDetail, AlbumListResponse, AlbumSong } from '../../models/album.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';

interface MySongOption {
  songId: number;
  songTitle: string;
  albumId?: number | null;
  approvalStatus?: string;
  durationSeconds?: number;
}

@Component({
  selector: 'app-album-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SidebarComponent, SearchHeaderComponent],
  templateUrl: './album-management.component.html',
  styleUrls: ['./album-management.component.css']
})
export class AlbumManagementComponent implements OnInit {
  albums: Album[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';

  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;
  searchQuery = '';

  showCreateModal = false;
  showEditModal = false;
  showManageSongsModal = false;

  editingAlbum: Album | null = null;
  selectedAlbum: Album | null = null;
  selectedAlbumDetail: AlbumDetail | null = null;
  loadingAlbumDetail = false;

  availableSongs: MySongOption[] = [];
  loadingAvailableSongs = false;
  selectedSongId: number | null = null;
  addingSong = false;
  removingSongId: number | null = null;

  formData = {
    albumTitle: '',
    releaseDate: '',
    albumType: 'album',
    coverImageFile: null as File | null
  };

  currentUser: User | null = null;
  currentArtistId: number | null = null;

  constructor(
    private albumService: AlbumService,
    private songService: SongService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
      this.currentArtistId = state.user?.artistId || null;

      if (!this.currentArtistId) {
        this.errorMessage = 'Bạn cần đăng nhập bằng tài khoản nghệ sĩ để quản lý album';
        return;
      }

      this.loadAlbums();
    });
  }

  loadAlbums(): void {
    if (!this.currentArtistId) {
      return;
    }

    this.loading = true;
    this.albumService.getAlbums(this.currentPage, this.pageSize, this.currentArtistId, this.searchQuery).subscribe({
      next: (response: AlbumListResponse) => {
        this.albums = response.albums;
        this.totalPages = response.totalPages;
        this.totalCount = response.totalCount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading albums:', error);
        this.errorMessage = error.error?.message || 'Không thể tải danh sách album';
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

  openCreateModal(): void {
    this.formData = {
      albumTitle: '',
      releaseDate: '',
      albumType: 'album',
      coverImageFile: null
    };
    this.showCreateModal = true;
    this.errorMessage = '';
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
  }

  onCoverSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.formData.coverImageFile = file;
    }
  }

  createAlbum(): void {
    if (!this.currentArtistId) {
      return;
    }
    if (!this.formData.albumTitle.trim()) {
      this.errorMessage = 'Vui lòng nhập tên album';
      return;
    }

    const request = {
      albumTitle: this.formData.albumTitle.trim(),
      artistId: this.currentArtistId,
      releaseDate: this.formData.releaseDate || undefined,
      albumType: this.formData.albumType,
      coverImageFile: this.formData.coverImageFile || undefined
    };

    this.albumService.createAlbum(request).subscribe({
      next: () => {
        this.successMessage = 'Tạo album thành công';
        this.closeCreateModal();
        this.loadAlbums();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        console.error('Error creating album:', error);
        this.errorMessage = error.error?.message || 'Không thể tạo album';
        setTimeout(() => this.errorMessage = '', 4000);
      }
    });
  }

  openEditModal(album: Album, event?: MouseEvent): void {
    event?.stopPropagation();
    this.editingAlbum = album;
    this.formData = {
      albumTitle: album.albumTitle,
      releaseDate: album.releaseDate ? album.releaseDate.split('T')[0] : '',
      albumType: album.albumType,
      coverImageFile: null
    };
    this.showEditModal = true;
    this.errorMessage = '';
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.editingAlbum = null;
  }

  updateAlbum(): void {
    if (!this.editingAlbum) {
      return;
    }
    if (!this.formData.albumTitle.trim()) {
      this.errorMessage = 'Vui lòng nhập tên album';
      return;
    }

    const request = {
      albumTitle: this.formData.albumTitle.trim(),
      releaseDate: this.formData.releaseDate || undefined,
      albumType: this.formData.albumType,
      coverImageFile: this.formData.coverImageFile || undefined
    };

    this.albumService.updateAlbum(this.editingAlbum.albumId, request).subscribe({
      next: () => {
        this.successMessage = 'Cập nhật album thành công';
        this.closeEditModal();
        this.loadAlbums();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        console.error('Error updating album:', error);
        this.errorMessage = error.error?.message || 'Không thể cập nhật album';
        setTimeout(() => this.errorMessage = '', 4000);
      }
    });
  }

  deleteAlbum(album: Album, event?: MouseEvent): void {
    event?.stopPropagation();
    
    const hasSongs = album.totalTracks > 0;
    const confirmMessage = hasSongs 
      ? `Album "${album.albumTitle}" có ${album.totalTracks} bài hát. Các bài hát sẽ được gỡ khỏi album nhưng vẫn được giữ lại. Bạn có chắc muốn xóa album này?`
      : `Bạn có chắc muốn xóa album "${album.albumTitle}"?`;
    
    if (!confirm(confirmMessage)) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.albumService.deleteAlbum(album.albumId).subscribe({
      next: () => {
        this.loading = false;
        this.successMessage = hasSongs 
          ? `Đã xóa album. ${album.totalTracks} bài hát đã được gỡ khỏi album.`
          : 'Đã xóa album thành công';
        this.loadAlbums();
        setTimeout(() => this.successMessage = '', 5000);
      },
      error: (error) => {
        console.error('Error deleting album:', error);
        this.loading = false;
        this.errorMessage = error.error?.message || 'Không thể xóa album. Vui lòng thử lại.';
        setTimeout(() => this.errorMessage = '', 5000);
      }
    });
  }

  openManageSongsModal(album: Album, event?: MouseEvent): void {
    event?.stopPropagation();
    this.selectedAlbum = album;
    this.selectedAlbumDetail = null;
    this.availableSongs = [];
    this.selectedSongId = null;
    this.showManageSongsModal = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.loadAlbumDetail(album.albumId);
    this.loadAvailableSongs(album.albumId);
  }

  closeManageSongsModal(): void {
    this.showManageSongsModal = false;
    this.selectedAlbum = null;
    this.selectedAlbumDetail = null;
    this.availableSongs = [];
    this.selectedSongId = null;
    this.loadingAlbumDetail = false;
    this.loadingAvailableSongs = false;
    this.addingSong = false;
    this.removingSongId = null;
  }

  private loadAvailableSongs(albumId: number): void {
    this.loadingAvailableSongs = true;
    this.songService.getMySongs(1, 200).subscribe({
      next: (response) => {
        const songs: MySongOption[] = Array.isArray(response?.songs) ? response.songs : [];
        this.availableSongs = songs.filter(song =>
          !song.albumId || song.albumId === null
        );
        this.loadingAvailableSongs = false;
      },
      error: (error) => {
        console.error('Error loading available songs:', error);
        this.availableSongs = [];
        this.loadingAvailableSongs = false;
      }
    });
  }

  addSongToAlbum(): void {
    if (!this.selectedAlbum || this.selectedSongId === null) {
      this.errorMessage = 'Vui lòng chọn bài hát cần thêm';
      return;
    }

    const songId = Number(this.selectedSongId);
    if (!songId) {
      this.errorMessage = 'Lựa chọn bài hát không hợp lệ';
      return;
    }

    this.addingSong = true;
    this.songService.updateSongAlbum(songId, this.selectedAlbum.albumId).subscribe({
      next: () => {
        this.addingSong = false;
        this.successMessage = 'Đã thêm bài hát vào album';
        this.selectedSongId = null;
        this.loadAlbumDetail(this.selectedAlbum!.albumId);
        this.loadAvailableSongs(this.selectedAlbum!.albumId);
        this.loadAlbums();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        console.error('Error adding song:', error);
        this.errorMessage = error.error?.message || 'Không thể thêm bài hát vào album';
        this.addingSong = false;
        setTimeout(() => this.errorMessage = '', 4000);
      }
    });
  }

  removeSongFromAlbum(song: AlbumSong): void {
    if (!this.selectedAlbum) return;
    if (!confirm(`Bạn có chắc chắn muốn xóa "${song.songTitle}" khỏi album?`)) return;

    this.removingSongId = song.songId;
    this.songService.updateSongAlbum(song.songId, null).subscribe({
      next: () => {
        this.removingSongId = null;
        this.successMessage = 'Đã xóa bài hát khỏi album';
        this.loadAlbumDetail(this.selectedAlbum!.albumId);
        this.loadAvailableSongs(this.selectedAlbum!.albumId);
        this.loadAlbums();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        console.error('Error removing song:', error);
        this.errorMessage = error.error?.message || 'Không thể xóa bài hát khỏi album';
        this.removingSongId = null;
        setTimeout(() => this.errorMessage = '', 4000);
      }
    });
  }

  private loadAlbumDetail(albumId: number): void {
    this.loadingAlbumDetail = true;
    this.albumService.getAlbumById(albumId).subscribe({
      next: (detail: AlbumDetail) => {
        this.selectedAlbumDetail = detail;
        this.loadingAlbumDetail = false;
      },
      error: (error) => {
        console.error('Error loading album detail:', error);
        this.errorMessage = error.error?.message || 'Không thể tải chi tiết album';
        this.loadingAlbumDetail = false;
      }
    });
  }

  formatDuration(seconds?: number | null): string {
    if (!seconds || seconds <= 0) {
      return '--:--';
    }
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  formatDate(date?: string | null): string {
    if (!date) {
      return 'N/A';
    }
    return new Date(date).toLocaleDateString('vi-VN');
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

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  goToAlbum(album: Album): void {
    this.router.navigate(['/albums', album.albumId]);
  }
}

