import { Component, OnInit } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { AuthService } from '../../services/auth.service';
import { AlbumService } from '../../services/album.service';
import { User } from '../../models/user.model';
import { Album, AlbumListResponse } from '../../models/album.model';

@Component({
  selector: 'app-album-library',
  standalone: true,
  imports: [CommonModule, NgIf, NgFor, RouterModule, SidebarComponent, SearchHeaderComponent],
  templateUrl: './album-library.component.html',
  styleUrls: ['./album-library.component.css']
})
export class AlbumLibraryComponent implements OnInit {
  currentUser: User | null = null;

  albums: Album[] = [];
  isLoading = false;
  error: string | null = null;

  page = 1;
  pageSize = 24;
  totalPages = 1;
  searchTerm = '';

  constructor(
    private authService: AuthService,
    private albumService: AlbumService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
    });

    this.loadAlbums();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  loadAlbums(page: number = this.page): void {
    this.isLoading = true;
    this.error = null;

    this.albumService.getAllAlbumsPublic(page, this.pageSize, this.searchTerm).subscribe({
      next: (response: AlbumListResponse) => {
        this.albums = response?.albums ?? [];
        this.page = response?.page ?? page;
        this.pageSize = response?.pageSize ?? this.pageSize;
        this.totalPages = response?.totalPages ?? 1;
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Không thể tải danh sách album.';
        this.isLoading = false;
      }
    });
  }

  onSearchChange(value: string): void {
    this.searchTerm = value.trim();
    this.page = 1;
    this.loadAlbums(1);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.page) {
      return;
    }
    this.loadAlbums(page);
  }

  get pages(): number[] {
    const pages: number[] = [];
    for (let i = 1; i <= this.totalPages; i++) {
      pages.push(i);
    }
    return pages;
  }

  onAlbumClick(album: Album): void {
    // Điều hướng tới chi tiết album cho người dùng để xem danh sách bài hát
    this.router.navigate(['/albums-user', album.albumId]);
  }

  formatTracks(count: number): string {
    if (!count || Number.isNaN(count)) {
      return '0 bài hát';
    }
    return `${count} bài hát`;
  }
}


