import { Component, Input, Output, EventEmitter, HostListener, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { User } from '../../models/user.model';
import { SongService } from '../../services/song.service';
import { AlbumService } from '../../services/album.service';
import { GenreService } from '../../services/genre.service';
import { PlayerService } from '../../services/player.service';
import { Song } from '../../models/song.model';
import { Genre } from '../../models/genre.model';
import { Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Subject } from 'rxjs';

interface Artist {
  artistId: number;
  artistName: string;
  profileImageUrl?: string | null;
  verified: boolean;
  monthlyListeners: number;
}

interface Album {
  albumId: number;
  albumTitle: string;
  artistId: number;
  artistName: string;
  coverImageUrl?: string | null;
}

@Component({
  selector: 'app-search-header',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './search-header.component.html',
  styleUrls: ['./search-header.component.css']
})
export class SearchHeaderComponent implements OnInit, OnDestroy {
  @Input() currentUser: User | null = null;
  @Output() logoutEvent = new EventEmitter<void>();
  
  showUserDropdown = false;
  showSearchDropdown = false;
  searchQuery = '';
  
  // Data
  popularSongs: Song[] = [];
  genres: Genre[] = [];
  artists: Artist[] = [];
  albums: Album[] = [];
  selectedGenreSongs: Song[] = [];
  selectedGenre: Genre | null = null;
  searchResults: Song[] = [];
  
  // Loading states
  isLoadingPopularSongs = false;
  isLoadingGenres = false;
  isLoadingArtists = false;
  isLoadingAlbums = false;
  isLoadingGenreSongs = false;
  isLoadingSearchResults = false;
  
  // UI states
  showSearchResults = false;
  
  currentSongId: number | null = null;
  isPlaying = false;
  
  private searchSubject = new Subject<string>();
  private subscriptions = new Subscription();
  private searchSubscription: Subscription | null = null;

  constructor(
    private router: Router,
    private songService: SongService,
    private albumService: AlbumService,
    private genreService: GenreService,
    private playerService: PlayerService
  ) {}

  ngOnInit() {
    // Subscribe to search input changes - immediate search without debounce
    const searchSub = this.searchSubject.pipe(
      distinctUntilChanged()
    ).subscribe(query => {
      if (query.trim()) {
        this.showSearchResults = true;
        this.searchSongs(query.trim());
      } else {
        this.showSearchResults = false;
        this.searchResults = [];
      }
    });
    this.subscriptions.add(searchSub);

    // Subscribe to player state
    const playerSub = this.playerService.currentSongId$.subscribe(songId => {
      this.currentSongId = songId;
    });
    this.subscriptions.add(playerSub);

    const playingSub = this.playerService.isPlaying$.subscribe(playing => {
      this.isPlaying = playing;
    });
    this.subscriptions.add(playingSub);

    // Load initial data when dropdown is shown
    this.loadInitialData();
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  // Kiểm tra xem user có phải admin không
  isAdmin(): boolean {
    return this.currentUser !== null && this.currentUser.roleId === 1;
  }

  // Kiểm tra xem user có phải artist không
  isArtist(): boolean {
    console.log('Checking isArtist:', this.currentUser);
    console.log('Artist ID:', this.currentUser?.artistId);
    const result = this.currentUser !== null && this.currentUser.artistId !== undefined && this.currentUser.artistId !== null;
    console.log('isArtist result:', result);
    return result;
  }

  // Kiểm tra xem user có phải user thường không (không phải admin, không phải artist)
  isRegularUser(): boolean {
    return this.currentUser !== null && 
           !this.isAdmin() && 
           !this.isArtist();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const clickedInsideUser = target.closest('.user-dropdown-wrapper');
    const clickedInsideSearch = target.closest('.search-bar-wrapper');
    
    if (!clickedInsideUser && this.showUserDropdown) {
      this.showUserDropdown = false;
    }
    
    if (!clickedInsideSearch && this.showSearchDropdown) {
      this.showSearchDropdown = false;
    }
  }

  toggleUserDropdown() {
    this.showUserDropdown = !this.showUserDropdown;
  }

  onSearchFocus() {
    this.showSearchDropdown = true;
    // If there's a search query, show search results, otherwise show popular songs
    if (this.searchQuery.trim() && this.showSearchResults) {
      // Keep showing search results
    } else if (this.popularSongs.length === 0) {
      this.loadInitialData();
    }
  }

  onSearchInput(value?: string) {
    // Always show dropdown when typing
    this.showSearchDropdown = true;
    
    // Get the current query value - use parameter if provided, otherwise use model
    const query = value !== undefined ? value : (this.searchQuery || '');
    
    // Search immediately when typing, even with 1 character
    if (query.length > 0) {
      this.showSearchResults = true;
      this.searchSongs(query);
    } else {
      this.showSearchResults = false;
      this.searchResults = [];
    }
  }

  onSearchKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' && this.searchQuery.trim()) {
      event.preventDefault();
      // Trigger search immediately without debounce
      this.showSearchResults = true;
      this.searchSongs(this.searchQuery.trim());
    }
  }

  navigateToArtistPage() {
    if (this.currentUser && this.currentUser.artistId) {
      this.showUserDropdown = false;
      this.router.navigate(['/artists', this.currentUser.artistId]);
    }
  }

  logout() {
    this.showUserDropdown = false;
    this.logoutEvent.emit();
  }

  getProfileImageUrl(): string {
    if (!this.currentUser || !this.currentUser.profilePictureUrl) {
      return '';
    }
    
    let imageUrl = this.currentUser.profilePictureUrl;
    
    // If URL is already a full URL (starts with http:// or https://), use it directly
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl;
    }
    
    // If URL is a relative path (starts with /images/ or images/), add base URL
    if (imageUrl.startsWith('/images/') || imageUrl.startsWith('images/')) {
      const baseUrl = 'https://localhost:5001';
      return imageUrl.startsWith('/') ? `${baseUrl}${imageUrl}` : `${baseUrl}/${imageUrl}`;
    }
    
    return imageUrl;
  }

  onSongClick(song: Song) {
    this.showSearchDropdown = false;
    this.router.navigate(['/songs', song.songId]);
  }

  onSongPlayClick(event: Event, song: Song) {
    event.stopPropagation();
    
    if (!this.currentUser) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: '/home' }
      });
      return;
    }

    const currentSongId = this.playerService.getCurrentSongId();
    if (currentSongId === song.songId) {
      this.playerService.togglePlayback();
      return;
    }

    let playlist: Song[] = [];
    if (this.showSearchResults && this.searchResults.length > 0) {
      playlist = this.searchResults;
    } else if (this.selectedGenre && this.selectedGenreSongs.length > 0) {
      playlist = this.selectedGenreSongs;
    } else {
      playlist = this.popularSongs;
    }

    this.playerService.playSong(song, playlist);
    this.playerService.setPlayingPlaylistId(null);
  }

  onGenreClick(genre: Genre) {
    this.selectedGenre = genre;
    this.loadSongsByGenre(genre.genreId);
  }

  onArtistClick(artist: Artist) {
    this.showSearchDropdown = false;
    this.router.navigate(['/artists', artist.artistId]);
  }

  onAlbumClick(album: Album) {
    this.showSearchDropdown = false;
    this.router.navigate(['/artists', album.artistId]);
  }

  onBrowseGenresClick() {
    this.showSearchDropdown = false;
    this.router.navigate(['/browse-genres']);
  }

  private loadInitialData() {
    this.loadPopularSongs();
  }

  private loadPopularSongs() {
    this.isLoadingPopularSongs = true;
    this.songService.getSongs(1, 10).subscribe({
      next: (response) => {
        // Sort by playCount and likeCount for popular songs
        const songs: Song[] = response?.songs ?? [];
        this.popularSongs = songs
          .sort((a: Song, b: Song) => {
            if (b.playCount !== a.playCount) {
              return b.playCount - a.playCount;
            }
            return b.likeCount - a.likeCount;
          })
          .slice(0, 5);
        this.isLoadingPopularSongs = false;
      },
      error: () => {
        this.isLoadingPopularSongs = false;
      }
    });
  }

  private loadGenres() {
    this.isLoadingGenres = true;
    this.genreService.getGenres().subscribe({
      next: (genres) => {
        this.genres = genres;
        this.isLoadingGenres = false;
      },
      error: () => {
        this.isLoadingGenres = false;
      }
    });
  }

  private loadSongsByGenre(genreId: number) {
    this.isLoadingGenreSongs = true;
    this.songService.getSongs(1, 20).subscribe({
      next: (response) => {
        const songs: Song[] = response?.songs ?? [];
        this.selectedGenreSongs = songs
          .filter((s: Song) => s.genreId === genreId)
          .sort((a: Song, b: Song) => b.playCount - a.playCount)
          .slice(0, 5);
        this.isLoadingGenreSongs = false;
      },
      error: () => {
        this.isLoadingGenreSongs = false;
      }
    });
  }

  private loadArtists() {
    this.isLoadingArtists = true;
    this.songService.getArtists().subscribe({
      next: (artists) => {
        this.artists = (artists || []).slice(0, 20).map(a => ({
          artistId: a.artistId,
          artistName: a.artistName,
          profileImageUrl: null,
          verified: false,
          monthlyListeners: 0
        }));
        this.isLoadingArtists = false;
      },
      error: () => {
        this.isLoadingArtists = false;
      }
    });
  }

  private loadAlbums() {
    this.isLoadingAlbums = true;
    this.albumService.getAlbums(1, 30).subscribe({
      next: (response) => {
        this.albums = (response?.albums ?? []).slice(0, 20).map(a => ({
          albumId: a.albumId,
          albumTitle: a.albumTitle,
          artistId: a.artistId,
          artistName: a.artistName,
          coverImageUrl: a.coverImageUrl
        }));
        this.isLoadingAlbums = false;
      },
      error: () => {
        this.isLoadingAlbums = false;
      }
    });
  }

  private searchSongs(query: string) {
    // Cancel previous search request if exists
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
    
    // Trim query before sending to API but keep original for display
    const trimmedQuery = query.trim();
    
    // Only search if query has content
    if (trimmedQuery.length === 0) {
      this.searchResults = [];
      this.isLoadingSearchResults = false;
      return;
    }
    
    this.isLoadingSearchResults = true;
    this.searchSubscription = this.songService.getSongs(1, 50, trimmedQuery).subscribe({
      next: (response) => {
        this.searchResults = response?.songs ?? [];
        this.isLoadingSearchResults = false;
      },
      error: () => {
        this.searchResults = [];
        this.isLoadingSearchResults = false;
      }
    });
  }
}

