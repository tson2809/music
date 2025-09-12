import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { HomeComponent } from './components/home/home.component';
import { UploadComponent } from './components/upload/upload.component';
import { ForgotPasswordComponent } from './components/forgot-password/forgot-password.component';
import { AccountComponent } from './components/account/account.component';
import { AuthGuard } from './guards/auth.guard';
import { AdminGuard } from './guards/admin.guard';
import { ArtistGuard } from './guards/artist.guard';
import { GenreListComponent } from './components/genre-list/genre-list.component';
import { SongManagementComponent } from './components/song-management/song-management.component';
import { SongApprovalComponent } from './components/song-approval/song-approval';
import { MySongsComponent } from './components/my-songs/my-songs';
import { FavoritesComponent } from './components/favorites/favorites.component';
import { PlaylistsComponent } from './components/playlists/playlists.component';
import { PlaylistDetailComponent } from './components/playlist-detail/playlist-detail.component';
import { AlbumManagementComponent } from './components/album-management/album-management.component';
import { AlbumDetailComponent } from './components/album-detail/album-detail.component';
import { UserManagementComponent } from './components/user-management/user-management.component';
import { SongDetailComponent } from './components/song-detail/song-detail';
import { ArtistDetailComponent } from './components/artist-detail/artist-detail.component';
import { AdminAlbumManagementComponent } from './components/admin-album-management/admin-album-management.component';
import { StatisticsComponent } from './components/statistics/statistics.component';
import { BrowseGenresComponent } from './components/browse-genres/browse-genres';
import { AlbumLibraryComponent } from './components/album-library/album-library.component';
import { ArtistStatisticsComponent } from './components/artist-statistics/artist-statistics.component';
import { ArtistUpgradeComponent } from './components/artist-upgrade/artist-upgrade.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'home', component: HomeComponent },
  { path: 'albums-user', component: AlbumLibraryComponent },
  { path: 'songs/:id', component: SongDetailComponent },
  { path: 'artists/:id', component: ArtistDetailComponent },
  { path: 'favorites', component: FavoritesComponent, canActivate: [AuthGuard] },
  { path: 'playlists', component: PlaylistsComponent, canActivate: [AuthGuard] },
  { path: 'playlists/:id', component: PlaylistDetailComponent, canActivate: [AuthGuard] },
  { path: 'genres', component: GenreListComponent },
  { path: 'browse-genres', component: BrowseGenresComponent },
  { path: 'upload', component: UploadComponent, canActivate: [ArtistGuard] },
  { path: 'my-songs', component: MySongsComponent, canActivate: [ArtistGuard] },
  { path: 'albums', component: AlbumManagementComponent, canActivate: [ArtistGuard] },
  { path: 'artist-statistics', component: ArtistStatisticsComponent, canActivate: [ArtistGuard] },
  { path: 'albums/:id', component: AlbumDetailComponent, canActivate: [ArtistGuard], data: { mode: 'manage' } },
  { path: 'albums-user/:id', component: AlbumDetailComponent, data: { mode: 'public' } },
  { path: 'songs', component: SongManagementComponent, canActivate: [AdminGuard] },
  { path: 'song-approval', component: SongApprovalComponent, canActivate: [AdminGuard] },
  { path: 'users', component: UserManagementComponent, canActivate: [AdminGuard] },
  { path: 'admin-albums', component: AdminAlbumManagementComponent, canActivate: [AdminGuard] },
  { path: 'statistics', component: StatisticsComponent, canActivate: [AdminGuard] },
  { path: 'account', component: AccountComponent, canActivate: [AuthGuard] },
  { path: 'upgrade-account', component: ArtistUpgradeComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '/login' }
];

