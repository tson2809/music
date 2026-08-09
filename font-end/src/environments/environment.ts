// Cấu hình cho môi trường development
// Dùng relative path - Angular proxy sẽ tự động forward đến backend
export const environment = {
  production: false,
  apiUrl: '/api' // Không cần port - dùng proxy!
};

