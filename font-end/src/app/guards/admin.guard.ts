import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const AdminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Kiểm tra xem user có đăng nhập không
  if (!authService.isAuthenticated()) {
    router.navigate(['/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }

  // Kiểm tra xem user có phải admin không
  if (authService.isAdmin()) {
    return true;
  }

  // Nếu không phải admin, chuyển về trang home
  router.navigate(['/home']);
  return false;
};

