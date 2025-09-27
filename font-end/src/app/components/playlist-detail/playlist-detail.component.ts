import { Component, OnInit, OnDestroy, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap, takeUntil, combineLatest } from 'rxjs';
import { PlaylistsService, PlaylistDetail, PlaylistSong, SearchSong } from '../../services/playlists.service';
import { AuthService } from '../../services/auth.service';
import { PlayerService } from '../../services/player.service';
import { User } from '../../models/user.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';

@Component({
  selector: 'app-playlist-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, SidebarComponent, SearchHeaderComponent],
  templateUrl: './playlist-detail.component.html',
  styleUrls: ['./playlist-detail.component.css']
})
export class PlaylistDetailComponent implements OnInit, OnDestroy {
  playlist: PlaylistDetail | null = null;
  playlistId: number = 0;
  loading = false;
  errorMessage = '';
  successMessage = '';
  
  // Add song modal
  showAddSongModal = false;
  availableSongs: SearchSong[] = [];
  searchQuery = '';
  loadingSongs = false;
  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();
  
  // For layout components
  currentUser: User | null = null;
  
  // Player state
  currentSongId: number | null = null;
  isPlaying = false;

  // Cache busting timestamp for cover image
  coverImageTimestamp: number = 0;

  // Edit modal
  showEditModal = false;
  editForm = {
    playlistName: '',
    description: '',
    isPublic: false
  };
  selectedImage: File | null = null;
  imagePreview: string | null = null;
  editErrorMessage = '';
  editSuccessMessage = '';

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  @ViewChild('editFileInput') editFileInput!: ElementRef<HTMLInputElement>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private playlistsService: PlaylistsService,
    private authService: AuthService,
    private playerService: PlayerService,
    private cdr: ChangeDetectorRef
  ) {}

  // Track if we came from browse-genres public playlists
  fromBrowseGenres = false;

  ngOnInit(): void {
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
    });

    // Check if we came from browse-genres public playlists
    this.route.queryParams.subscribe(params => {
      this.fromBrowseGenres = params['from'] === 'browse-genres' && params['section'] === 'public-playlists';
    });

    this.route.params.subscribe(params => {
      this.playlistId = +params['id'];
      if (this.playlistId) {
        this.loadPlaylist();
      }
    });

    // Setup real-time search with debounce
    this.searchSubject.pipe(
      debounceTime(300), // Wait 300ms after user stops typing
      distinctUntilChanged(), // Only search if query changed
      switchMap((query: string) => {
        this.loadingSongs = true;
        return this.playlistsService.searchSongs(query || undefined);
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (songs) => {
        // Filter out songs already in playlist (using current playlist data)
        const existingSongIds = this.playlist?.songs.map(s => s.songId) || [];
        this.availableSongs = songs.filter(s => !existingSongIds.includes(s.songId));
        this.loadingSongs = false;
      },
      error: (error) => {
        console.error('Error searching songs:', error);
        this.loadingSongs = false;
        this.availableSongs = [];
      }
    });

    // Subscribe to player state
    combineLatest([
      this.playerService.currentSongId$,
      this.playerService.isPlaying$
    ]).pipe(takeUntil(this.destroy$)).subscribe(([songId, playing]) => {
      this.currentSongId = songId;
      this.isPlaying = playing;
    });

    // Subscribe to playlist song changes (when songs are added/removed from any playlist)
    this.playlistsService.onPlaylistSongChanged.pipe(takeUntil(this.destroy$)).subscribe(({ playlistId, action }) => {
      // Nếu bài hát được thêm/xóa vào playlist hiện tại, reload danh sách
      if (playlistId === this.playlistId) {
        this.loadPlaylist();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPlaylist(): void {
    this.loading = true;
    this.errorMessage = '';

    this.playlistsService.getPlaylist(this.playlistId).subscribe({
      next: (playlist) => {
        console.log('Playlist loaded:', playlist);
        console.log('Cover image URL from load:', playlist.coverImageUrl);
        this.playlist = playlist;
        // Reset cache busting timestamp when loading fresh data
        this.coverImageTimestamp = 0;
        this.loading = false;
        // Force change detection to update the view
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading playlist:', error);
        this.errorMessage = error.error?.message || 'Không thể tải danh sách phát';
        this.loading = false;
      }
    });
  }

  openAddSongModal(): void {
    this.showAddSongModal = true;
    this.searchQuery = '';
    this.availableSongs = [];
    // Don't trigger search automatically - wait for user to type
  }

  closeAddSongModal(): void {
    this.showAddSongModal = false;
    this.searchQuery = '';
    this.availableSongs = [];
  }

  onSearchChange(): void {
    // Trigger search immediately when user types (debounce handled by Subject)
    this.searchSubject.next(this.searchQuery);
  }

  addSongToPlaylist(song: SearchSong): void {
    this.playlistsService.addSongToPlaylist(this.playlistId, song.songId).subscribe({
      next: () => {
        this.successMessage = `Đã thêm "${song.songTitle}" vào danh sách phát`;
        setTimeout(() => this.successMessage = '', 3000);
        
        // Reload playlist and refresh search results immediately
        // This ensures the list is updated when modal is reopened
        this.playlistsService.getPlaylist(this.playlistId).subscribe({
          next: (playlist) => {
            this.playlist = playlist;
            // Also update the main playlist view
            this.loadPlaylist();
            // Trigger search again to refresh the available songs list
            // This will filter out the newly added song
            this.searchSubject.next(this.searchQuery);
          },
          error: (error) => {
            console.error('Error reloading playlist after adding song:', error);
            // Still trigger search even if reload fails
            this.searchSubject.next(this.searchQuery);
          }
        });
      },
      error: (error) => {
        console.error('Error adding song:', error);
        this.errorMessage = error.error?.message || 'Không thể thêm bài hát';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  removeSongFromPlaylist(song: PlaylistSong): void {
    if (!confirm(`Bạn có chắc chắn muốn xóa "${song.songTitle}" khỏi danh sách phát?`)) {
      return;
    }

    this.playlistsService.removeSongFromPlaylist(this.playlistId, song.songId).subscribe({
      next: () => {
        this.successMessage = `Đã xóa "${song.songTitle}" khỏi danh sách phát`;
        this.loadPlaylist();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        console.error('Error removing song:', error);
        this.errorMessage = error.error?.message || 'Không thể xóa bài hát';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  formatDuration(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${minutes}:${secs.toString().padStart(2, '0')}`;
  }

  playPlaylist(): void {
    if (!this.playlist || this.playlist.songs.length === 0) {
      this.errorMessage = 'Danh sách phát không có bài hát nào';
      setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    // Kiểm tra xem có đang phát từ playlist này không (sử dụng playingPlaylistId)
    const playingPlaylistId = this.playerService.getPlayingPlaylistId();
    const isPlayingFromThisPlaylist = playingPlaylistId === this.playlistId;

    if (isPlayingFromThisPlaylist && this.isPlaying) {
      // Nếu đang phát từ playlist này, thì pause
      this.playerService.togglePlayback();
    } else if (isPlayingFromThisPlaylist && !this.isPlaying) {
      // Nếu đang pause từ playlist này, thì resume
      this.playerService.togglePlayback();
    } else {
      // Nếu không phát từ playlist này, phát từ đầu
      const firstSong = this.playlist.songs[0];
      this.playerService.playSong(firstSong, this.playlist.songs);
      // Set playing playlist ID in PlayerService so it persists across navigation
      this.playerService.setPlayingPlaylistId(this.playlistId);
    }
  }

  isPlayingFromThisPlaylist(): boolean {
    const playingPlaylistId = this.playerService.getPlayingPlaylistId();
    return playingPlaylistId === this.playlistId && this.isPlaying;
  }

  onSongPlayClick(song: PlaylistSong): void {
    const currentSongId = this.playerService.getCurrentSongId();
    if (currentSongId === song.songId) {
      this.playerService.togglePlayback();
      return;
    }
    // Play song with the playlist songs as the playlist
    if (this.playlist?.songs) {
      this.playerService.playSong(song, this.playlist.songs);
      // Set playing playlist ID when playing a song from this playlist
      this.playerService.setPlayingPlaylistId(this.playlistId);
    } else {
      this.playerService.playSong(song);
      // Reset playingPlaylistId if playing without playlist
      this.playerService.setPlayingPlaylistId(null);
    }
  }

  getCoverImageUrl(): string {
    if (!this.playlist) {
      return 'https://via.placeholder.com/300x300/e0e0e0/666?text=Playlist';
    }
    
    if (!this.playlist.coverImageUrl || this.playlist.coverImageUrl.trim() === '') {
      return 'https://via.placeholder.com/300x300/e0e0e0/666?text=Playlist';
    }
    
    let imageUrl = this.playlist.coverImageUrl;
    
    // If URL is already a full URL (starts with http:// or https://), use it directly
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      // Add cache busting only if we have a timestamp (after upload)
      if (this.coverImageTimestamp > 0) {
        const separator = imageUrl.includes('?') ? '&' : '?';
        imageUrl = `${imageUrl}${separator}t=${this.coverImageTimestamp}`;
      }
      return imageUrl;
    }
    
    // If URL is a relative path (starts with images/), add base URL
    if (imageUrl.startsWith('images/')) {
      const baseUrl = 'https://localhost:5001';
      let fullUrl = `${baseUrl}/${imageUrl}`;
      // Add cache busting only if we have a timestamp (after upload)
      if (this.coverImageTimestamp > 0) {
        const separator = fullUrl.includes('?') ? '&' : '?';
        fullUrl = `${fullUrl}${separator}t=${this.coverImageTimestamp}`;
      }
      return fullUrl;
    }
    
    // Otherwise, use as is
    return imageUrl;
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'https://via.placeholder.com/300x300/e0e0e0/666?text=Playlist';
  }

  onImageLoad(event: Event): void {
    console.log('Image loaded successfully');
  }

  openImageUpload(): void {
    if (this.fileInput) {
      this.fileInput.nativeElement.click();
    }
  }

  openEditModal(): void {
    if (!this.playlist) {
      return;
    }
    this.editForm = {
      playlistName: this.playlist.playlistName,
      description: this.playlist.description || '',
      isPublic: this.playlist.isPublic || false
    };
    this.imagePreview = this.getCoverImageUrl();
    this.selectedImage = null;
    this.showEditModal = true;
    this.editErrorMessage = '';
    this.editSuccessMessage = '';
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.editForm = {
      playlistName: '',
      description: '',
      isPublic: false
    };
    this.selectedImage = null;
    this.imagePreview = null;
    this.editErrorMessage = '';
    this.editSuccessMessage = '';
  }

  openEditImageUpload(): void {
    if (this.editFileInput) {
      this.editFileInput.nativeElement.click();
    }
  }

  onEditImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.editErrorMessage = 'Vui lòng chọn file ảnh hợp lệ';
        setTimeout(() => this.editErrorMessage = '', 3000);
        return;
      }

      // Validate file size (max 10MB)
      const maxSize = 10 * 1024 * 1024; // 10MB
      if (file.size > maxSize) {
        this.editErrorMessage = 'Ảnh quá lớn. Kích thước tối đa là 10MB';
        setTimeout(() => this.editErrorMessage = '', 3000);
        return;
      }

      this.selectedImage = file;
      this.editErrorMessage = '';

      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.imagePreview = e.target?.result as string;
        this.cdr.detectChanges();
      };
      reader.readAsDataURL(file);
    }
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.errorMessage = 'Vui lòng chọn file ảnh hợp lệ';
        setTimeout(() => this.errorMessage = '', 3000);
        return;
      }

      // Validate file size (max 10MB)
      const maxSize = 10 * 1024 * 1024; // 10MB
      if (file.size > maxSize) {
        this.errorMessage = 'Ảnh quá lớn. Kích thước tối đa là 10MB';
        setTimeout(() => this.errorMessage = '', 3000);
        return;
      }

      this.loading = true;
      this.errorMessage = '';
      this.successMessage = '';

      this.playlistsService.uploadPlaylistCover(this.playlistId, file).subscribe({
        next: (updatedPlaylist) => {
          console.log('Upload response:', updatedPlaylist);
          console.log('Cover image URL from response:', updatedPlaylist.coverImageUrl);
          
          // Update playlist cover image URL immediately from response
          if (this.playlist && updatedPlaylist.coverImageUrl) {
            // Directly update the property - Angular will detect the change
            this.playlist.coverImageUrl = updatedPlaylist.coverImageUrl;
            console.log('Updated playlist coverImageUrl:', this.playlist.coverImageUrl);
            // Force change detection to update the view immediately
            this.cdr.detectChanges();
          }
          
          this.successMessage = 'Đã cập nhật ảnh bìa thành công!';
          this.loading = false;
          setTimeout(() => this.successMessage = '', 3000);
          
          // Reset file input
          input.value = '';
        },
        error: (error) => {
          console.error('Error uploading cover image:', error);
          console.error('Error details:', error);
          this.errorMessage = error.error?.message || 'Không thể upload ảnh bìa';
          this.loading = false;
          setTimeout(() => this.errorMessage = '', 3000);
          // Reset file input
          input.value = '';
        }
      });
    }
  }

  savePlaylistInfo(): void {
    if (!this.playlist) {
      return;
    }

    if (!this.editForm.playlistName.trim()) {
      this.editErrorMessage = 'Tên danh sách phát không được để trống';
      setTimeout(() => this.editErrorMessage = '', 3000);
      return;
    }

    this.loading = true;
    this.editErrorMessage = '';
    this.editSuccessMessage = '';

    // First, update playlist info if changed
    const nameChanged = this.editForm.playlistName.trim() !== this.playlist.playlistName;
    const descriptionChanged = (this.editForm.description.trim() || '') !== (this.playlist.description || '');
    const publicChanged = this.editForm.isPublic !== this.playlist.isPublic;

    if (nameChanged || descriptionChanged || publicChanged) {
      this.playlistsService.updatePlaylist(this.playlistId, {
        playlistName: this.editForm.playlistName.trim(),
        description: this.editForm.description.trim() || undefined,
        isPublic: this.editForm.isPublic
      }).subscribe({
        next: () => {
          // Then upload image if selected
          if (this.selectedImage) {
            this.playlistsService.uploadPlaylistCover(this.playlistId, this.selectedImage).subscribe({
              next: () => {
                this.coverImageTimestamp = Date.now();
                this.loadPlaylist(); // Reload playlist to get full details
                this.editSuccessMessage = 'Đã cập nhật thông tin danh sách phát thành công!';
                this.loading = false;
                this.closeEditModal();
                setTimeout(() => this.editSuccessMessage = '', 3000);
              },
              error: (error) => {
                console.error('Error uploading cover image:', error);
                this.editErrorMessage = error.error?.message || 'Không thể upload ảnh bìa';
                this.loading = false;
                setTimeout(() => this.editErrorMessage = '', 3000);
              }
            });
          } else {
            this.loadPlaylist(); // Reload playlist to get full details
            this.editSuccessMessage = 'Đã cập nhật thông tin danh sách phát thành công!';
            this.loading = false;
            this.closeEditModal();
            setTimeout(() => this.editSuccessMessage = '', 3000);
          }
        },
        error: (error) => {
          console.error('Error updating playlist:', error);
          this.editErrorMessage = error.error?.message || 'Không thể cập nhật thông tin danh sách phát';
          this.loading = false;
          setTimeout(() => this.editErrorMessage = '', 3000);
        }
      });
    } else if (this.selectedImage) {
      // Only image changed
      this.playlistsService.uploadPlaylistCover(this.playlistId, this.selectedImage).subscribe({
        next: () => {
          this.coverImageTimestamp = Date.now();
          this.loadPlaylist(); // Reload playlist to get full details
          this.editSuccessMessage = 'Đã cập nhật ảnh bìa thành công!';
          this.loading = false;
          this.closeEditModal();
          setTimeout(() => this.editSuccessMessage = '', 3000);
        },
        error: (error) => {
          console.error('Error uploading cover image:', error);
          this.editErrorMessage = error.error?.message || 'Không thể upload ảnh bìa';
          this.loading = false;
          setTimeout(() => this.editErrorMessage = '', 3000);
        }
      });
    } else {
      // Nothing changed
      this.closeEditModal();
    }
  }

  navigateToArtist(song: PlaylistSong, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/artists', song.artistId]);
  }

  navigateToSong(song: PlaylistSong, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.router.navigate(['/songs', song.songId]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  isOwner(): boolean {
    if (!this.playlist || !this.currentUser) {
      return false;
    }
    return this.playlist.ownerId === this.currentUser.userId;
  }

  goBack(): void {
    if (this.fromBrowseGenres) {
      // Navigate back to browse-genres and trigger public playlists view
      this.router.navigate(['/browse-genres'], {
        queryParams: { show: 'public-playlists' }
      });
    } else {
      // Default: go back to playlists page
      this.router.navigate(['/playlists']);
    }
  }
}

