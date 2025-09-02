import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { GenreService } from '../../services/genre.service';
import { Genre } from '../../models/genre.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { SongService } from '../../services/song.service';
import { Song } from '../../models/song.model';
import { PlayerService } from '../../services/player.service';
import { PlaylistsService, Playlist } from '../../services/playlists.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-browse-genres',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent, SearchHeaderComponent],
  templateUrl: './browse-genres.html',
  styleUrl: './browse-genres.css',
})
export class BrowseGenresComponent implements OnInit, OnDestroy {
  genres: Genre[] = [];
  isLoading = false;
  error: string | null = null;
  currentUser: User | null = null;
  
  // Song list state
  selectedGenre: Genre | null = null;
  showRanking = false;
  songs: Song[] = [];
  isLoadingSongs = false;
  songError: string | null = null;
  currentSongId: number | null = null;
  isPlaying = false;
  
  // Top songs ranking
  topSongs: Song[] = [];
  isLoadingTopSongs = false;
  topSongsError: string | null = null;
  
  // Public playlists
  showPublicPlaylists = false;
  publicPlaylists: Playlist[] = [];
  isLoadingPublicPlaylists = false;
  publicPlaylistsError: string | null = null;
  
  private subscriptions = new Subscription();

  constructor(
    private genreService: GenreService,
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    private songService: SongService,
    private playerService: PlayerService,
    private playlistsService: PlaylistsService
  ) {}

  ngOnInit() {
    this.loadCurrentUser();
    this.loadGenres();
    this.loadTopSongs();
    
    // Check if we should show public playlists from query params
    this.route.queryParams.subscribe(params => {
      if (params['show'] === 'public-playlists') {
        this.onPublicPlaylistsClick();
      }
    });
    
    // Subscribe to player state
    const playerSub = this.playerService.currentSongId$.subscribe(songId => {
      this.currentSongId = songId;
    });
    this.subscriptions.add(playerSub);

    const playingSub = this.playerService.isPlaying$.subscribe(playing => {
      this.isPlaying = playing;
    });
    this.subscriptions.add(playingSub);
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  loadCurrentUser() {
    this.currentUser = this.authService.getCurrentUser();
  }

  loadGenres() {
    this.isLoading = true;
    this.error = null;
    
    this.genreService.getGenres().subscribe({
      next: (genres) => {
        this.genres = genres;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Không thể tải danh sách thể loại';
        this.isLoading = false;
        console.error('Error loading genres:', err);
      }
    });
  }

  onGenreClick(genre: Genre) {
    this.selectedGenre = genre;
    this.showRanking = false;
    this.showPublicPlaylists = false;
    this.loadSongsByGenre(genre.genreId);
  }

  onRankingClick() {
    this.showRanking = true;
    this.selectedGenre = null;
    this.showPublicPlaylists = false;
    this.songError = null;
    this.isLoadingSongs = this.isLoadingTopSongs;
    
    if (this.topSongs.length > 0) {
      this.songs = this.topSongs;
      this.isLoadingSongs = false;
    } else {
      // If topSongs not loaded yet, load them
      this.isLoadingSongs = true;
      this.loadTopSongs();
    }
  }

  onPublicPlaylistsClick() {
    this.showPublicPlaylists = true;
    this.selectedGenre = null;
    this.showRanking = false;
    this.loadPublicPlaylists();
  }

  loadPublicPlaylists() {
    this.isLoadingPublicPlaylists = true;
    this.publicPlaylistsError = null;
    
    this.playlistsService.getPublicPlaylists().subscribe({
      next: (playlists) => {
        this.publicPlaylists = playlists;
        console.log('Loaded public playlists:', playlists); // Debug
        this.isLoadingPublicPlaylists = false;
      },
      error: (err) => {
        this.publicPlaylistsError = 'Không thể tải danh sách phát công khai';
        this.isLoadingPublicPlaylists = false;
        console.error('Error loading public playlists:', err);
      }
    });
  }

  loadSongsByGenre(genreId: number) {
    this.isLoadingSongs = true;
    this.songError = null;
    
    // Get all songs and filter by genre
    this.songService.getSongs(1, 100).subscribe({
      next: (response) => {
        const allSongs: Song[] = response?.songs ?? [];
        this.songs = allSongs.filter(song => song.genreId === genreId);
        this.isLoadingSongs = false;
        
        if (this.songs.length === 0) {
          this.songError = 'Không có bài hát nào trong thể loại này';
        }
      },
      error: (err) => {
        this.songError = 'Không thể tải danh sách bài hát';
        this.isLoadingSongs = false;
        console.error('Error loading songs:', err);
      }
    });
  }

  goBackToGenres() {
    this.selectedGenre = null;
    this.showRanking = false;
    this.showPublicPlaylists = false;
    this.songs = [];
    this.songError = null;
    this.publicPlaylists = [];
    this.publicPlaylistsError = null;
  }

  onSongClick(song: Song) {
    this.router.navigate(['/songs', song.songId]);
  }

  onSongPlayClick(event: Event, song: Song) {
    event.stopPropagation();
    
    if (!this.currentUser) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: '/browse-genres' }
      });
      return;
    }

    const currentSongId = this.playerService.getCurrentSongId();
    if (currentSongId === song.songId) {
      this.playerService.togglePlayback();
      return;
    }

    this.playerService.playSong(song, this.songs);
    this.playerService.setPlayingPlaylistId(null);
  }

  loadTopSongs() {
    this.isLoadingTopSongs = true;
    this.topSongsError = null;
    
    this.songService.getSongs(1, 100).subscribe({
      next: (response) => {
        const allSongs: Song[] = response?.songs ?? [];
        // Sort by playCount descending and take top 20
        this.topSongs = allSongs
          .sort((a, b) => (b.playCount || 0) - (a.playCount || 0))
          .slice(0, 20);
        this.isLoadingTopSongs = false;
        
        // If ranking view is active, update songs list
        if (this.showRanking) {
          this.songs = this.topSongs;
          this.isLoadingSongs = false;
        }
      },
      error: (err) => {
        this.topSongsError = 'Không thể tải bảng xếp hạng';
        this.isLoadingTopSongs = false;
        if (this.showRanking) {
          this.isLoadingSongs = false;
          this.songError = 'Không thể tải bảng xếp hạng';
        }
        console.error('Error loading top songs:', err);
      }
    });
  }

  getTotalPlays(): number {
    return this.topSongs.reduce((sum, song) => sum + (song.playCount || 0), 0);
  }

  onTopSongClick(song: Song) {
    this.router.navigate(['/songs', song.songId]);
  }

  onTopSongPlayClick(event: Event, song: Song) {
    event.stopPropagation();
    
    if (!this.currentUser) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: '/browse-genres' }
      });
      return;
    }

    const currentSongId = this.playerService.getCurrentSongId();
    if (currentSongId === song.songId) {
      this.playerService.togglePlayback();
      return;
    }

    this.playerService.playSong(song, this.topSongs);
    this.playerService.setPlayingPlaylistId(null);
  }

  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  onPlaylistClick(playlist: Playlist) {
    this.router.navigate(['/playlists', playlist.playlistId], {
      queryParams: { from: 'browse-genres', section: 'public-playlists' }
    });
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
