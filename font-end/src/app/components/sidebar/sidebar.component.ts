import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';

const SIDEBAR_STORAGE_KEY = 'sidebarCollapsed';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  isAdmin = false;
  isArtist = false;
  @Input() playlists: any[] = [];
  isCollapsed = false;

  constructor(
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.authState$.subscribe(state => {
      this.currentUser = state.user;
      this.isAdmin = state.user?.roleId === 1;
      this.isArtist = state.user?.artistId !== undefined && state.user?.artistId !== null;
    });

    try {
      const storedValue = localStorage.getItem(SIDEBAR_STORAGE_KEY);
      this.isCollapsed = storedValue === 'true';
    } catch {
      this.isCollapsed = false;
    }

    this.updateBodyClass();
  }

  ngOnDestroy(): void {
    document.body.classList.remove('sidebar-collapsed');
  }

  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
    try {
      localStorage.setItem(SIDEBAR_STORAGE_KEY, this.isCollapsed ? 'true' : 'false');
    } catch {
      // ignore storage failures
    }
    this.updateBodyClass();
  }

  private updateBodyClass(): void {
    if (this.isCollapsed) {
      document.body.classList.add('sidebar-collapsed');
    } else {
      document.body.classList.remove('sidebar-collapsed');
    }
  }
}

