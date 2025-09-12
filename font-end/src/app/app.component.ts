import { Component, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PlayerBarComponent } from './components/player-bar/player-bar.component';
import { PlayerService } from './services/player.service';
import { AuthService } from './services/auth.service';
import { LyricsService } from './services/lyrics.service';
import { Subscription, combineLatest, filter } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, PlayerBarComponent],
  template: `
    <div class="app-container">
      <router-outlet></router-outlet>
      <app-player-bar
        *ngIf="isAuthenticated && showPlayerBar"
        [currentTrack]="currentTrack"
        [progress]="progress"
        [volume]="volume"
        [isPlaying]="isPlaying"
        [currentTime]="currentTime"
        [duration]="duration"
        [currentSongId]="currentSongId"
        [isFavorite]="isFavorite"
        [bufferedProgress]="bufferedProgress"
        [showLyrics]="showLyrics"
        (playPause)="onPlayPause()"
        (seek)="onSeek($event)"
        (volumeChange)="onVolumeChange($event)"
        (replay)="onReplay()"
        (shuffle)="onShuffle()"
        (muteToggle)="onMuteToggle()"
        (previous)="onPrevious()"
        (next)="onNext()"
        (toggleFavorite)="onToggleFavorite()"
        (toggleLyrics)="onToggleLyrics()">
      </app-player-bar>
    </div>
  `,
  styles: [`
    .app-container {
      height: 100vh;
      overflow: hidden;
      display: flex;
      flex-direction: column;
    }
    app-player-bar {
      display: block;
    }
  `]
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'Music Stream';
  
  currentTrack = { name: 'Chọn bài hát để phát', artist: 'Nghệ sĩ' };
  progress = 0;
  volume = 70;
  isPlaying = false;
  currentTime = 0;
  duration = 0;
  currentSongId: number | null = null;
  isFavorite = false;
  bufferedProgress = 0;
  isAuthenticated = false;
  showPlayerBar = true;
  showLyrics = false;

  private subscriptions = new Subscription();

  constructor(
    private playerService: PlayerService,
    private authService: AuthService,
    private router: Router,
    private lyricsService: LyricsService
  ) {}

  ngOnInit(): void {
    // Subscribe to router events to hide player bar on login/forgot-password pages
    const routerSub = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      const url = event.urlAfterRedirects || event.url;
      this.showPlayerBar = !url.includes('/login') && !url.includes('/forgot-password');
    });
    this.subscriptions.add(routerSub);

    // Check initial route
    this.updatePlayerBarVisibility();

    // Subscribe to authentication state
    const authSub = this.authService.authState$.subscribe(state => {
      this.isAuthenticated = state.isAuthenticated;
      // Dừng nhạc khi đăng xuất
      if (!state.isAuthenticated) {
        this.playerService.stop();
      }
      this.updatePlayerBarVisibility();
    });
    this.subscriptions.add(authSub);

    // Subscribe to all player state observables
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

    // Subscribe to lyrics service
    const lyricsSub = this.lyricsService.showLyrics$.subscribe(show => {
      this.showLyrics = show;
    });
    this.subscriptions.add(lyricsSub);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
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

  onToggleLyrics(): void {
    this.lyricsService.toggleLyrics();
  }

  private updatePlayerBarVisibility(): void {
    const url = this.router.url;
    this.showPlayerBar = this.isAuthenticated && !url.includes('/login') && !url.includes('/forgot-password');
  }
}

