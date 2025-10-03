import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Album, AlbumDetail, AlbumListResponse, CreateAlbumRequest, UpdateAlbumRequest } from '../models/album.model';

@Injectable({
  providedIn: 'root'
})
export class AlbumService {
  private apiUrl = 'https://localhost:5001/api/albums';
  private publicApiUrl = 'https://localhost:5001/api/home/albums';

  constructor(private http: HttpClient) { }

  // GET: Get all albums with pagination and filters (artist area)
  getAlbums(page: number = 1, pageSize: number = 20, artistId?: number, search?: string): Observable<AlbumListResponse> {
    let url = `${this.apiUrl}?page=${page}&pageSize=${pageSize}`;
    if (artistId) {
      url += `&artistId=${artistId}`;
    }
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    return this.http.get<AlbumListResponse>(url);
  }

  // GET: Public albums for users (home/library)
  getAllAlbumsPublic(page: number = 1, pageSize: number = 20, search?: string): Observable<AlbumListResponse> {
    let url = `${this.publicApiUrl}?page=${page}&pageSize=${pageSize}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    return this.http.get<AlbumListResponse>(url);
  }

  // GET: Get album by ID
  getAlbumById(albumId: number): Observable<AlbumDetail> {
    return this.http.get<AlbumDetail>(`${this.apiUrl}/${albumId}`);
  }

  // POST: Create new album
  createAlbum(request: CreateAlbumRequest): Observable<Album> {
    const formData = new FormData();
    formData.append('albumTitle', request.albumTitle);
    formData.append('artistId', request.artistId.toString());
    
    if (request.releaseDate) {
      formData.append('releaseDate', request.releaseDate);
    }
    if (request.albumType) {
      formData.append('albumType', request.albumType);
    }
    if (request.coverImageFile) {
      formData.append('coverImageFile', request.coverImageFile);
    }

    return this.http.post<Album>(this.apiUrl, formData);
  }

  // PUT: Update album
  updateAlbum(albumId: number, request: UpdateAlbumRequest): Observable<Album> {
    const formData = new FormData();
    formData.append('albumTitle', request.albumTitle);
    
    if (request.releaseDate) {
      formData.append('releaseDate', request.releaseDate);
    }
    if (request.albumType) {
      formData.append('albumType', request.albumType);
    }
    if (request.coverImageFile) {
      formData.append('coverImageFile', request.coverImageFile);
    }

    return this.http.put<Album>(`${this.apiUrl}/${albumId}`, formData);
  }

  // DELETE: Delete album
  deleteAlbum(albumId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${albumId}`);
  }

  // Admin Management endpoints
  private managementApiUrl = 'https://localhost:5001/api/AlbumManagement';

  // GET: Get all albums (admin only)
  getAllAlbums(page: number = 1, pageSize: number = 20, search?: string, artistId?: number): Observable<any> {
    let url = `${this.managementApiUrl}/all?page=${page}&pageSize=${pageSize}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    if (artistId) {
      url += `&artistId=${artistId}`;
    }
    return this.http.get<any>(url);
  }

  // DELETE: Delete album (admin only)
  deleteAlbumByAdmin(albumId: number): Observable<any> {
    return this.http.delete<any>(`${this.managementApiUrl}/${albumId}`);
  }
}
