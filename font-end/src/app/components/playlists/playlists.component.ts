import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PlaylistsService, Playlist, CreatePlaylistRequest, PlaylistDetail } from '../../services/playlists.service';
import { AuthService } from '../../services/auth.service';
import { PlayerService } from '../../services/player.service';
import { User } from '../../models/user.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { Subject, combineLatest, takeUntil } from 'rxjs';

@Component({
  selector: 'app-playlists',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, SidebarComponent, SearchHeaderComponent],
  templateUrl: './playlists.component.html',
  styleUrls: ['./playlists.component.css']
})
export class PlaylistsComponent implements OnInit, OnDestroy {
  playlists: Playlist[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';
  
  // Modal
  showCreateModal = false;
  createForm: FormGroup;
  selectedCoverImage: File | null = null;
  
  // For layout components
  currentUser: User | null = null;
  
  // Player state
  currentSongId: number | null = null;
  isPlaying = false;
  private destroy$ = new Subject<void>();

  constructor(
    private playlistsService: PlaylistsService,
    private authService: AuthService,
    private router: Router,
    private fb: FormBuilder,
    private playerService: PlayerService
  ) {
    this.createForm = this.fb.group({
      playlistName: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(200)]],
      description: [''],
      isPublic: [true]
    });
  }

  ngOnInit(): void {
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
    });

    // Subscribe to player state
    combineLatest([
      this.playerService.currentSongId$,
      this.playerService.isPlaying$
    ]).pipe(takeUntil(this.destroy$)).subscribe(([songId, playing]) => {
      this.currentSongId = songId;
      this.isPlaying = playing;
    });

    this.loadPlaylists();
    
    // Check if there's a current song playing when component initializes
    const currentSongId = this.playerService.getCurrentSongId();
    const isPlaying = this.playerService.getIsPlaying();
    if (currentSongId !== null) {
      this.currentSongId = currentSongId;
      this.isPlaying = isPlaying || false;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }


  loadPlaylists(): void {
    this.loading = true;
    this.errorMessage = '';

    // Debug: Log current user
    const currentUser = this.authService.getCurrentUser();
    console.log('Current user:', currentUser);
    console.log('User ID:', currentUser?.userId);

    this.playlistsService.getPlaylists().subscribe({
      next: (playlists) => {
        console.log('Playlists received:', playlists);
        console.log('Number of playlists:', playlists.length);
        this.playlists = playlists;
        this.loading = false;
        
        // Note: We don't automatically find playlist for current song here
        // because a song might be played from elsewhere (home, favorites, etc.)
        // and just happen to be in a playlist. We only set playingPlaylistId
        // when user explicitly plays from a playlist card or detail page.
      },
      error: (error) => {
        console.error('Error loading playlists:', error);
        console.error('Error status:', error.status);
        console.error('Error message:', error.error);
        this.errorMessage = error.error?.message || `Không thể tải danh sách phát (Status: ${error.status})`;
        this.loading = false;
      }
    });
  }

  openCreateModal(): void {
    this.createForm.reset({
      playlistName: '',
      description: '',
      isPublic: true
    });
    this.selectedCoverImage = null;
    this.showCreateModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  closeCreateModal(): void {
    this.showCreateModal = false;
    this.createForm.reset();
    this.selectedCoverImage = null;
    this.errorMessage = '';
    this.successMessage = '';
  }

  onCoverImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.errorMessage = 'Vui lòng chọn file ảnh hợp lệ';
        setTimeout(() => this.errorMessage = '', 3000);
        input.value = '';
        return;
      }

      // Validate file size (max 10MB)
      const maxSize = 10 * 1024 * 1024; // 10MB
      if (file.size > maxSize) {
        this.errorMessage = 'Ảnh quá lớn. Kích thước tối đa là 10MB';
        setTimeout(() => this.errorMessage = '', 3000);
        input.value = '';
        return;
      }

      this.selectedCoverImage = file;
      this.errorMessage = '';
    }
  }

  removeCoverImage(): void {
    this.selectedCoverImage = null;
    // Reset file input
    const fileInput = document.getElementById('coverImageFile') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  onCreateSubmit(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const request: CreatePlaylistRequest = {
      playlistName: this.createForm.value.playlistName,
      description: this.createForm.value.description || undefined,
      isPublic: this.createForm.value.isPublic ?? true
    };

    // Create playlist first
    this.playlistsService.createPlaylist(request).subscribe({
      next: (playlist) => {
        // If cover image is selected, upload it
        if (this.selectedCoverImage) {
          this.playlistsService.uploadPlaylistCover(playlist.playlistId, this.selectedCoverImage).subscribe({
            next: (updatedPlaylist) => {
              this.successMessage = 'Tạo danh sách phát thành công!';
              this.loadPlaylists();
              setTimeout(() => {
                this.closeCreateModal();
                this.router.navigate(['/playlists', updatedPlaylist.playlistId]);
              }, 1000);
            },
            error: (uploadError) => {
              console.error('Error uploading cover image:', uploadError);
              // Playlist was created but cover upload failed - still show success but with warning
              this.successMessage = 'Tạo danh sách phát thành công, nhưng không thể upload ảnh bìa. Bạn có thể thêm ảnh sau.';
              this.loadPlaylists();
              setTimeout(() => {
                this.closeCreateModal();
                this.router.navigate(['/playlists', playlist.playlistId]);
              }, 2000);
            }
          });
        } else {
          // No cover image, just navigate
          this.successMessage = 'Tạo danh sách phát thành công!';
          this.loadPlaylists();
          setTimeout(() => {
            this.closeCreateModal();
            this.router.navigate(['/playlists', playlist.playlistId]);
          }, 1000);
        }
      },
      error: (error) => {
        console.error('Error creating playlist:', error);
        this.errorMessage = error.error?.message || 'Không thể tạo danh sách phát';
      }
    });
  }

  deletePlaylist(playlist: Playlist): void {
    if (!confirm(`Bạn có chắc chắn muốn xóa danh sách phát "${playlist.playlistName}"?`)) {
      return;
    }

    this.playlistsService.deletePlaylist(playlist.playlistId).subscribe({
      next: () => {
        this.successMessage = 'Xóa danh sách phát thành công!';
        this.loadPlaylists();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        console.error('Error deleting playlist:', error);
        this.errorMessage = error.error?.message || 'Không thể xóa danh sách phát';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  get playlistName() {
    return this.createForm.get('playlistName');
  }

  playPlaylist(playlist: Playlist, event: Event): void {
    event.stopPropagation();
    
    if (playlist.songCount === 0) {
      this.errorMessage = 'Danh sách phát không có bài hát nào';
      setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    // Check if this playlist is currently playing
    const playingPlaylistId = this.playerService.getPlayingPlaylistId();
    const isPlayingFromThisPlaylist = playingPlaylistId === playlist.playlistId;

    if (isPlayingFromThisPlaylist && this.isPlaying) {
      // If playing from this playlist, pause
      this.playerService.togglePlayback();
      return;
    }

    if (isPlayingFromThisPlaylist && !this.isPlaying) {
      // If paused from this playlist, resume
      this.playerService.togglePlayback();
      return;
    }

    // If playing from a different playlist or no playlist, load and play this one
    // Load playlist details to get songs
    this.playlistsService.getPlaylist(playlist.playlistId).subscribe({
      next: (playlistDetail: PlaylistDetail) => {
        if (playlistDetail.songs.length === 0) {
          this.errorMessage = 'Danh sách phát không có bài hát nào';
          setTimeout(() => this.errorMessage = '', 3000);
          return;
        }

        // Play first song with full playlist
        const firstSong = playlistDetail.songs[0];
        this.playerService.playSong(firstSong, playlistDetail.songs);
        // Set playing playlist ID in PlayerService so it persists across navigation
        this.playerService.setPlayingPlaylistId(playlist.playlistId);
      },
      error: (error) => {
        console.error('Error loading playlist details:', error);
        this.errorMessage = 'Không thể tải danh sách phát';
        setTimeout(() => this.errorMessage = '', 3000);
      }
    });
  }

  isPlayingFromPlaylist(playlistId: number): boolean {
    const playingPlaylistId = this.playerService.getPlayingPlaylistId();
    return playingPlaylistId === playlistId && this.isPlaying;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

