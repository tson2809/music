import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AlbumService } from '../../services/album.service';
import { SongService } from '../../services/song.service';
import { AuthService } from '../../services/auth.service';
import { PlayerService } from '../../services/player.service';
import { AlbumDetail, AlbumSong } from '../../models/album.model';
import { User } from '../../models/user.model';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { Subscription, combineLatest } from 'rxjs';

interface MySongOption {
  songId: number;
  songTitle: string;
  albumId?: number | null;
  approvalStatus?: string;
  durationSeconds?: number;
}

@Component({
  selector: 'app-album-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SidebarComponent, SearchHeaderComponent],
  templateUrl: './album-detail.component.html',
  styleUrls: ['./album-detail.component.css']
})
export class AlbumDetailComponent implements OnInit, OnDestroy {
  albumId: number | null = null;
  albumDetail: AlbumDetail | null = null;
  loading = true;
  errorMessage = '';
  successMessage = '';

  isManageMode = true;
  availableSongs: MySongOption[] = [];
  loadingAvailableSongs = false;
  selectedSongId: number | null = null;
  addingSong = false;
  removingSongId: number | null = null;

  currentArtistId: number | null = null;
  currentUser: User | null = null;

  currentSongId: number | null = null;
  isPlaying = false;

  private subscriptions = new Subscription();

  constructor(
    private route: ActivatedRoute,
    private albumService: AlbumService,
    private songService: SongService,
    private authService: AuthService,
    private playerService: PlayerService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isManageMode = (this.route.snapshot.data?.['mode'] ?? 'manage') !== 'public';

    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
      this.currentArtistId = state.user?.artistId || null;
    });

    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      this.albumId = idParam ? parseInt(idParam, 10) : null;
      if (!this.albumId) {
        this.errorMessage = 'Không xác định được album';
        return;
      }
      this.loadAlbumDetail();
      if (this.isManageMode) {
        this.loadAvailableSongs();
      } else {
        this.availableSongs = [];
      }
    });

    // Subscribe to player state
    const playerSub = combineLatest([
      this.playerService.currentSongId$,
      this.playerService.isPlaying$
    ]).subscribe(([songId, playing]) => {
      this.currentSongId = songId;
      this.isPlaying = playing;
    });
    this.subscriptions.add(playerSub);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadAlbumDetail(): void {
    if (!this.albumId) {
      return;
    }
    this.loading = true;
    this.albumService.getAlbumById(this.albumId).subscribe({
      next: (detail) => {
        this.albumDetail = detail;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading album detail:', error);
        this.errorMessage = error.error?.message || 'Không thể tải chi tiết album';
        this.loading = false;
      }
    });
  }

  loadAvailableSongs(): void {
    if (!this.isManageMode || !this.albumId) {
      return;
    }
    this.loadingAvailableSongs = true;
    this.songService.getMySongs(1, 200).subscribe({
      next: (response) => {
        const songs: MySongOption[] = Array.isArray(response?.songs) ? response.songs : [];
        this.availableSongs = songs.filter(song =>
          !song.albumId || song.albumId === null
        );
        this.loadingAvailableSongs = false;
      },
      error: (error) => {
        console.error('Error loading available songs:', error);
        this.availableSongs = [];
        this.loadingAvailableSongs = false;
      }
    });
  }

  addSongToAlbum(): void {
    if (!this.isManageMode) {
      return;
    }
    if (!this.albumDetail || this.selectedSongId === null) {
      this.errorMessage = 'Vui lòng chọn bài hát cần thêm';
      return;
    }

    const songId = Number(this.selectedSongId);
    if (!songId) {
      this.errorMessage = 'Lựa chọn bài hát không hợp lệ';
      return;
    }

    this.addingSong = true;
    this.songService.updateSongAlbum(songId, this.albumDetail.albumId).subscribe({
      next: () => {
        this.addingSong = false;
        this.successMessage = 'Đã thêm bài hát vào album';
        this.selectedSongId = null;
        this.loadAlbumDetail();
        this.loadAvailableSongs();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        console.error('Error adding song:', error);
        this.errorMessage = error.error?.message || 'Không thể thêm bài hát vào album';
        this.addingSong = false;
        setTimeout(() => this.errorMessage = '', 4000);
      }
    });
  }

  removeSongFromAlbum(song: AlbumSong): void {
    if (!this.isManageMode) {
      return;
    }
    if (!this.albumDetail) {
      return;
    }
    if (!confirm(`Bạn có chắc chắn muốn xóa "${song.songTitle}" khỏi album?`)) {
      return;
    }

    this.removingSongId = song.songId;
    this.songService.updateSongAlbum(song.songId, null).subscribe({
      next: () => {
        this.removingSongId = null;
        this.successMessage = 'Đã xóa bài hát khỏi album';
        this.loadAlbumDetail();
        this.loadAvailableSongs();
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (error) => {
        console.error('Error removing song:', error);
        this.errorMessage = error.error?.message || 'Không thể xóa bài hát khỏi album';
        this.removingSongId = null;
        setTimeout(() => this.errorMessage = '', 4000);
      }
    });
  }

  formatDuration(seconds?: number | null): string {
    if (!seconds || seconds <= 0) {
      return '--:--';
    }
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  formatDate(date?: string | null): string {
    if (!date) {
      return 'N/A';
    }
    return new Date(date).toLocaleDateString('vi-VN');
  }

  getImageUrl(url?: string | null): string {
    if (!url) {
      return 'https://via.placeholder.com/50x50?text=Song';
    }
    // If URL is already a full URL (starts with http:// or https://), use it directly
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }
    // If URL is a relative path (starts with images/), add base URL
    if (url.startsWith('images/')) {
      return `https://localhost:5001/${url}`;
    }
    // Otherwise, use as is
    return url;
  }

  goBack(): void {
    this.router.navigate([this.isManageMode ? '/albums' : '/albums-user']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  onSongPlayClick(event: Event, song: AlbumSong): void {
    event.stopPropagation();

    if (!this.currentUser) {
      const baseRoute = this.isManageMode ? 'albums' : 'albums-user';
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: `/${baseRoute}/${this.albumId}` }
      });
      return;
    }

    if (!this.albumDetail) {
      return;
    }

    const currentSongId = this.playerService.getCurrentSongId();
    if (currentSongId === song.songId) {
      this.playerService.togglePlayback();
      return;
    }

    // Convert AlbumSong to PlayableSong format
    const playableSong = {
      songId: song.songId,
      songTitle: song.songTitle,
      artistName: this.albumDetail.artistName,
      audioFileUrl: song.audioFileUrl,
      coverImageUrl: song.coverImageUrl || this.albumDetail.coverImageUrl || null,
      durationSeconds: song.durationSeconds
    };

    // Convert all album songs to playlist
    const playlist = this.albumDetail.songs.map(s => ({
      songId: s.songId,
      songTitle: s.songTitle,
      artistName: this.albumDetail!.artistName,
      audioFileUrl: s.audioFileUrl,
      coverImageUrl: s.coverImageUrl || this.albumDetail!.coverImageUrl || null,
      durationSeconds: s.durationSeconds
    }));

    // Play song with the album songs as playlist
    this.playerService.playSong(playableSong, playlist);
  }
}


