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

interface ArtistSong {
  songId: number;
  songTitle: string;
  artistId: number;
  artistName: string;
  albumId?: number;
  albumTitle?: string;
  genreId?: number;
  genreName?: string;
  durationSeconds: number;
  playCount: number;
  likeCount: number;
  approvalStatus: string;
  createdAt: string;
  releaseDate?: string;
  coverImageUrl?: string | null;
}

@Component({
  selector: 'app-artist-statistics',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    SidebarComponent,
    SearchHeaderComponent,
    PlayerBarComponent
  ],
  templateUrl: './artist-statistics.component.html',
  styleUrls: ['./artist-statistics.component.css']
})
export class ArtistStatisticsComponent implements OnInit {
  // Song statistics tabs
  selectedTab: 'listens' | 'likes' | 'trending' = 'listens';
  
  // Songs data
  allSongs: ArtistSong[] = [];
  topSongsByListens: ArtistSong[] = [];
  topSongsByLikes: ArtistSong[] = [];
  trendingSongs: ArtistSong[] = [];
  
  loading = false;
  loadingTopSongs = false;
  errorMessage = '';
  
  // For layout components
  currentUser: User | null = null;
  progress = 0;
  volume = 70;
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
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
      // Verify user is an artist
      if (!state.user || !state.user.artistId) {
        this.router.navigate(['/home']);
      }
    });

    this.loadAllSongs();
  }

  loadAllSongs(): void {
    this.loading = true;
    this.loadingTopSongs = true;
    this.errorMessage = '';
    
    // Load all songs of the artist (without status filter to get all)
    this.songService.getMySongs(1, 1000).subscribe({
      next: (response) => {
        this.allSongs = (response.songs || []).map((song: any) => ({
          songId: song.songId,
          songTitle: song.songTitle,
          artistId: song.artistId,
          artistName: song.artistName,
          albumId: song.albumId,
          albumTitle: song.albumTitle,
          genreId: song.genreId,
          genreName: song.genreName,
          durationSeconds: song.durationSeconds,
          playCount: song.playCount || 0,
          likeCount: song.likeCount || 0,
          approvalStatus: song.approvalStatus,
          createdAt: song.createdAt,
          releaseDate: song.releaseDate,
          coverImageUrl: song.coverImageUrl
        }));
        
        this.processSongStatistics();
        this.loading = false;
        this.loadingTopSongs = false;
      },
      error: (error) => {
        console.error('Error loading songs:', error);
        this.errorMessage = 'Không thể tải dữ liệu thống kê';
        this.loading = false;
        this.loadingTopSongs = false;
      }
    });
  }

  processSongStatistics(): void {
    // Sort by playCount and take top 10
    this.topSongsByListens = [...this.allSongs]
      .filter(song => song.approvalStatus === 'Approved') // Only show approved songs
      .sort((a, b) => (b.playCount || 0) - (a.playCount || 0))
      .slice(0, 10);
    
    // Sort by likeCount and take top 10
    this.topSongsByLikes = [...this.allSongs]
      .filter(song => song.approvalStatus === 'Approved')
      .sort((a, b) => (b.likeCount || 0) - (a.likeCount || 0))
      .slice(0, 10);
    
    // Calculate trending songs (songs with good combination of plays and likes)
    // Trending score = (playCount * 0.7) + (likeCount * 100 * 0.3)
    this.trendingSongs = [...this.allSongs]
      .filter(song => song.approvalStatus === 'Approved' && ((song.playCount || 0) > 0 || (song.likeCount || 0) > 0))
      .map(song => ({
        ...song,
        trendingScore: (song.playCount || 0) * 0.7 + (song.likeCount || 0) * 100 * 0.3
      }))
      .sort((a: any, b: any) => b.trendingScore - a.trendingScore)
      .slice(0, 10)
      .map(({ trendingScore, ...song }: any) => song); // Remove trendingScore before display
  }

  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  formatDuration(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  formatDate(dateString?: string | null): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
  }

  getTotalListens(): number {
    return this.topSongsByListens.reduce((total, song) => total + (song.playCount || 0), 0);
  }

  getTotalLikes(): number {
    return this.topSongsByLikes.reduce((total, song) => total + (song.likeCount || 0), 0);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

