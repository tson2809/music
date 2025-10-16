import { Component, OnInit } from '@angular/core';
import { CommonModule, NgIf, NgFor } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StatisticsService, StatisticsOverview, User as StatisticsUser, UsersListResponse, Artist as StatisticsArtist, ArtistsListResponse, Song as StatisticsSong, SongsListResponse, Album as StatisticsAlbum, AlbumsListResponse } from '../../services/statistics.service';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { PlayerBarComponent, CurrentTrack } from '../player-bar/player-bar.component';

@Component({
  selector: 'app-statistics',
  standalone: true,
  imports: [CommonModule, NgIf, NgFor, FormsModule, RouterModule, SidebarComponent, SearchHeaderComponent, PlayerBarComponent],
  templateUrl: './statistics.component.html',
  styleUrls: ['./statistics.component.css']
})
export class StatisticsComponent implements OnInit {
  overview: StatisticsOverview | null = null;
  users: StatisticsUser[] = [];
  artists: StatisticsArtist[] = [];
  songs: StatisticsSong[] = [];
  albums: StatisticsAlbum[] = [];
  
  loading = false;
  loadingUsers = false;
  loadingArtists = false;
  loadingSongs = false;
  loadingAlbums = false;
  loadingTopSongs = false;
  topSongsLoaded = false;
  errorMessage = '';
  
  // Show/hide tables
  showUsersTable = false;
  showArtistsTable = false;
  showSongsTable = false;
  showAlbumsTable = false;
  
  // Song statistics tabs
  selectedTab: 'listens' | 'likes' | 'trending' = 'listens';
  topSongsByListens: StatisticsSong[] = [];
  topSongsByLikes: StatisticsSong[] = [];
  trendingSongs: StatisticsSong[] = [];
  
  // Pagination for users
  usersCurrentPage = 1;
  usersPageSize = 20;
  usersTotalPages = 0;
  usersTotalCount = 0;
  usersSearchQuery = '';
  
  // Pagination for artists
  artistsCurrentPage = 1;
  artistsPageSize = 20;
  artistsTotalPages = 0;
  artistsTotalCount = 0;
  artistsSearchQuery = '';
  
  // Pagination for songs
  songsCurrentPage = 1;
  songsPageSize = 20;
  songsTotalPages = 0;
  songsTotalCount = 0;
  songsSearchQuery = '';
  
  // Pagination for albums
  albumsCurrentPage = 1;
  albumsPageSize = 20;
  albumsTotalPages = 0;
  albumsTotalCount = 0;
  albumsSearchQuery = '';
  
  // For layout components
  currentUser: User | null = null;
  progress = 0;
  volume = 70;
  currentTrack: CurrentTrack = {
    name: 'Chọn bài hát để phát',
    artist: 'Nghệ sĩ'
  };

