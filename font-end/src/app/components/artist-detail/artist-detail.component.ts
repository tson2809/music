import { Component, OnInit, OnDestroy, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { AuthService } from '../../services/auth.service';
import { SongService } from '../../services/song.service';
import { PlayerService } from '../../services/player.service';
import { Router } from '@angular/router';
import { User } from '../../models/user.model';
import { Subscription, combineLatest } from 'rxjs';

interface Artist {
  artistId: number;
  artistName: string;
  biography?: string;
  country?: string;
  profileImageUrl?: string;
  verified: boolean;
  monthlyListeners: number;
}

interface Song {
  songId: number;
  songTitle: string;
  artistName: string;
  albumTitle?: string;
  genreName?: string;
  durationSeconds: number;
  playCount: number;
  likeCount: number;
  coverImageUrl?: string;
  audioFileUrl: string;
  approvalStatus: string;
}

@Component({
  selector: 'app-artist-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, SidebarComponent, SearchHeaderComponent],
  templateUrl: './artist-detail.component.html',
  styleUrls: ['./artist-detail.component.css']
})
export class ArtistDetailComponent implements OnInit, OnDestroy {
  artistId: number = 0;
  artist: Artist | null = null;
  songs: Song[] = [];
  isLoading = false;
  errorMessage = '';

  // Layout components
  currentUser: User | null = null;
  playlists = ['Yêu thích', 'Nhạc Việt', 'Pop', 'Rock', 'Chill', 'Workout'];

  // Player state (từ PlayerService)
  activeSongId: number | null = null;
  isPlaying = false;

  // Edit modal state
  showEditModal = false;
  editForm = {
    artistName: '',
    biography: ''
  };
  selectedImage: File | null = null;
  imagePreview: string | null = null;
  loading = false;
  successMessage = '';
  editErrorMessage = '';

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  private subscriptions = new Subscription();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private songService: SongService,
    private playerService: PlayerService,
    private cdr: ChangeDetectorRef,
    private http: HttpClient
  ) {}

  // Kiểm tra xem user hiện tại có phải là chủ sở hữu của artist page không
  isOwner(): boolean {
    return this.currentUser !== null && 
           this.currentUser.artistId !== undefined && 
           this.currentUser.artistId !== null &&
           this.currentUser.artistId === this.artistId;
  }

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
      this.activeSongId = songId;
      this.isPlaying = playing;
    });
    this.subscriptions.add(playerSub);

    // Get artist ID from route
    this.route.params.subscribe(params => {
      this.artistId = +params['id'];
      if (this.artistId) {
        this.loadArtistDetails();
        this.loadArtistSongs();
      }
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadArtistDetails(): void {
    this.isLoading = true;
    console.log('Loading artist:', this.artistId);
    this.songService.getArtistById(this.artistId).subscribe({
      next: (artist: any) => {
        console.log('Artist loaded:', artist);
        this.artist = artist;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading artist:', error);
        this.errorMessage = 'Không thể tải thông tin nghệ sĩ';
        this.isLoading = false;
      }
    });
  }

  loadArtistSongs(): void {
    console.log('Loading songs for artist:', this.artistId);
    this.songService.getSongsByArtist(this.artistId).subscribe({
      next: (response: any) => {
        console.log('Songs loaded:', response);
        this.songs = response.songs || response;
      },
      error: (error: any) => {
        console.error('Error loading artist songs:', error);
      }
    });
  }

  onSongPlayClick(song: Song): void {
    // Kiểm tra đăng nhập trước khi phát nhạc
    if (!this.currentUser) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: `/artists/${this.artistId}` }
      });
      return;
    }

    const currentSongId = this.playerService.getCurrentSongId();
    if (currentSongId === song.songId) {
      // Nếu đang phát bài này, toggle play/pause
      this.playerService.togglePlayback();
      return;
    }

    // Phát bài hát với danh sách bài hát của artist làm playlist
    this.playerService.playSong(song, this.songs);
  }

  formatDuration(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    } else if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  openEditModal(): void {
    if (!this.isOwner()) {
      return;
    }
    if (this.artist) {
      this.editForm = {
        artistName: this.artist.artistName,
        biography: this.artist.biography || ''
      };
      this.imagePreview = this.artist.profileImageUrl || null;
      this.selectedImage = null;
      this.showEditModal = true;
      this.editErrorMessage = '';
      this.successMessage = '';
    }
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.editForm = {
      artistName: '',
      biography: ''
    };
    this.selectedImage = null;
    this.imagePreview = null;
    this.editErrorMessage = '';
    this.successMessage = '';
  }

  openImageUpload(): void {
    if (this.fileInput) {
      this.fileInput.nativeElement.click();
    }
  }

  onImageSelected(event: Event): void {
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

  saveArtistInfo(): void {
    if (!this.artist) {
      return;
    }

    this.loading = true;
    this.editErrorMessage = '';
    this.successMessage = '';

    const formData = new FormData();
    
    if (this.editForm.artistName.trim()) {
      formData.append('ArtistName', this.editForm.artistName.trim());
    }
    
    if (this.editForm.biography !== null) {
      formData.append('Biography', this.editForm.biography.trim() || '');
    }
    
    if (this.selectedImage) {
      formData.append('ProfileImageFile', this.selectedImage);
    }

    this.songService.updateArtist(this.artistId, formData).subscribe({
      next: (updatedArtist) => {
        this.artist = updatedArtist;
        this.successMessage = 'Đã cập nhật thông tin nghệ sĩ thành công!';
        this.loading = false;
        
        // Reload user info để cập nhật avatar trong header
        if (this.currentUser) {
          this.http.get<User>(`https://localhost:5001/api/auth/profile/${this.currentUser.userId}`, {
            headers: {
              'Authorization': `Bearer ${this.authService.getToken()}`
            }
          }).subscribe({
            next: (updatedUser) => {
              this.authService.updateUser(updatedUser);
              this.currentUser = updatedUser;
            },
            error: (error) => {
              console.error('Error reloading user profile:', error);
            }
          });
        }
        
        setTimeout(() => {
          this.closeEditModal();
          this.loadArtistDetails(); // Reload để có dữ liệu mới nhất
        }, 1500);
      },
      error: (error) => {
        console.error('Error updating artist:', error);
        this.editErrorMessage = error.error?.message || 'Không thể cập nhật thông tin nghệ sĩ';
        this.loading = false;
        setTimeout(() => this.editErrorMessage = '', 3000);
      }
    });
  }

  getProfileImageUrl(): string {
    if (!this.artist || !this.artist.profileImageUrl) {
      return '';
    }
    
    let imageUrl = this.artist.profileImageUrl;
    
    // If URL is already a full URL (starts with http:// or https://), use it directly
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl;
    }
    
    // If URL is a relative path (starts with images/), add base URL
    if (imageUrl.startsWith('images/')) {
      const baseUrl = 'https://localhost:5001';
      return `${baseUrl}/${imageUrl}`;
    }
    
    return imageUrl;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
