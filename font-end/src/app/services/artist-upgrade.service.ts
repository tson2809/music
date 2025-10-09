import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface SubmitUpgradeRequest {
  artistName: string;
  biography?: string;
  approvalReason: string;
}

export interface UpgradeRequest {
  requestId: number;
  userId: number;
  userName?: string;
  userEmail?: string;
  userFullName?: string;
  userProfilePictureUrl?: string;
  artistName: string;
  biography?: string;
  approvalReason: string;
  status: string;
  createdAt: Date;
  reviewedAt?: Date;
  reviewedByUserId?: number;
  reviewedByUserName?: string;
  rejectionReason?: string;
}

export interface UpgradeRequestListResponse {
  requests: UpgradeRequest[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ArtistUpgradeService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  submitRequest(request: SubmitUpgradeRequest): Observable<{ message: string; requestId: number }> {
    return this.http.post<{ message: string; requestId: number }>(
      `${this.apiUrl}/artistupgrade/submit`,
      request
    );
  }

  getMyRequest(): Observable<UpgradeRequest> {
    return this.http.get<UpgradeRequest>(`${this.apiUrl}/artistupgrade/my-request`);
  }

  getPendingRequests(page: number = 1, pageSize: number = 20): Observable<UpgradeRequestListResponse> {
    return this.http.get<UpgradeRequestListResponse>(
      `${this.apiUrl}/artistupgrade/pending?page=${page}&pageSize=${pageSize}`
    );
  }

  approveRequest(requestId: number): Observable<{ message: string; artistId: number }> {
    return this.http.post<{ message: string; artistId: number }>(
      `${this.apiUrl}/artistupgrade/${requestId}/approve`,
      {}
    );
  }

  rejectRequest(requestId: number, rejectionReason?: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.apiUrl}/artistupgrade/${requestId}/reject`,
      { rejectionReason }
    );
  }
}

