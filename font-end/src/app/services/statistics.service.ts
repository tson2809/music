import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface StatisticsOverview {
  totalUsers: number;
  totalArtists: number;
  totalSongs: number;
  totalAlbums: number;
  approvedSongs: number;
  pendingSongs: number;
  rejectedSongs: number;
}

export interface GenreStatistics {
  genreId: number;
  genreName: string;
  songCount: number;
  totalPlayCount: number;
  totalLikeCount: number;
}

export interface ListeningHistory {
  historyId: number;
  userId: number;
  userName: string;
  songId: number;
  songTitle: string;
  artistName: string;
  playedAt: string;
  durationPlayed?: number;
  completed: boolean;
}

export interface ListeningHistoryResponse {
  history: ListeningHistory[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface User {
  userId: number;
  username: string;
  email: string;
  fullName?: string;
  country?: string;
  roleId: number;
  roleName: string;
  isActive: boolean;
  dateOfBirth?: string;
  profilePictureUrl?: string;
}

export interface UsersListResponse {
  users: User[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface Artist {
  artistId: number;
  artistName: string;
  biography?: string;
  country?: string;
  verified: boolean;
  monthlyListeners: number;
  userId?: number;
  username?: string;
  email?: string;
  songCount: number;
  albumCount: number;
}

export interface ArtistsListResponse {
  artists: Artist[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface Song {
  songId: number;
  songTitle: string;
  artistId: number;
  artistName: string;
  albumId?: number;
  albumTitle?: string;
  genreId?: number;
  genreName?: string;
  durationSeconds: number;
  playCount: number;
  likeCount: number;
  approvalStatus: string;
  createdAt: string;
  releaseDate?: string;
  coverImageUrl?: string | null;
}

export interface SongsListResponse {
  songs: Song[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface Album {
  albumId: number;
  albumTitle: string;
  artistId: number;
  artistName: string;
  albumType: string;
  totalTracks: number;
  durationSeconds: number;
  releaseDate?: string;
  createdAt: string;
}

export interface AlbumsListResponse {
  albums: Album[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class StatisticsService {
  private apiUrl = `${environment.apiUrl}/Statistics`;

  constructor(private http: HttpClient) { }

  // GET: Get overview statistics
  getOverview(): Observable<StatisticsOverview> {
    return this.http.get<StatisticsOverview>(`${this.apiUrl}/overview`);
  }

  // GET: Get genre statistics
  getGenreStatistics(): Observable<GenreStatistics[]> {
    return this.http.get<GenreStatistics[]>(`${this.apiUrl}/genres`);
  }

  // GET: Get listening history
  getListeningHistory(page: number = 1, pageSize: number = 50): Observable<ListeningHistoryResponse> {
    return this.http.get<ListeningHistoryResponse>(`${this.apiUrl}/listening-history?page=${page}&pageSize=${pageSize}`);
  }

  // GET: Get users list
  getUsers(page: number = 1, pageSize: number = 20, search?: string): Observable<UsersListResponse> {
    let url = `${this.apiUrl}/users?page=${page}&pageSize=${pageSize}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    return this.http.get<UsersListResponse>(url);
  }

  // GET: Get artists list
  getArtists(page: number = 1, pageSize: number = 20, search?: string): Observable<ArtistsListResponse> {
    let url = `${this.apiUrl}/artists?page=${page}&pageSize=${pageSize}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    return this.http.get<ArtistsListResponse>(url);
  }

  // GET: Get songs list
  getSongs(page: number = 1, pageSize: number = 20, search?: string): Observable<SongsListResponse> {
    let url = `${this.apiUrl}/songs?page=${page}&pageSize=${pageSize}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    return this.http.get<SongsListResponse>(url);
  }

  // GET: Get albums list
  getAlbums(page: number = 1, pageSize: number = 20, search?: string): Observable<AlbumsListResponse> {
    let url = `${this.apiUrl}/albums?page=${page}&pageSize=${pageSize}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    return this.http.get<AlbumsListResponse>(url);
  }
}

