import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const ArtistGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Kiểm tra xem user có đăng nhập không
  if (!authService.isAuthenticated()) {
    router.navigate(['/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }

  // Kiểm tra xem user có phải artist không
  if (authService.isArtist()) {
    return true;
  }

  // Nếu không phải artist, chuyển về trang home
  router.navigate(['/home']);
  return false;
};

