# MusicStream - Nền Tảng Phát Nhạc & Streaming Âm Nhạc Trực Tuyến

MusicStream là một ứng dụng phát nhạc và streaming âm nhạc trực tuyến full-stack hiện đại được xây dựng theo kiến trúc tách biệt giữa **Backend Web API (ASP.NET Core / .NET)** và **Frontend Web App (Angular)**. Hệ thống cho phép người dùng nghe nhạc trực tuyến, quản lý danh sách phát cá nhân, yêu thích bài hát, nâng cấp tài khoản Nghệ sĩ (Artist), tải lên album/bài hát kết nối lưu trữ đám mây Cloudflare R2 và theo dõi thống kê lượt nghe.

---

## 1. Tổng Quan Kiến Trúc Hệ Thống

Dự án được phân chia thành 2 mô-đun chính:

- **Backend (`back-end/`)**:
  - Xây dựng trên nền tảng **ASP.NET Core Web API / .NET 8**.
  - Sử dụng **Entity Framework Core** để quản lý cơ sở dữ liệu.
  - Tích hợp dịch vụ lưu trữ đám mây **Cloudflare R2** (`R2Service`) phục vụ lưu trữ file nhạc audio mp3 và hình ảnh bìa album/avatar.
  - Cơ chế xác thực **JWT Bearer Token** bảo mật API endpoints.

- **Frontend (`font-end/`)**:
  - Xây dựng trên nền tảng **Angular 18+** Single Page Application (SPA).
  - Giao diện phát nhạc hiện đại hỗ trợ trình phát audio chuyên nghiệp (Play, Pause, Seek bar, Volume control, Repeat, Shuffle).
  - Tích hợp Angular Services, RxJS State Management và Router Guards.

---

## 2. Các Phân Hệ & Chức Năng Chính

### 2.1 Xác Thực & Quản Lý Tài Khoản (Authentication & Profile)
- **Đăng ký & Đăng nhập**: Xác thực tài khoản người dùng, mã hóa mật khẩu và phát hành JWT Token.
- **Quản lý Hồ sơ**: Thay đổi thông tin cá nhân, cập nhật ảnh đại diện (Avatar).
- **Phân quyền người dùng**: Hệ thống 3 cấp độ phân quyền (Admin, Artist, Listener).

### 2.2 Trình Phát Nhạc & Tìm Kiếm (Music Player & Search)
- **Trình phát nhạc trực tuyến**: Phát nhạc audio chất lượng cao với các điều khiển chuyên nghiệp.
- **Tìm kiếm thông minh**: Tìm kiếm bài hát theo tên, ca sĩ, album hoặc thể loại nhạc.
- **Phần loại Thể loại (Genre)**: Khám phá âm nhạc theo dòng nhạc (Pop, Ballad, Rap, Rock, EDM...).

### 2.3 Quản Lý Danh Sách Phát & Yêu Thích (Playlists & Favorites)
- **Danh sách yêu thích (User Favorites)**: Thả tim và lưu danh sách bài hát yêu thích cá nhân.
- **Tạo Playlist**: Tạo, chỉnh sửa và quản lý các danh sách phát nhạc tùy chỉnh.
- **Lịch sử nghe (Listening History)**: Tự động ghi nhận lịch sử các bài hát đã phát.

### 2.4 Dành Cho Nghệ Sĩ (Artist Portal & Upgrades)
- **Đăng ký Nghệ sĩ (Artist Upgrade Request)**: Cho phép người dùng gửi yêu cầu nâng cấp tài khoản lên Nghệ sĩ.
- **Quản lý Album (Album Management)**: Tạo album mới, tải ảnh bìa và thêm danh sách bài hát vào album.
- **Quản lý Bài hát (Song Management)**: Upload bài hát mới, cập nhật lời bài hát (Lyrics) và file audio lên Cloudflare R2 Storage.

### 2.5 Quản Trị & Thống Kê (Admin & Analytics)
- **Duyệt Nghệ sĩ**: Admin phê duyệt/từ chối các yêu cầu nâng cấp tài khoản Nghệ sĩ.
- **Quản lý Người dùng & Nội dung**: Quản lý danh sách tài khoản, kiểm duyệt bài hát và album.
- **Thống kê (Statistics)**: Thống kê tổng số lượt nghe, top bài hát nổi bật, top nghệ sĩ được yêu thích nhất.

---

## 3. Cấu Trúc Thư Mục Dự Án

```
music/
├── back-end/                     # Mô-đun Backend (ASP.NET Core Web API)
│   ├── MusicStream/
│   │   ├── Controllers/          # API Controllers (Auth, Songs, Albums, Artists...)
│   │   ├── Models/               # Entities & Data Models (User, Song, Album, Playlist...)
│   │   ├── Services/             # Cloudflare R2 Storage & Business Logic
│   │   ├── Migrations/           # EF Core Database Migrations
│   │   └── Program.cs            # File khởi chạy ứng dụng API
│   └── MusicStream.sln
│
└── font-end/                     # Mô-đun Frontend (Angular SPA)
    ├── src/
    │   ├── app/                  # Components, Services, Guards, Pipes
    │   ├── assets/               # Hình ảnh & Static Assets
    │   └── environments/         # Cấu hình môi trường API URL
    ├── angular.json
    └── package.json
```

---

## 4. Hướng Dẫn Cài Đặt & Chạy Dự Án

### 4.1 Yêu Cầu Môi Trường
- **Backend**: .NET 8.0 SDK trở lên.
- **Frontend**: Node.js v18+ và Angular CLI (`npm install -g @angular/cli`).
- **Database**: SQL Server hoặc SQLite.

### 4.2 Khởi Chạy Backend (ASP.NET Core Web API)
1. Di chuyển vào thư mục backend:
   ```bash
   cd back-end/MusicStream
   ```
2. Cập nhật chuỗi kết nối Database trong `appsettings.json` (hoặc biến môi trường).
3. Khởi chạy ứng dụng Web API:
   ```bash
   dotnet run
   ```
4. API sẽ lắng nghe tại mặc định `http://localhost:5000` (hoặc cổng cấu hình).

### 4.3 Khởi Chạy Frontend (Angular App)
1. Di chuyển vào thư mục frontend:
   ```bash
   cd font-end
   ```
2. Cài đặt các gói phụ thuộc (Dependencies):
   ```bash
   npm install
   ```
3. Khởi chạy server phát triển Angular:
   ```bash
   ng serve --open
   ```
4. Ứng dụng web sẽ tự động mở tại `http://localhost:4200`.
