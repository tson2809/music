using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using MusicStream.Data;
using MusicStream.Models;
using MusicStream.Services;
using Microsoft.AspNetCore.Hosting;

namespace MusicStream.Controllers
{
    [Route("api/albums")]
    [ApiController]
    [Authorize]
    public class AlbumsController : ControllerBase
    {
        private readonly MusicStreamContext _context;
        private readonly ILogger<AlbumsController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AlbumsController(
            MusicStreamContext context,
            ILogger<AlbumsController> logger,
            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<AlbumListResponse>> GetAlbums(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? artistId = null,
            [FromQuery] string? search = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            try
            {
                var query = _context.Albums
                    .Include(a => a.Artist)
                    .AsQueryable();

                if (artistId.HasValue && artistId.Value > 0)
                {
                    query = query.Where(a => a.ArtistId == artistId.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(a =>
                        a.AlbumTitle.Contains(search) ||
                        a.Artist.ArtistName.Contains(search));
                }

                var totalCount = await query.CountAsync();

                var albums = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new AlbumResponseDto
                    {
                        AlbumId = a.AlbumId,
                        AlbumTitle = a.AlbumTitle,
                        ArtistId = a.ArtistId,
                        ArtistName = a.Artist.ArtistName,
                        ReleaseDate = a.ReleaseDate,
                        AlbumType = a.AlbumType,
                        CoverImageUrl = a.CoverImageUrl,
                        TotalTracks = a.TotalTracks,
                        DurationSeconds = a.DurationSeconds,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new AlbumListResponse
                {
                    Albums = albums,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching albums");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AlbumDetailDto>> GetAlbum(int id)
        {
            try
            {
                var album = await _context.Albums
                    .Include(a => a.Artist)
                    .Include(a => a.Songs)
                    .ThenInclude(s => s.Genre)
                    .FirstOrDefaultAsync(a => a.AlbumId == id);

                if (album == null)
                {
                    return NotFound(new { message = "Không tìm thấy album" });
                }

                var dto = new AlbumDetailDto
                {
                    AlbumId = album.AlbumId,
                    AlbumTitle = album.AlbumTitle,
                    ArtistId = album.ArtistId,
                    ArtistName = album.Artist.ArtistName,
                    ReleaseDate = album.ReleaseDate,
                    AlbumType = album.AlbumType,
                    CoverImageUrl = album.CoverImageUrl,
                    TotalTracks = album.TotalTracks,
                    DurationSeconds = album.DurationSeconds,
                    CreatedAt = album.CreatedAt,
                    Songs = album.Songs
                        .OrderByDescending(s => s.CreatedAt)
                        .Select(s => new AlbumSongDto
                        {
                            SongId = s.SongId,
                            SongTitle = s.SongTitle,
                            DurationSeconds = s.DurationSeconds,
                            GenreName = s.Genre != null ? s.Genre.GenreName : null,
                            AudioFileUrl = s.AudioFileUrl,
                            CoverImageUrl = s.CoverImageUrl,
                            ApprovalStatus = s.ApprovalStatus.ToString()
                        })
                        .ToList()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching album detail");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<AlbumResponseDto>> CreateAlbum([FromForm] CreateAlbumRequest request)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var isAdmin = currentUser.RoleId == 1;
                var artist = currentUser.Artist;

                if (!isAdmin && artist == null)
                {
                    return StatusCode(403, new { message = "Chỉ nghệ sĩ mới có thể tạo album" });
                }

                if (string.IsNullOrWhiteSpace(request.AlbumTitle))
                {
                    return BadRequest(new { message = "Vui lòng nhập tên album" });
                }

                int targetArtistId = isAdmin && request.ArtistId > 0
                    ? request.ArtistId
                    : artist!.ArtistId;

                var targetArtist = await _context.Artists.FindAsync(targetArtistId);
                if (targetArtist == null)
                {
                    return NotFound(new { message = "Không tìm thấy nghệ sĩ" });
                }

                string? coverUrl = null;
                if (request.CoverImageFile != null && request.CoverImageFile.Length > 0)
                {
                    coverUrl = await SaveCoverImageAsync(request.CoverImageFile);
                }

                var album = new Album
                {
                    AlbumTitle = request.AlbumTitle.Trim(),
                    ArtistId = targetArtistId,
                    ReleaseDate = request.ReleaseDate,
                    AlbumType = string.IsNullOrWhiteSpace(request.AlbumType) ? "album" : request.AlbumType!.Trim(),
                    CoverImageUrl = coverUrl,
                    TotalTracks = 0,
                    DurationSeconds = 0,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Albums.Add(album);
                await _context.SaveChangesAsync();

                await _context.Entry(album).Reference(a => a.Artist).LoadAsync();

                return CreatedAtAction(nameof(GetAlbum), new { id = album.AlbumId }, new AlbumResponseDto
                {
                    AlbumId = album.AlbumId,
                    AlbumTitle = album.AlbumTitle,
                    ArtistId = album.ArtistId,
                    ArtistName = album.Artist.ArtistName,
                    ReleaseDate = album.ReleaseDate,
                    AlbumType = album.AlbumType,
                    CoverImageUrl = album.CoverImageUrl,
                    TotalTracks = album.TotalTracks,
                    DurationSeconds = album.DurationSeconds,
                    CreatedAt = album.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating album");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AlbumResponseDto>> UpdateAlbum(int id, [FromForm] UpdateAlbumRequest request)
        {
            try
            {
                var album = await _context.Albums
                    .Include(a => a.Artist)
                    .FirstOrDefaultAsync(a => a.AlbumId == id);

                if (album == null)
                {
                    return NotFound(new { message = "Không tìm thấy album" });
                }

                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var isAdmin = currentUser.RoleId == 1;
                var artistId = currentUser.Artist?.ArtistId;

                if (!isAdmin && artistId != album.ArtistId)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền chỉnh sửa album này" });
                }

                if (!string.IsNullOrWhiteSpace(request.AlbumTitle))
                {
                    album.AlbumTitle = request.AlbumTitle.Trim();
                }

                album.ReleaseDate = request.ReleaseDate ?? album.ReleaseDate;

                if (!string.IsNullOrWhiteSpace(request.AlbumType))
                {
                    album.AlbumType = request.AlbumType.Trim();
                }

                if (request.CoverImageFile != null && request.CoverImageFile.Length > 0)
                {
                    album.CoverImageUrl = await SaveCoverImageAsync(request.CoverImageFile);
                }

                await _context.SaveChangesAsync();

                return Ok(new AlbumResponseDto
                {
                    AlbumId = album.AlbumId,
                    AlbumTitle = album.AlbumTitle,
                    ArtistId = album.ArtistId,
                    ArtistName = album.Artist.ArtistName,
                    ReleaseDate = album.ReleaseDate,
                    AlbumType = album.AlbumType,
                    CoverImageUrl = album.CoverImageUrl,
                    TotalTracks = album.TotalTracks,
                    DurationSeconds = album.DurationSeconds,
                    CreatedAt = album.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating album");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAlbum(int id)
        {
            try
            {
                var album = await _context.Albums
                    .Include(a => a.Songs)
                    .FirstOrDefaultAsync(a => a.AlbumId == id);

                if (album == null)
                {
                    return NotFound(new { message = "Không tìm thấy album" });
                }

                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var isAdmin = currentUser.RoleId == 1;
                var artistId = currentUser.Artist?.ArtistId;

                if (!isAdmin && artistId != album.ArtistId)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền xóa album này" });
                }

                // Nếu album có bài hát, set albumId = null cho các bài hát trước khi xóa album
                if (album.Songs.Any())
                {
                    foreach (var song in album.Songs)
                    {
                        song.AlbumId = null;
                    }
                    await _context.SaveChangesAsync();
                }

                // Xóa album
                _context.Albums.Remove(album);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Album deleted: {album.AlbumId} - {album.AlbumTitle}");

                return Ok(new { message = "Đã xóa album thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting album");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return null;
            }

            return await _context.Users
                .Include(u => u.Artist)
                .FirstOrDefaultAsync(u => u.UserId == currentUserId);
        }

        // Lưu ảnh bìa album vào thư mục local: MusicStream/images/albums
        private async Task<string> SaveCoverImageAsync(IFormFile imageFile)
        {
            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var imageExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!allowedImageExtensions.Contains(imageExtension))
            {
                throw new InvalidOperationException("Định dạng ảnh không được hỗ trợ");
            }

            const long maxImageSize = 10 * 1024 * 1024; // 10MB
            if (imageFile.Length > maxImageSize)
            {
                throw new InvalidOperationException("Ảnh quá lớn. Kích thước tối đa là 10MB");
            }

            var uploadsFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "images", "albums");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + imageExtension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Trả về relative path để frontend dùng `https://localhost:5001/` + path
            return $"images/albums/{uniqueFileName}";
        }
    }

    public class AlbumResponseDto
    {
        public int AlbumId { get; set; }
        public string AlbumTitle { get; set; } = null!;
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = null!;
        public DateTime? ReleaseDate { get; set; }
        public string AlbumType { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public int TotalTracks { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AlbumDetailDto : AlbumResponseDto
    {
        public List<AlbumSongDto> Songs { get; set; } = new();
    }

    public class AlbumSongDto
    {
        public int SongId { get; set; }
        public string SongTitle { get; set; } = null!;
        public int DurationSeconds { get; set; }
        public string? GenreName { get; set; }
        public string AudioFileUrl { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public string ApprovalStatus { get; set; } = null!;
    }

    public class AlbumListResponse
    {
        public List<AlbumResponseDto> Albums { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class CreateAlbumRequest
    {
        public string AlbumTitle { get; set; } = null!;
        public int ArtistId { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? AlbumType { get; set; }
        public IFormFile? CoverImageFile { get; set; }
    }

    public class UpdateAlbumRequest
    {
        public string AlbumTitle { get; set; } = null!;
        public DateTime? ReleaseDate { get; set; }
        public string? AlbumType { get; set; }
        public IFormFile? CoverImageFile { get; set; }
    }
}

