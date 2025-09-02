import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { PlayerBarComponent, CurrentTrack } from '../player-bar/player-bar.component';
import { GenreService, CreateGenreRequest, UpdateGenreRequest } from '../../services/genre.service';
import { AuthService } from '../../services/auth.service';
import { Genre } from '../../models/genre.model';
import { User } from '../../models/user.model';


@Component({
  selector: 'app-genre-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    FormsModule,
    SidebarComponent,
    SearchHeaderComponent,
    PlayerBarComponent
  ],
  templateUrl: './genre-list.component.html',
  styleUrls: ['./genre-list.component.css']
})
export class GenreListComponent implements OnInit, OnDestroy {

  genres: Genre[] = [];
  currentUser: User | null = null;
  isLoadingGenres = false;
  genreError: string | null = null;
  
  // Search (client-side only)
  searchQuery = '';
  filteredGenres: Genre[] = [];
  
  // Player properties
  currentTrack: CurrentTrack = {
    name: 'Chọn bài hát để phát',
    artist: 'Nghệ sĩ'
  };
  progress = 0;
  volume = 70;
  isPlaying = false;
  currentTime = 0;
  duration = 0;
  
  private audio?: HTMLAudioElement;
  private readonly onTimeUpdate = () => {
    if (!this.audio) { return; }
    this.currentTime = this.audio.currentTime;
    this.duration = this.audio.duration || this.duration;
    const duration = this.duration || 0;
    this.progress = duration ? (this.audio.currentTime / duration) * 100 : 0;
  };
  private readonly onEnded = () => {
    this.playNextSong();
  };
  private readonly onLoadedMetadata = () => {
    if (this.audio?.duration) {
      this.duration = this.audio.duration;
    }
  };
  
