import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';

export interface Playlist {
  playlistId: number;
  playlistName: string;
  description?: string;
  coverImageUrl?: string;
  isPublic: boolean;
  createdAt: string;
  updatedAt: string;
  songCount: number;
  ownerId?: number;
  ownerName?: string;
}

export interface PlaylistSong {
  songId: number;
  songTitle: string;
  artistId: number;
  artistName: string;
  albumId?: number;
  albumTitle?: string;
  genreId?: number;
  genreName?: string;
  audioFileUrl: string;
  coverImageUrl?: string;
  durationSeconds: number;
  playCount: number;
  likeCount: number;
  position: number;
  addedAt: string;
}

export interface PlaylistDetail {
  playlistId: number;
  playlistName: string;
  description?: string;
  coverImageUrl?: string;
  isPublic: boolean;
  createdAt: string;
  updatedAt: string;
  songs: PlaylistSong[];
  ownerId?: number;
  ownerName?: string;
}

export interface CreatePlaylistRequest {
  playlistName: string;
  description?: string;
  coverImageUrl?: string;
  isPublic?: boolean;
}

export interface UpdatePlaylistRequest {
  playlistName: string;
  description?: string;
  coverImageUrl?: string;
  isPublic?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PlaylistsService {
  private apiUrl = 'https://localhost:5001/api/Playlists';
  private playlistCreated$ = new Subject<Playlist>();
  private playlistSongChanged$ = new Subject<{ playlistId: number; songId: number; action: 'added' | 'removed' }>();

  constructor(private http: HttpClient) { }

  // Observable để lắng nghe khi có playlist mới được tạo
  get onPlaylistCreated(): Observable<Playlist> {
    return this.playlistCreated$.asObservable();
  }

  // Observable để lắng nghe khi bài hát được thêm/xóa khỏi playlist
  get onPlaylistSongChanged(): Observable<{ playlistId: number; songId: number; action: 'added' | 'removed' }> {
    return this.playlistSongChanged$.asObservable();
  }

  getPlaylists(): Observable<Playlist[]> {
    return this.http.get<Playlist[]>(this.apiUrl);
  }

  getPlaylist(id: number): Observable<PlaylistDetail> {
    return this.http.get<PlaylistDetail>(`${this.apiUrl}/${id}`);
  }

  createPlaylist(request: CreatePlaylistRequest): Observable<Playlist> {
    return new Observable(observer => {
      this.http.post<Playlist>(this.apiUrl, request).subscribe({
        next: (playlist) => {
          // Emit event khi playlist được tạo thành công
          this.playlistCreated$.next(playlist);
          observer.next(playlist);
          observer.complete();
        },
        error: (error) => {
          observer.error(error);
        }
      });
    });
  }

  updatePlaylist(id: number, request: UpdatePlaylistRequest): Observable<Playlist> {
    return this.http.put<Playlist>(`${this.apiUrl}/${id}`, request);
  }

  deletePlaylist(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  addSongToPlaylist(playlistId: number, songId: number): Observable<any> {
    return new Observable(observer => {
      this.http.post<any>(`${this.apiUrl}/${playlistId}/songs`, { songId }).subscribe({
        next: (result) => {
          // Emit event khi bài hát được thêm thành công
          this.playlistSongChanged$.next({ playlistId, songId, action: 'added' });
          observer.next(result);
          observer.complete();
        },
        error: (error) => {
          observer.error(error);
        }
      });
    });
  }

  removeSongFromPlaylist(playlistId: number, songId: number): Observable<any> {
    return new Observable(observer => {
      this.http.delete<any>(`${this.apiUrl}/${playlistId}/songs/${songId}`).subscribe({
        next: (result) => {
          // Emit event khi bài hát được xóa thành công
          this.playlistSongChanged$.next({ playlistId, songId, action: 'removed' });
          observer.next(result);
          observer.complete();
        },
        error: (error) => {
          observer.error(error);
        }
      });
    });
  }

  searchSongs(search?: string): Observable<SearchSong[]> {
    let url = `${this.apiUrl}/search-songs`;
    if (search) {
      url += `?search=${encodeURIComponent(search)}`;
    }
    return this.http.get<SearchSong[]>(url);
  }

  uploadPlaylistCover(playlistId: number, coverImage: File): Observable<Playlist> {
    const formData = new FormData();
    formData.append('coverImage', coverImage);
    return this.http.post<Playlist>(`${this.apiUrl}/${playlistId}/upload-cover`, formData);
  }

  getPublicPlaylists(): Observable<Playlist[]> {
    return this.http.get<Playlist[]>(`${this.apiUrl}/public`);
  }
}

export interface SearchSong {
  songId: number;
  songTitle: string;
  artistId: number;
  artistName: string;
}

