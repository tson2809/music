import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { SongService } from '../../services/song.service';
import { AuthService } from '../../services/auth.service';
import { Artist, Album, Genre } from '../../models/song.model';
import { User } from '../../models/user.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { PlayerBarComponent, CurrentTrack } from '../player-bar/player-bar.component';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SidebarComponent, SearchHeaderComponent, PlayerBarComponent],
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.css']
})
export class UploadComponent implements OnInit {
  uploadForm: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';
  selectedFile: File | null = null;
  fileName = '';
  fileSize = 0;
  
  // Image upload
  selectedImageFile: File | null = null;
  imageFileName = '';
  imageFileSize = 0;
  imagePreview: string | null = null;

  albums: Album[] = [];
  genres: Genre[] = [];
  loadingAlbums = false;
  loadingGenres = false;
  
  // Artist info
  currentArtistName: string = '';

  // For layout components
  currentUser: User | null = null;
  progress = 0;
  volume = 70;
  currentTrack: CurrentTrack = {
    name: 'Chọn bài hát để phát',
    artist: 'Nghệ sĩ'
  };

  constructor(
    private fb: FormBuilder,
    private songService: SongService,
    private authService: AuthService,
    private router: Router
  ) {
    this.uploadForm = this.fb.group({
      audioFile: [null, [Validators.required]],
      imageFile: [null],
      songTitle: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(200)]],
      artistId: [0, [Validators.required, Validators.min(1)]],
      albumId: [null],
      genreId: [null],
      releaseDate: [null],
      lyrics: ['']
    });
  }

  ngOnInit(): void {
    // Get current user and set artist automatically
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
      
      // Automatically set artistId from logged-in user
      if (this.currentUser?.artistId) {
        this.uploadForm.patchValue({ artistId: this.currentUser.artistId });
        this.loadArtistInfo(this.currentUser.artistId);
        this.loadAlbums(this.currentUser.artistId);
      } else {
        this.errorMessage = 'Tài khoản của bạn chưa được liên kết với nghệ sĩ. Vui lòng liên hệ quản trị viên.';
      }
    });

    this.loadGenres();
    
    // Load albums when artist changes
    this.uploadForm.get('artistId')?.valueChanges.subscribe(artistId => {
      if (artistId && artistId > 0) {
        this.loadAlbums(artistId);
      } else {
        this.albums = [];
        this.uploadForm.patchValue({ albumId: null });
      }
    });
  }

  loadArtistInfo(artistId: number): void {
    this.songService.getArtists().subscribe({
      next: (artists) => {
        const artist = artists.find(a => a.artistId === artistId);
        if (artist) {
          this.currentArtistName = artist.artistName;
        } else {
          this.currentArtistName = 'Nghệ sĩ không xác định';
        }
      },
      error: (error) => {
        console.error('Error loading artist info:', error);
        this.currentArtistName = 'Không thể tải thông tin nghệ sĩ';
      }
    });
  }

  loadAlbums(artistId: number): void {
    this.loadingAlbums = true;
    this.songService.getAlbums(artistId).subscribe({
      next: (albums) => {
        this.albums = albums;
        this.loadingAlbums = false;
      },
      error: (error) => {
        console.error('Error loading albums:', error);
        this.loadingAlbums = false;
      }
    });
  }

  loadGenres(): void {
    this.loadingGenres = true;
    this.songService.getGenres().subscribe({
      next: (genres) => {
        this.genres = genres;
        this.loadingGenres = false;
      },
      error: (error) => {
        console.error('Error loading genres:', error);
        this.loadingGenres = false;
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      const allowedTypes = ['audio/mpeg', 'audio/mp3', 'audio/wav', 'audio/m4a', 'audio/flac', 'audio/ogg'];
      const allowedExtensions = ['.mp3', '.wav', '.m4a', '.flac', '.ogg'];
      const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase();
      
      if (!allowedTypes.includes(file.type) && !allowedExtensions.includes(fileExtension)) {
        this.errorMessage = 'Định dạng file không được hỗ trợ. Chỉ chấp nhận: mp3, wav, m4a, flac, ogg';
        input.value = '';
        return;
      }

      // Validate file size (50MB max)
      const maxSize = 50 * 1024 * 1024; // 50MB
      if (file.size > maxSize) {
        this.errorMessage = 'File quá lớn. Kích thước tối đa là 50MB';
        input.value = '';
        return;
      }

      this.selectedFile = file;
      this.fileName = file.name;
      this.fileSize = file.size;
      this.uploadForm.patchValue({ audioFile: file });
      this.errorMessage = '';
    }
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
      const allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];
      const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase();
      
      if (!allowedTypes.includes(file.type) && !allowedExtensions.includes(fileExtension)) {
        this.errorMessage = 'Định dạng ảnh không được hỗ trợ. Chỉ chấp nhận: jpg, jpeg, png, gif, webp';
        input.value = '';
        return;
      }

      // Validate file size (10MB max)
      const maxSize = 10 * 1024 * 1024; // 10MB
      if (file.size > maxSize) {
        this.errorMessage = 'Ảnh quá lớn. Kích thước tối đa là 10MB';
        input.value = '';
        return;
      }

      this.selectedImageFile = file;
      this.imageFileName = file.name;
      this.imageFileSize = file.size;
      this.uploadForm.patchValue({ imageFile: file });
      
      // Create image preview
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreview = e.target.result;
      };
      reader.readAsDataURL(file);
      
      this.errorMessage = '';
    }
  }

  removeImage(): void {
    this.selectedImageFile = null;
    this.imageFileName = '';
    this.imageFileSize = 0;
    this.imagePreview = null;
    this.uploadForm.patchValue({ imageFile: null });
    
    // Reset file input
    const imageInput = document.getElementById('imageFile') as HTMLInputElement;
    if (imageInput) {
      imageInput.value = '';
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  get audioFile() {
    return this.uploadForm.get('audioFile');
  }

  get imageFile() {
    return this.uploadForm.get('imageFile');
  }

  get songTitle() {
    return this.uploadForm.get('songTitle');
  }

  get artistId() {
    return this.uploadForm.get('artistId');
  }

  get albumId() {
    return this.uploadForm.get('albumId');
  }

  get genreId() {
    return this.uploadForm.get('genreId');
  }

  get releaseDate() {
    return this.uploadForm.get('releaseDate');
  }

  get lyrics() {
    return this.uploadForm.get('lyrics');
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  onSubmit(): void {
    if (this.uploadForm.invalid) {
      this.uploadForm.markAllAsTouched();
      return;
    }

    if (!this.selectedFile) {
      this.errorMessage = 'Vui lòng chọn file nhạc';
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    // Create FormData
    const formData = new FormData();
    formData.append('audioFile', this.selectedFile);
    formData.append('songTitle', this.uploadForm.value.songTitle);
    formData.append('artistId', this.uploadForm.value.artistId.toString());
    
    // Add image if selected
    if (this.selectedImageFile) {
      formData.append('imageFile', this.selectedImageFile);
    }
    
    if (this.uploadForm.value.albumId && this.uploadForm.value.albumId > 0) {
      formData.append('albumId', this.uploadForm.value.albumId.toString());
    }
    
    if (this.uploadForm.value.genreId && this.uploadForm.value.genreId > 0) {
      formData.append('genreId', this.uploadForm.value.genreId.toString());
    }
    
    if (this.uploadForm.value.releaseDate) {
      formData.append('releaseDate', this.uploadForm.value.releaseDate);
    }
    
    if (this.uploadForm.value.lyrics) {
      formData.append('lyrics', this.uploadForm.value.lyrics);
    }

    this.songService.uploadSong(formData).subscribe({
      next: (response) => {
        console.log('Upload thành công:', response);
        this.loading = false;
        this.successMessage = response.message || 'Upload nhạc thành công!';
        
        // Reset form after 2 seconds
        setTimeout(() => {
          this.uploadForm.reset();
          this.selectedFile = null;
          this.fileName = '';
          this.fileSize = 0;
          this.selectedImageFile = null;
          this.imageFileName = '';
          this.imageFileSize = 0;
          this.imagePreview = null;
          this.successMessage = '';
          this.albums = [];
        }, 2000);
      },
      error: (error) => {
        console.error('Lỗi upload:', error);
        this.loading = false;
        this.errorMessage = error.error?.message || 'Upload thất bại. Vui lòng thử lại.';
      }
    });
  }
}


