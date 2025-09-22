import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface FavoriteSong {
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
  releaseDate?: string;
  playCount: number;
  likeCount: number;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class FavoritesService {
  private apiUrl = `${environment.apiUrl}/favorites`;
  private favoriteSongIdsSubject = new BehaviorSubject<Set<number>>(new Set());
  public favoriteSongIds$ = this.favoriteSongIdsSubject.asObservable();

  constructor(private http: HttpClient) { }

  loadFavorites(): void {
    this.getFavoriteSongs().subscribe({
      next: (favorites) => {
        const songIds = new Set(favorites.map(f => f.songId));
        this.favoriteSongIdsSubject.next(songIds);
      },
      error: (error) => {
        console.error('Error loading favorites:', error);
      }
    });
  }

  isFavorite(songId: number): boolean {
    return this.favoriteSongIdsSubject.value.has(songId);
  }

  getFavoriteSongs(): Observable<FavoriteSong[]> {
    return this.http.get<FavoriteSong[]>(`${this.apiUrl}/songs`);
  }

  addToFavorites(songId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/songs/${songId}`, {}).pipe(
      tap(() => {
        const currentFavorites = new Set(this.favoriteSongIdsSubject.value);
        currentFavorites.add(songId);
        this.favoriteSongIdsSubject.next(currentFavorites);
      })
    );
  }

  removeFromFavorites(songId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/songs/${songId}`).pipe(
      tap(() => {
        const currentFavorites = new Set(this.favoriteSongIdsSubject.value);
        currentFavorites.delete(songId);
        this.favoriteSongIdsSubject.next(currentFavorites);
      })
    );
  }
}

