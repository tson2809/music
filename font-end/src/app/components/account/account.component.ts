import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule, NgIf } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subscription, combineLatest } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { environment } from '../../../environments/environment';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { PlayerBarComponent, CurrentTrack } from '../player-bar/player-bar.component';
import { PlayerService } from '../../services/player.service';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [CommonModule, NgIf, ReactiveFormsModule, SidebarComponent, SearchHeaderComponent, PlayerBarComponent],
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css']
})
export class AccountComponent implements OnInit, OnDestroy {
  profileForm: FormGroup;
  currentUser: User | null = null;
  loading = false;
  errorMessage = '';
  passwordErrorMessage = '';
  successMessage = '';
  public avatarUploading = false;
  currentTrack: CurrentTrack = {
    name: 'Chọn bài hát để phát',
    artist: 'Nghệ sĩ'
  };
  progress = 0;
  volume = 70;
  isPlaying = false;
  currentTime = 0;
  duration = 0;
  currentSongId: number | null = null;
  isFavorite = false;
  bufferedProgress = 0;
  pendingAvatarFile: File | null = null;
  avatarPreviewUrl: string | null = null;
  private avatarPreviewObjectUrl: string | null = null;

  private subscriptions = new Subscription();
  @ViewChild('avatarInput') avatarInput?: ElementRef<HTMLInputElement>;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private http: HttpClient,
    private router: Router,
    private playerService: PlayerService
  ) {
    this.profileForm = this.fb.group({
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      dateOfBirth: [''],
      country: [''],
      newPassword: ['', [Validators.minLength(6)]],
      confirmPassword: ['']
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword');
    const confirmPassword = control.get('confirmPassword');
    
    // Chỉ validate nếu có nhập mật khẩu mới
    if (newPassword && confirmPassword && newPassword.value && confirmPassword.value && newPassword.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  ngOnInit(): void {
    const authSub = this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
      if (this.currentUser) {
        this.loadProfile();
      }
    });
    this.subscriptions.add(authSub);

    const playerSub = combineLatest([
      this.playerService.currentTrack$,
      this.playerService.isPlaying$,
      this.playerService.currentTime$,
      this.playerService.duration$,
      this.playerService.progress$,
      this.playerService.volume$,
      this.playerService.currentSongId$,
      this.playerService.isFavorite$,
      this.playerService.bufferedProgress$
    ]).subscribe(([track, playing, time, dur, prog, vol, songId, favorite, buffered]) => {
      this.currentTrack = track;
      this.isPlaying = playing;
      this.currentTime = time;
      this.duration = dur;
      this.progress = prog;
      this.volume = vol;
      this.currentSongId = songId;
      this.isFavorite = favorite;
      this.bufferedProgress = buffered;
    });
    this.subscriptions.add(playerSub);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadProfile() {
    if (!this.currentUser) return;

    // Xử lý dateOfBirth để tránh lệch timezone
    let dateOfBirthValue = '';
    if (this.currentUser.dateOfBirth) {
      const date = new Date(this.currentUser.dateOfBirth);
      // Lấy ngày tháng năm theo local timezone để tránh bị lệch
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      dateOfBirthValue = `${year}-${month}-${day}`;
    }

    this.profileForm.patchValue({
      fullName: this.currentUser.fullName || '',
      email: this.currentUser.email || '',
      dateOfBirth: dateOfBirthValue,
      country: this.currentUser.country || ''
    });

    this.clearPendingAvatar();
  }

  cancelEdit() {
    this.router.navigate(['/home']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  public getAvatarUrl(path?: string | null): string | null {
    if (!path) {
      return null;
    }
    const normalizedPath = path.trim();
    if (normalizedPath.startsWith('http://') || normalizedPath.startsWith('https://')) {
      return normalizedPath;
    }

    let baseOrigin = '';
    if (environment.apiUrl.startsWith('http')) {
      try {
        const apiUrl = new URL(environment.apiUrl, window.location.origin);
        baseOrigin = apiUrl.origin.replace(/\/$/, '');
      } catch {
        baseOrigin = window.location.origin;
      }
    } else {
      baseOrigin = window.location.origin;
    }

    const cleanedPath = normalizedPath.startsWith('/')
      ? normalizedPath
      : `/${normalizedPath}`;

    return `${baseOrigin}${cleanedPath}`;
  }

  public getDisplayedAvatarUrl(): string | null {
    if (this.avatarPreviewUrl) {
      return this.avatarPreviewUrl;
    }
    return this.getAvatarUrl(this.currentUser?.profilePictureUrl);
  }

  public triggerAvatarSelect(): void {
    if (this.avatarUploading) {
      return;
    }
    this.avatarInput?.nativeElement.click();
  }

  public onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.currentUser) {
      return;
    }
    this.setPendingAvatar(file);
    input.value = '';
  }

  private setPendingAvatar(file: File): void {
    this.pendingAvatarFile = file;
    if (this.avatarPreviewObjectUrl) {
      URL.revokeObjectURL(this.avatarPreviewObjectUrl);
    }
    this.avatarPreviewObjectUrl = URL.createObjectURL(file);
    this.avatarPreviewUrl = this.avatarPreviewObjectUrl;
  }

  private clearPendingAvatar(): void {
    this.pendingAvatarFile = null;
    if (this.avatarPreviewObjectUrl) {
      URL.revokeObjectURL(this.avatarPreviewObjectUrl);
      this.avatarPreviewObjectUrl = null;
    }
    this.avatarPreviewUrl = null;
  }

  private uploadAvatar(file: File) {
    if (!this.currentUser) {
      return null;
    }
    const formData = new FormData();
    formData.append('avatar', file);
    return this.http.post<User>(
      `${environment.apiUrl}/auth/profile/${this.currentUser.userId}/avatar`,
      formData
    );
  }

  onPlayPause(): void {
    this.playerService.togglePlayback();
  }

  onSeek(position: number): void {
    this.playerService.seekTo(position);
  }

  onVolumeChange(volume: number): void {
    this.playerService.changeVolume(volume);
  }

  onReplay(): void {
    this.playerService.replayCurrentTrack();
  }

  onShuffle(): void {
    this.playerService.shufflePlay();
  }

  onMuteToggle(): void {
    this.playerService.toggleMute();
  }

  onPrevious(): void {
    this.playerService.playPrevious();
  }

  onNext(): void {
    this.playerService.playNext();
  }

  onToggleFavorite(): void {
    this.playerService.toggleFavorite();
  }

  onSubmit() {
    if (this.profileForm.invalid || !this.currentUser) {
      this.profileForm.markAllAsTouched();
      return;
    }

    // Validate password nếu có nhập mật khẩu mới
    const newPassword = this.profileForm.value.newPassword?.trim();
    const confirmPassword = this.profileForm.value.confirmPassword?.trim();
    
    if (newPassword || confirmPassword) {
      // Nếu có nhập một trong hai thì phải nhập cả hai
      if (!newPassword) {
        this.passwordErrorMessage = 'Vui lòng nhập mật khẩu mới';
        this.profileForm.get('newPassword')?.markAsTouched();
        return;
      }
      if (!confirmPassword) {
        this.passwordErrorMessage = 'Vui lòng xác nhận mật khẩu mới';
        this.profileForm.get('confirmPassword')?.markAsTouched();
        return;
      }
      // Kiểm tra độ dài mật khẩu
      if (newPassword.length < 6) {
        this.passwordErrorMessage = 'Mật khẩu phải có ít nhất 6 ký tự';
        this.profileForm.get('newPassword')?.markAsTouched();
        return;
      }
      // Kiểm tra mật khẩu khớp
      if (newPassword !== confirmPassword) {
        this.passwordErrorMessage = 'Mật khẩu xác nhận không khớp';
        this.profileForm.get('confirmPassword')?.markAsTouched();
        return;
      }
    }

    this.loading = true;
    this.errorMessage = '';
    this.passwordErrorMessage = '';
    this.successMessage = '';

    const updateData = {
      fullName: this.profileForm.value.fullName,
      email: this.profileForm.value.email,
      dateOfBirth: this.profileForm.value.dateOfBirth || null,
      country: this.profileForm.value.country || null
    };

    // Update profile
    this.http.put(
      `${environment.apiUrl}/auth/profile/${this.currentUser.userId}`,
      updateData
    ).subscribe({
      next: (response: any) => {
        let updatedUser = { ...this.currentUser, ...response };
        const afterAvatarUpload = () => {
          const newPassword = this.profileForm.value.newPassword?.trim();
          if (newPassword && newPassword !== '') {
            this.changePassword();
          } else {
            this.loading = false;
            this.currentUser = updatedUser;
            this.authService.updateUser(updatedUser);
            this.loadProfile();
            this.successMessage = 'Cập nhật thông tin thành công!';
            setTimeout(() => {
              this.successMessage = '';
            }, 3000);
          }
        };

        if (this.pendingAvatarFile) {
          this.avatarUploading = true;
          const upload$ = this.uploadAvatar(this.pendingAvatarFile);
          if (upload$) {
            upload$.subscribe({
              next: (avatarResponse) => {
                this.avatarUploading = false;
                const finalUser = { ...updatedUser, ...avatarResponse };
                updatedUser = finalUser;
                this.currentUser = finalUser;
                this.authService.updateUser(finalUser);
                this.clearPendingAvatar();
                afterAvatarUpload();
              },
              error: (error) => {
                this.avatarUploading = false;
                this.loading = false;
                this.errorMessage = error.error?.message || 'Không thể cập nhật ảnh đại diện. Vui lòng thử lại!';
              }
            });
          }
        } else {
          afterAvatarUpload();
        }
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Có lỗi xảy ra. Vui lòng thử lại!';
      }
    });
  }

  changePassword() {
    if (!this.currentUser) {
      this.loading = false;
      return;
    }

    const newPassword = this.profileForm.value.newPassword?.trim();
    if (!newPassword || newPassword === '') {
      this.loading = false;
      this.passwordErrorMessage = 'Vui lòng nhập mật khẩu mới';
      return;
    }

    const changePasswordData = {
      newPassword: newPassword
    };

    const url = `${environment.apiUrl}/auth/change-password/${this.currentUser.userId}`;
    console.log('Calling change password API:', url, changePasswordData);
    
    this.http.post<any>(
      url,
      changePasswordData,
      { headers: { 'Content-Type': 'application/json' } }
    ).subscribe({
      next: (response) => {
        console.log('Change password success:', response);
        this.loading = false;
        // Reset password fields
        this.profileForm.patchValue({
          newPassword: '',
          confirmPassword: ''
        });
        this.profileForm.get('newPassword')?.markAsUntouched();
        this.profileForm.get('confirmPassword')?.markAsUntouched();
        this.passwordErrorMessage = '';
        // Cập nhật user trong AuthService
        this.http.get(
          `${environment.apiUrl}/auth/profile/${this.currentUser?.userId}`
        ).subscribe({
          next: (response: any) => {
            const updatedUser = { ...this.currentUser, ...response };
            this.authService.updateUser(updatedUser);
            this.currentUser = updatedUser;
            // Reload form với giá trị mới để đảm bảo date hiển thị đúng
            this.loadProfile();
            // Hiển thị thông báo thành công
            this.successMessage = 'Cập nhật thông tin và mật khẩu thành công!';
            // Tự động ẩn thông báo sau 3 giây
            setTimeout(() => {
              this.successMessage = '';
            }, 3000);
          }
        });
      },
      error: (error) => {
        console.error('Change password error:', error);
        this.loading = false;
        this.passwordErrorMessage = error.error?.message || error.message || 'Có lỗi xảy ra khi đổi mật khẩu. Vui lòng thử lại!';
      }
    });
  }

  backToHome() {
    this.router.navigate(['/home']);
  }
}

