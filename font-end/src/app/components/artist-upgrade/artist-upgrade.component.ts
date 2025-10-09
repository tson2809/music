import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ArtistUpgradeService, SubmitUpgradeRequest, UpgradeRequest } from '../../services/artist-upgrade.service';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { PlayerBarComponent, CurrentTrack } from '../player-bar/player-bar.component';
import { PlayerService } from '../../services/player.service';
import { Subscription, combineLatest } from 'rxjs';

@Component({
  selector: 'app-artist-upgrade',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, SidebarComponent, SearchHeaderComponent, PlayerBarComponent],
  templateUrl: './artist-upgrade.component.html',
  styleUrls: ['./artist-upgrade.component.css']
})
export class ArtistUpgradeComponent implements OnInit, OnDestroy {
  upgradeForm: FormGroup;
  currentUser: User | null = null;
  loading = false;
  errorMessage = '';
  successMessage = '';
  existingRequest: UpgradeRequest | null = null;
  checkingRequest = false;
  
  // Player bar properties
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
  showLyrics = false;
  
  private subscriptions = new Subscription();

  constructor(
    private fb: FormBuilder,
    private upgradeService: ArtistUpgradeService,
    private authService: AuthService,
    private router: Router,
    private playerService: PlayerService
  ) {
    this.upgradeForm = this.fb.group({
      artistName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      biography: ['', [Validators.maxLength(2000)]],
      approvalReason: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]]
    });
  }

  ngOnInit() {
    const authSub = this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
    });
    this.subscriptions.add(authSub);
    
    // Check if user is already an artist
    if (this.currentUser?.artistId) {
      this.router.navigate(['/home']);
      return;
    }

    // Check if user is admin
    if (this.currentUser?.roleId === 1) {
      this.router.navigate(['/home']);
      return;
    }

    // Check for existing request
    this.checkExistingRequest();
    
    // Subscribe to player state
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

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  checkExistingRequest() {
    this.checkingRequest = true;
    this.upgradeService.getMyRequest().subscribe({
      next: (request) => {
        this.existingRequest = request;
        this.checkingRequest = false;
        
        // If approved, redirect to home
        if (request.status === 'Approved') {
          this.router.navigate(['/home']);
        }
      },
      error: (error) => {
        // 404 means no request exists, which is fine
        if (error.status !== 404) {
          console.error('Error checking existing request:', error);
        }
        this.checkingRequest = false;
      }
    });
  }

  get artistName() {
    return this.upgradeForm.get('artistName');
  }

  get biography() {
    return this.upgradeForm.get('biography');
  }

  get approvalReason() {
    return this.upgradeForm.get('approvalReason');
  }

  onSubmit() {
    if (this.upgradeForm.invalid) {
      this.upgradeForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const request: SubmitUpgradeRequest = {
      artistName: this.upgradeForm.value.artistName.trim(),
      biography: this.upgradeForm.value.biography?.trim() || undefined,
      approvalReason: this.upgradeForm.value.approvalReason.trim()
    };

    this.upgradeService.submitRequest(request).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = response.message || 'Gửi yêu cầu nâng cấp thành công!';
        this.upgradeForm.reset();
        // Refresh existing request
        this.checkExistingRequest();
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Gửi yêu cầu thất bại. Vui lòng thử lại.';
      }
    });
  }

  getStatusText(status: string): string {
    switch (status) {
      case 'Pending':
        return 'Đang chờ duyệt';
      case 'Approved':
        return 'Đã được duyệt';
      case 'Rejected':
        return 'Đã bị từ chối';
      default:
        return status;
    }
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pending':
        return 'status-pending';
      case 'Approved':
        return 'status-approved';
      case 'Rejected':
        return 'status-rejected';
      default:
        return '';
    }
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  // Player bar methods
  onPlayPause() {
    this.playerService.togglePlayback();
  }

  onSeek(progress: number) {
    // Convert progress percentage to seconds
    const duration = this.duration;
    if (duration > 0) {
      const positionInSeconds = (progress / 100) * duration;
      this.playerService.seekTo(positionInSeconds);
    }
  }

  onVolumeChange(volume: number) {
    this.playerService.changeVolume(volume);
  }

  onReplay() {
    this.playerService.replayCurrentTrack();
  }

  onShuffle() {
    this.playerService.shufflePlay();
  }

  onMuteToggle() {
    this.playerService.toggleMute();
  }

  onPrevious() {
    this.playerService.playPrevious();
  }

  onNext() {
    this.playerService.playNext();
  }

  onToggleFavorite() {
    this.playerService.toggleFavorite();
  }
}

