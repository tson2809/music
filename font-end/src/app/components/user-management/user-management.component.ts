import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { SearchHeaderComponent } from '../search-header/search-header.component';
import { PlayerBarComponent, CurrentTrack } from '../player-bar/player-bar.component';
import { User, UserListResponse } from '../../models/user.model';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { ArtistUpgradeService, UpgradeRequest, UpgradeRequestListResponse } from '../../services/artist-upgrade.service';
import { Subject, Subscription } from 'rxjs';
import { distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    SidebarComponent,
    SearchHeaderComponent,
    PlayerBarComponent
  ],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit, OnDestroy {
  users: User[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';

  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;
  searchQuery = '';
  selectedRoleId: number | null = null;
  selectedRoleFilter: string | null = null; // 'admin', 'user', 'artist', null for all
  selectedStatusFilter: string | null = null; // 'active', 'inactive', null for all

  currentUser: User | null = null;
  progress = 0;
  volume = 70;
  playlists = [
    'Yêu thích',
    'Nhạc Việt',
    'Pop',
    'Rock',
    'Chill',
    'Workout'
  ];
  currentTrack: CurrentTrack = {
    name: 'Chọn bài hát để phát',
    artist: 'Nghệ sĩ'
  };

  private statusLoading = new Set<number>();

  // Upgrade requests modal
  showUpgradeModal = false;
  upgradeRequests: UpgradeRequest[] = [];
  loadingUpgradeRequests = false;
  upgradeRequestPage = 1;
  upgradeRequestPageSize = 10;
  upgradeRequestTotalPages = 0;
  upgradeRequestTotalCount = 0;
  selectedRequest: UpgradeRequest | null = null;
  showRejectModal = false;
  rejectionReason = '';
  processingRequest = false;

  private searchSubject = new Subject<string>();
  private subscriptions = new Subscription();

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router,
    private upgradeService: ArtistUpgradeService
  ) {}

  ngOnInit(): void {
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
    });

    this.loadUsers();
    this.loadUpgradeRequestsCount();

    // Setup auto search - immediate search without debounce
    const searchSub = this.searchSubject.pipe(
      distinctUntilChanged()
    ).subscribe(query => {
      this.currentPage = 1;
      this.searchQuery = query;
      this.loadUsers();
    });
    this.subscriptions.add(searchSub);
  }

  onRoleFilterChange(): void {
    // Convert filter string to roleId or handle artist filter
    if (this.selectedRoleFilter === 'admin') {
      this.selectedRoleId = 1;
    } else if (this.selectedRoleFilter === 'user') {
      this.selectedRoleId = 2;
    } else if (this.selectedRoleFilter === 'artist') {
      this.selectedRoleId = null; // Will filter by artistId in frontend
    } else {
      this.selectedRoleId = null;
    }
    this.currentPage = 1;
    this.loadUsers();
  }

  onStatusFilterChange(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  resetAllFilters(): void {
    this.searchQuery = '';
    this.selectedRoleFilter = null;
    this.selectedRoleId = null;
    this.selectedStatusFilter = null;
    this.currentPage = 1;
    this.searchSubject.next('');
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadUsers(): void {
    this.loading = true;
    this.errorMessage = '';

    // Determine roleId and isArtist parameters based on filter
    let roleIdForBackend: number | undefined = undefined;
    let isArtistForBackend: boolean | undefined = undefined;
    let isActiveForBackend: boolean | undefined = undefined;

    if (this.selectedRoleFilter === 'admin') {
      // Filter Admin: roleId = 1
      roleIdForBackend = 1;
    } else if (this.selectedRoleFilter === 'user') {
      // Filter Người dùng: roleId = 2 (User role) and not artist
      roleIdForBackend = 2;
      isArtistForBackend = false;
    } else if (this.selectedRoleFilter === 'artist') {
      // Filter Nghệ sĩ: has artistId
      isArtistForBackend = true;
    }

    // Determine isActive parameter based on status filter
    if (this.selectedStatusFilter === 'active') {
      isActiveForBackend = true;
    } else if (this.selectedStatusFilter === 'inactive') {
      isActiveForBackend = false;
    }

    this.userService.getUsers(this.currentPage, this.pageSize, this.searchQuery, roleIdForBackend, isArtistForBackend, isActiveForBackend).subscribe({
      next: (response: UserListResponse) => {
        this.users = response.users;
        this.totalPages = response.totalPages;
        this.totalCount = response.totalCount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading users:', error);
        this.handleError('Không thể tải danh sách người dùng', error);
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  onSearchInput(): void {
    this.searchSubject.next(this.searchQuery);
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.currentPage = 1;
    this.searchSubject.next('');
    this.loadUsers();
  }

  toggleUserStatus(user: User): void {
    if (this.statusLoading.has(user.userId)) {
      return;
    }

    // Prevent admin from deactivating themselves
    if (user.roleId === 1 && this.currentUser && user.userId === this.currentUser.userId && user.isActive) {
      this.handleError('Bạn không thể vô hiệu hóa chính mình');
      return;
    }

    const newStatus = !user.isActive;
    this.statusLoading.add(user.userId);

    this.userService.updateUserStatus(user.userId, newStatus).subscribe({
      next: () => {
        user.isActive = newStatus;
        this.showSuccessMessage(`Đã ${newStatus ? 'kích hoạt' : 'vô hiệu hóa'} tài khoản`);
        this.statusLoading.delete(user.userId);
      },
      error: (error) => {
        console.error('Error updating user status:', error);
        this.handleError('Không thể cập nhật trạng thái tài khoản', error);
        this.statusLoading.delete(user.userId);
      }
    });
  }

  canToggleUserStatus(user: User): boolean {
    // Admin cannot deactivate themselves
    if (user.roleId === 1 && this.currentUser && user.userId === this.currentUser.userId && user.isActive) {
      return false;
    }
    return true;
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadUsers();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadUsers();
    }
  }

  goToPage(page: number): void {
    this.currentPage = page;
    this.loadUsers();
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;

    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  }

  getUserRoleLabel(user: User): string {
    if (user.roleId === 1) {
      return 'Admin';
    }
    if (user.artistId) {
      return 'Nghệ sĩ';
    }
    return 'Người dùng';
  }

  getUserRoleBadgeClass(user: User): string {
    if (user.roleId === 1) {
      return 'badge badge-admin';
    }
    if (user.artistId) {
      return 'badge badge-artist';
    }
    return 'badge badge-user';
  }

  getStatusBadgeClass(isActive: boolean): string {
    return isActive ? 'status-badge active' : 'status-badge inactive';
  }

  isStatusUpdating(userId: number): boolean {
    return this.statusLoading.has(userId);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private showSuccessMessage(message: string): void {
    this.successMessage = message;
    setTimeout(() => {
      this.successMessage = '';
    }, 3000);
  }

  private handleError(message: string, error?: unknown): void {
    this.errorMessage = (error as any)?.error?.message || message;
    setTimeout(() => {
      this.errorMessage = '';
    }, 4000);
  }

  // Upgrade request methods
  loadUpgradeRequestsCount(): void {
    this.upgradeService.getPendingRequests(1, 1).subscribe({
      next: (response: UpgradeRequestListResponse) => {
        this.upgradeRequestTotalCount = response.totalCount;
      },
      error: (error) => {
        // Silently fail - just don't show count
        console.error('Error loading upgrade requests count:', error);
        this.upgradeRequestTotalCount = 0;
      }
    });
  }

  openUpgradeModal(): void {
    this.showUpgradeModal = true;
    this.loadUpgradeRequests();
  }

  closeUpgradeModal(): void {
    this.showUpgradeModal = false;
    this.selectedRequest = null;
    this.showRejectModal = false;
    this.rejectionReason = '';
  }

  loadUpgradeRequests(): void {
    this.loadingUpgradeRequests = true;
    this.upgradeService.getPendingRequests(this.upgradeRequestPage, this.upgradeRequestPageSize).subscribe({
      next: (response: UpgradeRequestListResponse) => {
        this.upgradeRequests = response.requests;
        this.upgradeRequestTotalPages = response.totalPages;
        this.upgradeRequestTotalCount = response.totalCount;
        this.loadingUpgradeRequests = false;
      },
      error: (error) => {
        console.error('Error loading upgrade requests:', error);
        this.handleError('Không thể tải danh sách yêu cầu nâng cấp', error);
        this.loadingUpgradeRequests = false;
      }
    });
  }

  approveRequest(request: UpgradeRequest): void {
    if (this.processingRequest) return;

    this.processingRequest = true;
    this.upgradeService.approveRequest(request.requestId).subscribe({
      next: (response) => {
        this.showSuccessMessage('Đã duyệt yêu cầu nâng cấp thành công');
        this.loadUpgradeRequests();
        this.loadUsers(); // Refresh user list
        this.loadUpgradeRequestsCount(); // Refresh count
        this.processingRequest = false;
      },
      error: (error) => {
        console.error('Error approving request:', error);
        this.handleError('Không thể duyệt yêu cầu', error);
        this.processingRequest = false;
      }
    });
  }

  openRejectModal(request: UpgradeRequest): void {
    this.selectedRequest = request;
    this.rejectionReason = '';
    this.showRejectModal = true;
  }

  closeRejectModal(): void {
    this.showRejectModal = false;
    this.selectedRequest = null;
    this.rejectionReason = '';
  }

  rejectRequest(): void {
    if (!this.selectedRequest || this.processingRequest) return;

    this.processingRequest = true;
    this.upgradeService.rejectRequest(this.selectedRequest.requestId, this.rejectionReason?.trim() || undefined).subscribe({
      next: () => {
        this.showSuccessMessage('Đã từ chối yêu cầu nâng cấp');
        this.closeRejectModal();
        this.loadUpgradeRequests();
        this.loadUpgradeRequestsCount(); // Refresh count
        this.processingRequest = false;
      },
      error: (error) => {
        console.error('Error rejecting request:', error);
        this.handleError('Không thể từ chối yêu cầu', error);
        this.processingRequest = false;
      }
    });
  }

  getStatusText(status: string): string {
    switch (status) {
      case 'Pending':
        return 'Đang chờ duyệt';
      case 'Approved':
        return 'Đã được duyệt';
      case 'Rejected':
        return 'Đã bị từ chối';
      default:
        return status;
    }
  }

  previousUpgradePage(): void {
    if (this.upgradeRequestPage > 1) {
      this.upgradeRequestPage--;
      this.loadUpgradeRequests();
    }
  }

  nextUpgradePage(): void {
    if (this.upgradeRequestPage < this.upgradeRequestTotalPages) {
      this.upgradeRequestPage++;
      this.loadUpgradeRequests();
    }
  }

  getProfileImageUrl(user: User): string {
    if (!user || !user.profilePictureUrl) {
      return '';
    }
    
    let imageUrl = user.profilePictureUrl;
    
    // If URL is already a full URL (starts with http:// or https://), use it directly
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl;
    }
    
    // If URL is a relative path (starts with /images/ or images/), add base URL
    if (imageUrl.startsWith('/images/') || imageUrl.startsWith('images/')) {
      const baseUrl = 'https://localhost:5001';
      return imageUrl.startsWith('/') ? `${baseUrl}${imageUrl}` : `${baseUrl}/${imageUrl}`;
    }
    
    return imageUrl;
  }

  getRequestProfileImageUrl(request: UpgradeRequest): string {
    if (!request || !request.userProfilePictureUrl) {
      return '';
    }
    
    let imageUrl = request.userProfilePictureUrl;
    
    // If URL is already a full URL (starts with http:// or https://), use it directly
    if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
      return imageUrl;
    }
    
    // If URL is a relative path (starts with /images/ or images/), add base URL
    if (imageUrl.startsWith('/images/') || imageUrl.startsWith('images/')) {
      const baseUrl = 'https://localhost:5001';
      return imageUrl.startsWith('/') ? `${baseUrl}${imageUrl}` : `${baseUrl}/${imageUrl}`;
    }
    
    return imageUrl;
  }
}

