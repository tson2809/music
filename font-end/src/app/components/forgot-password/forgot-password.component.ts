import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {
  forgotPasswordForm: FormGroup;
  step: 'username' | 'password' = 'username';
  loading = false;
  errorMessage = '';
  successMessage = '';
  showNewPassword = false;
  showConfirmPassword = false;
  username = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private http: HttpClient
  ) {
    this.forgotPasswordForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    });
  }

  get usernameControl() {
    return this.forgotPasswordForm.get('username');
  }

  get newPassword() {
    return this.forgotPasswordForm.get('newPassword');
  }

  get confirmPassword() {
    return this.forgotPasswordForm.get('confirmPassword');
  }

  checkUsername() {
    if (this.usernameControl?.invalid) {
      this.usernameControl.markAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.username = this.usernameControl?.value;

    // Gọi API kiểm tra username có tồn tại không
    this.http.post(`${environment.apiUrl}/auth/check-username`, { 
      username: this.username 
    }).subscribe({
      next: (response: any) => {
        this.loading = false;
        if (response.exists) {
          this.step = 'password';
          this.successMessage = 'Tài khoản hợp lệ! Vui lòng nhập mật khẩu mới.';
        } else {
          this.errorMessage = 'Tên đăng nhập không tồn tại!';
        }
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Tên đăng nhập không tồn tại!';
      }
    });
  }

  resetPassword() {
    if (this.newPassword?.invalid || this.confirmPassword?.invalid) {
      this.newPassword?.markAsTouched();
      this.confirmPassword?.markAsTouched();
      return;
    }

    if (this.newPassword?.value !== this.confirmPassword?.value) {
      this.errorMessage = 'Mật khẩu xác nhận không khớp!';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    // Gọi API reset password
    this.http.post(`${environment.apiUrl}/auth/reset-password`, {
      username: this.username,
      newPassword: this.newPassword?.value
    }).subscribe({
      next: (response: any) => {
        this.loading = false;
        this.successMessage = 'Đổi mật khẩu thành công! Đang chuyển đến trang đăng nhập...';
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.error?.message || 'Có lỗi xảy ra. Vui lòng thử lại!';
      }
    });
  }

  toggleNewPasswordVisibility() {
    this.showNewPassword = !this.showNewPassword;
  }

  toggleConfirmPasswordVisibility() {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  backToUsername() {
    this.step = 'username';
    this.errorMessage = '';
    this.successMessage = '';
    this.forgotPasswordForm.patchValue({
      newPassword: '',
      confirmPassword: ''
    });
  }
}

