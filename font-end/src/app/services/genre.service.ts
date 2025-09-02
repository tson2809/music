import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Genre } from '../models/genre.model';

export interface CreateGenreRequest {
  genreName: string;
  description?: string;
}

export interface UpdateGenreRequest {
  genreName: string;
  description?: string;
}

@Injectable({
  providedIn: 'root'
})
export class GenreService {

  private apiUrl = `${environment.apiUrl}/Genres`;

  constructor(private http: HttpClient) {}

  getGenres(): Observable<Genre[]> {
    return this.http.get<Genre[]>(this.apiUrl);
  }

  getGenre(id: number): Observable<Genre> {
    return this.http.get<Genre>(`${this.apiUrl}/${id}`);
  }

  createGenre(request: CreateGenreRequest): Observable<Genre> {
    return this.http.post<Genre>(this.apiUrl, request);
  }

  updateGenre(id: number, request: UpdateGenreRequest): Observable<Genre> {
    return this.http.put<Genre>(`${this.apiUrl}/${id}`, request);
  }

  deleteGenre(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
