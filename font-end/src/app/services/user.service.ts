import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserListResponse } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly baseUrl = 'https://localhost:5001/api/users';

  constructor(private http: HttpClient) {}

  getUsers(page = 1, pageSize = 20, search?: string, roleId?: number, isArtist?: boolean, isActive?: boolean): Observable<UserListResponse> {
    let url = `${this.baseUrl}?page=${page}&pageSize=${pageSize}`;
    if (search) {
      url += `&search=${encodeURIComponent(search)}`;
    }
    if (roleId && roleId > 0) {
      url += `&roleId=${roleId}`;
    }
    if (isArtist !== undefined) {
      url += `&isArtist=${isArtist}`;
    }
    if (isActive !== undefined) {
      url += `&isActive=${isActive}`;
    }
    return this.http.get<UserListResponse>(url);
  }

  updateUserStatus(userId: number, isActive: boolean): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${userId}/status`, { isActive });
  }

  updateUserRole(userId: number, roleId: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${userId}/role`, { roleId });
  }
}