  // Form và modal
  showModal = false;
  isEditMode = false;
  editingGenre: Genre | null = null;
  genreForm: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private genreService: GenreService,
    private authService: AuthService,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.genreForm = this.fb.group({
      genreName: ['', [Validators.required, Validators.minLength(2)]],
      description: ['']
    });
  }

  ngOnInit() {
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
    });
    this.initAudio();
    this.loadGenres();
  }

  ngOnDestroy(): void {
    this.disposeAudio();
  }

  loadGenres() {
    this.isLoadingGenres = true;
    this.genreService.getGenres().subscribe({
      next: (data: Genre[]) => {
        this.genres = Array.isArray(data) ? data : [];
        // 👉 Sắp xếp từ nhỏ đến lớn (1 → 10)
        this.genres.sort((a, b) => a.genreId - b.genreId);
        this.filteredGenres = [...this.genres];
        this.isLoadingGenres = false;
        this.genreError = null;

        if (this.genres.length === 0) {
          this.genreError = 'Chưa có thể loại nào. Hãy thêm mới!';
        }
      },
      error: (err: any) => {
        console.error('Lỗi load genres:', err);
        this.genreError = 'Không thể tải danh sách thể loại.';
        this.isLoadingGenres = false;
      }
    });
  }
  
  onSearch(): void {
    if (!this.searchQuery.trim()) {
      this.filteredGenres = [...this.genres];
      return;
    }
    
    const query = this.searchQuery.toLowerCase().trim();
    this.filteredGenres = this.genres.filter(genre =>
      genre.genreName.toLowerCase().includes(query) ||
      (genre.description && genre.description.toLowerCase().includes(query))
    );
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.filteredGenres = [...this.genres];
  }

  openCreateModal() {
    this.isEditMode = false;
    this.editingGenre = null;
    this.genreForm.reset();
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditModal(genre: Genre) {
    this.isEditMode = true;
    this.editingGenre = genre;
    this.genreForm.patchValue({
      genreName: genre.genreName,
      description: genre.description || ''
    });
    this.showModal = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  closeModal() {
    this.showModal = false;
    this.genreForm.reset();
    this.isEditMode = false;
    this.editingGenre = null;
    this.errorMessage = '';
    this.successMessage = '';
  }

  onSubmit() {
    if (this.genreForm.invalid) {
      this.genreForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formValue = this.genreForm.value;

    if (this.isEditMode && this.editingGenre) {
      // Update
      const updateRequest: UpdateGenreRequest = {
        genreName: formValue.genreName,
        description: formValue.description
      };

      this.genreService.updateGenre(this.editingGenre.genreId, updateRequest).subscribe({
        next: () => {
          this.showSuccess('Cập nhật thể loại thành công!');
          this.loadGenres();
          setTimeout(() => this.closeModal(), 1500);
        },
        error: (err) => {
          this.showError(err.error?.message || 'Cập nhật thất bại. Vui lòng thử lại.');
          this.loading = false;
        }
      });
    } else {
      // Create
      const createRequest: CreateGenreRequest = {
        genreName: formValue.genreName,
        description: formValue.description
      };

      this.genreService.createGenre(createRequest).subscribe({
        next: () => {
          this.showSuccess('Tạo thể loại thành công!');
          this.loadGenres();
          setTimeout(() => this.closeModal(), 1500);
        },
        error: (err) => {
          this.showError(err.error?.message || 'Tạo thất bại. Vui lòng thử lại.');
          this.loading = false;
        }
      });
    }
  }

  deleteGenre(genre: Genre) {
    if (!confirm(`Bạn có chắc chắn muốn xóa thể loại "${genre.genreName}"?`)) {
      return;
    }

    this.genreService.deleteGenre(genre.genreId).subscribe({
      next: () => {
        this.showSuccess('Xóa thể loại thành công!');
        this.loadGenres();
      },
      error: (err) => {
        this.showError(err.error?.message || 'Xóa thất bại. Vui lòng thử lại.');
      }
    });
  }

  showError(message: string) {
    this.errorMessage = message;
    this.loading = false;
    setTimeout(() => this.errorMessage = '', 5000);
  }

  showSuccess(message: string) {
    this.successMessage = message;
    this.loading = false;
    setTimeout(() => this.successMessage = '', 5000);
  }

  get genreName() {
    return this.genreForm.get('genreName');
  }

  get description() {
    return this.genreForm.get('description');
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  // Player methods
  togglePlayback() {
    if (!this.audio) {
      return;
    }

    if (this.isPlaying) {
      this.audio.pause();
      this.isPlaying = false;
    } else {
      this.audio.play().then(() => {
        this.isPlaying = true;
      }).catch(() => {
        this.genreError = 'Không thể tiếp tục phát bài hát.';
      });
    }
  }

  seekTo(positionInSeconds: number) {
    if (!this.audio || Number.isNaN(positionInSeconds)) {
      return;
    }

    this.audio.currentTime = positionInSeconds;
    this.currentTime = positionInSeconds;
  }

  changeVolume(newVolume: number) {
    this.volume = newVolume;
    if (this.audio) {
      this.audio.volume = this.volume / 100;
    }
  }

  toggleMute() {
    if (!this.audio) {
      return;
    }
    if (this.audio.volume > 0) {
      this.audio.volume = 0;
    } else {
      this.audio.volume = this.volume / 100;
    }
  }

  replayCurrentTrack() {
    if (!this.audio) {
      return;
    }

    this.audio.currentTime = 0;
    this.currentTime = 0;
    this.progress = 0;

    this.audio.play().then(() => {
      this.isPlaying = true;
    }).catch(() => {
      this.genreError = 'Không thể phát lại bài hát.';
    });
  }

  shufflePlay() {
    // No-op: This page doesn't have songs to shuffle
  }

  playPreviousSong() {
    // No-op: This page doesn't have songs to play
  }

  playNextSong() {
    // No-op: This page doesn't have songs to play
  }

  private initAudio() {
    if (this.audio) {
      return;
    }

    this.audio = new Audio();
    this.audio.addEventListener('timeupdate', this.onTimeUpdate);
    this.audio.addEventListener('ended', this.onEnded);
    this.audio.addEventListener('loadedmetadata', this.onLoadedMetadata);
  }

  private disposeAudio() {
    if (!this.audio) {
      return;
    }
    this.audio.pause();
    this.audio.removeEventListener('timeupdate', this.onTimeUpdate);
    this.audio.removeEventListener('ended', this.onEnded);
    this.audio.removeEventListener('loadedmetadata', this.onLoadedMetadata);
    this.audio = undefined;
  }

}