  constructor(
    private statisticsService: StatisticsService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
    });

    this.loadOverview();
  }

  loadOverview(): void {
    this.loading = true;
    this.statisticsService.getOverview().subscribe({
      next: (data) => {
        this.overview = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading overview:', error);
        this.errorMessage = 'Không thể tải thống kê tổng quan';
        this.loading = false;
      }
    });
  }

  toggleUsersTable(): void {
    if (this.loadingUsers || this.loadingArtists || this.loadingSongs || this.loadingAlbums) return; // Prevent clicks when any table is loading
    
    // Close other tables
    this.showArtistsTable = false;
    this.showSongsTable = false;
    this.showAlbumsTable = false;
    
    this.showUsersTable = !this.showUsersTable;
    if (this.showUsersTable) {
      if (this.users.length === 0) {
        this.loadUsers();
      } else {
        // Scroll to table after a short delay
        setTimeout(() => this.scrollToTable(), 100);
      }
    }
  }

  loadUsers(): void {
    this.loadingUsers = true;
    this.statisticsService.getUsers(this.usersCurrentPage, this.usersPageSize, this.usersSearchQuery || undefined).subscribe({
      next: (response: UsersListResponse) => {
        this.users = response.users;
        this.usersTotalPages = response.totalPages;
        this.usersTotalCount = response.totalCount;
        this.loadingUsers = false;
        setTimeout(() => this.scrollToTable(), 100);
      },
      error: (error) => {
        console.error('Error loading users:', error);
        this.loadingUsers = false;
      }
    });
  }

  onUsersSearch(): void {
    this.usersCurrentPage = 1;
    this.loadUsers();
  }

  clearUsersSearch(): void {
    this.usersSearchQuery = '';
    this.usersCurrentPage = 1;
    this.loadUsers();
  }

  usersPreviousPage(): void {
    if (this.usersCurrentPage > 1) {
      this.usersCurrentPage--;
      this.loadUsers();
    }
  }

  usersNextPage(): void {
    if (this.usersCurrentPage < this.usersTotalPages) {
      this.usersCurrentPage++;
      this.loadUsers();
    }
  }

  usersGoToPage(page: number): void {
    this.usersCurrentPage = page;
    this.loadUsers();
  }

  getUsersPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    
    let start = Math.max(1, this.usersCurrentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.usersTotalPages, start + maxVisible - 1);
    
    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  toggleArtistsTable(): void {
    if (this.loadingUsers || this.loadingArtists || this.loadingSongs || this.loadingAlbums) return; // Prevent clicks when any table is loading
    
    // Close other tables
    this.showUsersTable = false;
    this.showSongsTable = false;
    this.showAlbumsTable = false;
    
    this.showArtistsTable = !this.showArtistsTable;
    if (this.showArtistsTable) {
      if (this.artists.length === 0) {
        this.loadArtists();
      } else {
        setTimeout(() => this.scrollToTable(), 100);
      }
    }
  }

  loadArtists(): void {
    this.loadingArtists = true;
    this.statisticsService.getArtists(this.artistsCurrentPage, this.artistsPageSize, this.artistsSearchQuery || undefined).subscribe({
      next: (response: ArtistsListResponse) => {
        this.artists = response.artists;
        this.artistsTotalPages = response.totalPages;
        this.artistsTotalCount = response.totalCount;
        this.loadingArtists = false;
        setTimeout(() => this.scrollToTable(), 100);
      },
      error: (error) => {
        console.error('Error loading artists:', error);
        this.loadingArtists = false;
      }
    });
  }

  onArtistsSearch(): void {
    this.artistsCurrentPage = 1;
    this.loadArtists();
  }

  clearArtistsSearch(): void {
    this.artistsSearchQuery = '';
    this.artistsCurrentPage = 1;
    this.loadArtists();
  }

  artistsPreviousPage(): void {
    if (this.artistsCurrentPage > 1) {
      this.artistsCurrentPage--;
      this.loadArtists();
    }
  }

  artistsNextPage(): void {
    if (this.artistsCurrentPage < this.artistsTotalPages) {
      this.artistsCurrentPage++;
      this.loadArtists();
    }
  }

  artistsGoToPage(page: number): void {
    this.artistsCurrentPage = page;
    this.loadArtists();
  }

  getArtistsPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    
    let start = Math.max(1, this.artistsCurrentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.artistsTotalPages, start + maxVisible - 1);
    
    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  toggleSongsTable(): void {
    if (this.loadingUsers || this.loadingArtists || this.loadingSongs || this.loadingAlbums) return; // Prevent clicks when any table is loading
    
    // Close other tables
    this.showUsersTable = false;
    this.showArtistsTable = false;
    this.showAlbumsTable = false;
    
    this.showSongsTable = !this.showSongsTable;
    if (this.showSongsTable) {
      if (this.songs.length === 0) {
        this.loadSongs();
      } else {
        setTimeout(() => this.scrollToTable(), 100);
      }
    }
  }

  loadSongs(): void {
    this.loadingSongs = true;
    this.statisticsService.getSongs(this.songsCurrentPage, this.songsPageSize, this.songsSearchQuery || undefined).subscribe({
      next: (response: SongsListResponse) => {
        this.songs = response.songs;
        this.songsTotalPages = response.totalPages;
        this.songsTotalCount = response.totalCount;
        this.loadingSongs = false;
        setTimeout(() => this.scrollToTable(), 100);
      },
      error: (error) => {
        console.error('Error loading songs:', error);
        this.loadingSongs = false;
      }
    });
  }

  onSongsSearch(): void {
    this.songsCurrentPage = 1;
    this.loadSongs();
  }

  clearSongsSearch(): void {
    this.songsSearchQuery = '';
    this.songsCurrentPage = 1;
    this.loadSongs();
  }

  songsPreviousPage(): void {
    if (this.songsCurrentPage > 1) {
      this.songsCurrentPage--;
      this.loadSongs();
    }
  }

  songsNextPage(): void {
    if (this.songsCurrentPage < this.songsTotalPages) {
      this.songsCurrentPage++;
      this.loadSongs();
    }
  }

  songsGoToPage(page: number): void {
    this.songsCurrentPage = page;
    this.loadSongs();
  }

  getSongsPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    
    let start = Math.max(1, this.songsCurrentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.songsTotalPages, start + maxVisible - 1);
    
    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  toggleAlbumsTable(): void {
    if (this.loadingUsers || this.loadingArtists || this.loadingSongs || this.loadingAlbums) return; // Prevent clicks when any table is loading
    
    // Close other tables
    this.showUsersTable = false;
    this.showArtistsTable = false;
    this.showSongsTable = false;
    
    this.showAlbumsTable = !this.showAlbumsTable;
    if (this.showAlbumsTable) {
      if (this.albums.length === 0) {
        this.loadAlbums();
      } else {
        setTimeout(() => this.scrollToTable(), 100);
      }
    }
  }

  loadAlbums(): void {
    this.loadingAlbums = true;
    this.statisticsService.getAlbums(this.albumsCurrentPage, this.albumsPageSize, this.albumsSearchQuery || undefined).subscribe({
      next: (response: AlbumsListResponse) => {
        this.albums = response.albums;
        this.albumsTotalPages = response.totalPages;
        this.albumsTotalCount = response.totalCount;
        this.loadingAlbums = false;
        setTimeout(() => this.scrollToTable(), 100);
      },
      error: (error) => {
        console.error('Error loading albums:', error);
        this.loadingAlbums = false;
      }
    });
  }

  scrollToTable(): void {
    const firstTable = document.querySelector('.stats-section .table-container');
    if (firstTable) {
      firstTable.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  onAlbumsSearch(): void {
    this.albumsCurrentPage = 1;
    this.loadAlbums();
  }

  clearAlbumsSearch(): void {
    this.albumsSearchQuery = '';
    this.albumsCurrentPage = 1;
    this.loadAlbums();
  }

  albumsPreviousPage(): void {
    if (this.albumsCurrentPage > 1) {
      this.albumsCurrentPage--;
      this.loadAlbums();
    }
  }

  albumsNextPage(): void {
    if (this.albumsCurrentPage < this.albumsTotalPages) {
      this.albumsCurrentPage++;
      this.loadAlbums();
    }
  }

  albumsGoToPage(page: number): void {
    this.albumsCurrentPage = page;
    this.loadAlbums();
  }

  getAlbumsPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    
    let start = Math.max(1, this.albumsCurrentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.albumsTotalPages, start + maxVisible - 1);
    
    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  formatDuration(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  formatDate(dateString?: string | null): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
  }

  loadTopSongs(): void {
    this.loadingTopSongs = true;
    this.topSongsLoaded = false;
    // Load a large number of songs to get accurate top songs
    this.statisticsService.getSongs(1, 100).subscribe({
      next: (response: SongsListResponse) => {
        const allSongs = response.songs || [];
        
        // Sort by playCount and take top 10
        this.topSongsByListens = [...allSongs]
          .sort((a, b) => (b.playCount || 0) - (a.playCount || 0))
          .slice(0, 10);
        
        // Sort by likeCount and take top 10
        this.topSongsByLikes = [...allSongs]
          .sort((a, b) => (b.likeCount || 0) - (a.likeCount || 0))
          .slice(0, 10);
        
        // Calculate trending songs (songs with good combination of plays and likes)
        // Trending score = (playCount * 0.7) + (likeCount * 100 * 0.3)
        this.trendingSongs = [...allSongs]
          .filter(song => (song.playCount || 0) > 0 || (song.likeCount || 0) > 0)
          .map(song => ({
            ...song,
            trendingScore: (song.playCount || 0) * 0.7 + (song.likeCount || 0) * 100 * 0.3
          }))
          .sort((a: any, b: any) => b.trendingScore - a.trendingScore)
          .slice(0, 10)
          .map(({ trendingScore, ...song }) => song); // Remove trendingScore before display
        
        this.loadingTopSongs = false;
        this.topSongsLoaded = true;
      },
      error: (error) => {
        console.error('Error loading top songs:', error);
        this.loadingTopSongs = false;
        this.topSongsLoaded = false;
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

