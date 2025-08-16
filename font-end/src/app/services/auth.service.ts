import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, User, AuthState } from '../models/user.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl; // Backend URL từ environment
  private authStateSubject = new BehaviorSubject<AuthState>({
    isAuthenticated: false,
    user: null,
    token: null
  });

  public authState$ = this.authStateSubject.asObservable();

  constructor(private http: HttpClient) {
    // Kiểm tra localStorage khi khởi tạo
    this.loadAuthState();
  }

  private loadAuthState(): void {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');
    
    if (token && userStr) {
      try {
        const user = JSON.parse(userStr);
        this.authStateSubject.next({
          isAuthenticated: true,
          user,
          token
        });
      } catch (error) {
        this.clearAuthState();
      }
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        tap(response => {
          if (response.token && response.user) {
            localStorage.setItem('token', response.token);
            localStorage.setItem('user', JSON.stringify(response.user));
            
            this.authStateSubject.next({
              isAuthenticated: true,
              user: response.user,
              token: response.token
            });
          }
        })
      );
  }

  logout(): void {
    this.clearAuthState();
  }

  private clearAuthState(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.authStateSubject.next({
      isAuthenticated: false,
      user: null,
      token: null
    });
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getCurrentUser(): User | null {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch {
        return null;
      }
    }
    return null;
  }

  isAuthenticated(): boolean {
    return this.authStateSubject.value.isAuthenticated;
  }

  isAdmin(): boolean {
    const user = this.getCurrentUser();
    return user !== null && user.roleId === 1; // RoleId 1 là Admin
  }

  isArtist(): boolean {
    const user = this.getCurrentUser();
    return user !== null && user.artistId !== undefined && user.artistId !== null;
  }

  updateUser(user: User): void {
    const currentState = this.authStateSubject.value;
    if (currentState.isAuthenticated) {
      localStorage.setItem('user', JSON.stringify(user));
      this.authStateSubject.next({
        ...currentState,
        user
      });
    }
  }
}

