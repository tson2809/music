using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MusicStream.Data;
using MusicStream.Models;
using Microsoft.AspNetCore.Hosting;

namespace MusicStream.Controllers
{
    [Route("api/artists")]
    [ApiController]
    [Authorize]
    public class ArtistsController : ControllerBase
    {
        private readonly MusicStreamContext _context;
        private readonly ILogger<ArtistsController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ArtistsController(
            MusicStreamContext context,
            ILogger<ArtistsController> logger,
            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        // PUT: api/artists/{id} - Cập nhật thông tin nghệ sĩ
        [HttpPut("{id}")]
        public async Task<ActionResult<ArtistDetailDto>> UpdateArtist(int id, [FromForm] UpdateArtistRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var artist = await _context.Artists
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.ArtistId == id);

                if (artist == null)
                {
                    return NotFound(new { message = "Không tìm thấy nghệ sĩ" });
                }

                // Kiểm tra quyền: chỉ nghệ sĩ đó mới được sửa
                var currentUser = await _context.Users
                    .Include(u => u.Artist)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (currentUser == null)
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var isAdmin = currentUser.RoleId == 1;
                var isOwner = currentUser.Artist != null && currentUser.Artist.ArtistId == id;

                if (!isAdmin && !isOwner)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền chỉnh sửa nghệ sĩ này" });
                }

                // Cập nhật tên nghệ sĩ
                if (!string.IsNullOrWhiteSpace(request.ArtistName))
                {
                    artist.ArtistName = request.ArtistName.Trim();
                }

                // Cập nhật biography
                if (request.Biography != null)
                {
                    artist.Biography = string.IsNullOrWhiteSpace(request.Biography) ? null : request.Biography.Trim();
                }

                // Xử lý upload ảnh nếu có
                if (request.ProfileImageFile != null && request.ProfileImageFile.Length > 0)
                {
                    try
                    {
                        // Xóa ảnh cũ nếu có (trước khi lưu ảnh mới)
                        if (artist.User != null && !string.IsNullOrEmpty(artist.User.ProfilePictureUrl) && 
                            (artist.User.ProfilePictureUrl.StartsWith("/images/avatar") || artist.User.ProfilePictureUrl.StartsWith("images/avatar")))
                        {
                            var oldPath = artist.User.ProfilePictureUrl.TrimStart('/');
                            var fullOldPath = Path.Combine(_hostingEnvironment.ContentRootPath, oldPath.Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(fullOldPath))
                            {
                                System.IO.File.Delete(fullOldPath);
                                _logger.LogInformation("Deleted old profile image: {Path}", fullOldPath);
                            }
                        }

                        var imageUrl = await SaveProfileImageAsync(request.ProfileImageFile, currentUserId);
                        if (artist.User != null)
                        {
                            artist.User.ProfilePictureUrl = imageUrl;
                        }
                        _logger.LogInformation("Profile image saved: {Url}", imageUrl);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return BadRequest(new { message = ex.Message });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving profile image");
                        return StatusCode(500, new { message = "Không thể lưu ảnh: " + ex.Message });
                    }
                }

                await _context.SaveChangesAsync();

                // Reload để lấy dữ liệu mới nhất
                await _context.Entry(artist).ReloadAsync();
                await _context.Entry(artist).Reference(a => a.User).LoadAsync();

                var result = new ArtistDetailDto
                {
                    ArtistId = artist.ArtistId,
                    ArtistName = artist.ArtistName,
                    Biography = artist.Biography,
                    Country = artist.User != null ? artist.User.Country : null,
                    ProfileImageUrl = artist.User != null ? artist.User.ProfilePictureUrl : null,
                    Verified = artist.Verified,
                    MonthlyListeners = artist.MonthlyListeners
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating artist");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        private async Task<string> SaveProfileImageAsync(IFormFile imageFile, int userId)
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

            var uploadsFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "images", "avatar");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"artist_{userId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{imageExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Trả về relative path (có dấu / ở đầu để nhất quán với AuthController)
            return $"/images/avatar/{uniqueFileName}";
        }
    }

    public class UpdateArtistRequest
    {
        public string? ArtistName { get; set; }
        public string? Biography { get; set; }
        public IFormFile? ProfileImageFile { get; set; }
    }
}

