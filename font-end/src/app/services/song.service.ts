import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { SongResponse, Artist, Album, Genre } from '../models/song.model';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SongService {
  private apiUrl = `${environment.apiUrl}/song`;
  private managementApiUrl = `${environment.apiUrl}/songmanagement`;
  private homeApiUrl = `${environment.apiUrl}/home`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  // Upload endpoints
  uploadSong(formData: FormData): Observable<SongResponse> {
    return this.http.post<SongResponse>(`${this.apiUrl}/upload`, formData);
  }

  getArtists(): Observable<Artist[]> {
    return this.http.get<Artist[]>(`${this.apiUrl}/artists`);
  }

  getAlbums(artistId?: number): Observable<Album[]> {
    let url = `${this.apiUrl}/albums`;
    if (artistId) {
      url += `?artistId=${artistId}`;
    }
    return this.http.get<Album[]>(url);
  }

  getGenres(): Observable<Genre[]> {
    return this.http.get<Genre[]>(`${this.apiUrl}/genres`);
  }

  // Management endpoints
  getAllSongs(page: number = 1, pageSize: number = 20, search?: string, genreId?: number | null): Observable<any> {
    let url = `${this.managementApiUrl}/all?page=${page}&pageSize=${pageSize}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    if (genreId) {
      url += `&genreId=${genreId}`;
    }
    return this.http.get<any>(url);
  }

  getSongById(songId: number): Observable<any> {
    return this.http.get<any>(`${this.managementApiUrl}/${songId}`);
  }

  deleteSong(songId: number): Observable<any> {
    return this.http.delete<any>(`${this.managementApiUrl}/${songId}`);
  }

  // Approval endpoints
  getPendingSongs(page: number = 1, pageSize: number = 20, search?: string): Observable<any> {
    let url = `${this.managementApiUrl}/pending?page=${page}&pageSize=${pageSize}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    return this.http.get<any>(url);
  }

  approveSong(songId: number): Observable<any> {
    return this.http.put<any>(`${this.managementApiUrl}/approve/${songId}`, {});
  }

  rejectSong(songId: number, rejectionReason?: string): Observable<any> {
    return this.http.put<any>(`${this.managementApiUrl}/reject/${songId}`, { rejectionReason });
  }

  // Artist my songs endpoint
  getMySongs(page: number = 1, pageSize: number = 20, approvalStatus?: number): Observable<any> {
    let url = `${this.apiUrl}/my-songs?page=${page}&pageSize=${pageSize}`;
    if (approvalStatus !== undefined) {
      url += `&approvalStatus=${approvalStatus}`;
    }
    return this.http.get<any>(url);
  }

  // Public home songs endpoint (chỉ lấy bài đã duyệt)
  getSongs(page: number = 1, pageSize: number = 20, search?: string): Observable<any> {
    let url = `${this.homeApiUrl}/songs?page=${page}&pageSize=${pageSize}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    return this.http.get<any>(url);
  }


  // Public song detail endpoint (chỉ lấy bài đã duyệt)
  getSongDetail(songId: number): Observable<any> {
    return this.http.get<any>(`${this.homeApiUrl}/songs/${songId}`);
  }
  // Track play count when user listens to 30% of song
  trackPlay(songId: number, durationPlayed: number): Observable<any> {
    const token = this.authService.getToken();
    let headers: HttpHeaders | undefined;

    // Nếu có token thì gửi kèm Authorization header
    if (token) {
      headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`
      });
    }

    return this.http.post<any>(
      `${this.apiUrl}/track-play`,
      { songId, durationPlayed },
      headers ? { headers } : {}
    );

  }

  updateSongAlbum(songId: number, albumId: number | null): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/update-album`, {
      songId,
      albumId
    });
  }

  // Artist endpoints
  getArtistById(artistId: number): Observable<any> {
    return this.http.get<any>(`${this.homeApiUrl}/artists/${artistId}`);
  }

  getSongsByArtist(artistId: number, page: number = 1, pageSize: number = 50): Observable<any> {
    return this.http.get<any>(`${this.homeApiUrl}/artists/${artistId}/songs?page=${page}&pageSize=${pageSize}`);
  }

  updateArtist(artistId: number, formData: FormData): Observable<any> {
    return this.http.put<any>(`${environment.apiUrl}/artists/${artistId}`, formData);
  }

  // Like/Unlike song (tăng/giảm likeCount)
  likeSong(songId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${songId}/like`, {});
  }

  unlikeSong(songId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${songId}/like`);
  }

  // Check like status (user đã like bài này chưa)
  checkIfSongIsLiked(songId: number): Observable<boolean> {
    return this.http.get<{ isLiked: boolean }>(`${this.apiUrl}/${songId}/like-status`).pipe(
      map(response => response.isLiked)
    );
  }

  // Get recently played items
  getRecentlyPlayed(limit: number = 20): Observable<RecentlyPlayedResponse> {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    return this.http.get<RecentlyPlayedResponse>(`${this.homeApiUrl}/recently-played?limit=${limit}`, { headers });
  }
}

export interface RecentlyPlayedItem {
  type: string; // "song", "artist", "album", "playlist"
  id: number;
  title: string;
  subtitle: string;
  imageUrl?: string;
  playedAt: string;
}

export interface RecentlyPlayedResponse {
  items: RecentlyPlayedItem[];
  totalCount: number;
}

