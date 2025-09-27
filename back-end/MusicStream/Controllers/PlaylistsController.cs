using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MusicStream.Data;
using MusicStream.Models;
using MusicStream.Services;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace MusicStream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlaylistsController : ControllerBase
    {
        private readonly MusicStreamContext _context;
        private readonly ILogger<PlaylistsController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PlaylistsController(
            MusicStreamContext context,
            ILogger<PlaylistsController> logger,
            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/Playlists - Lấy tất cả playlists của user hiện tại
        [HttpGet]
        public async Task<ActionResult<List<PlaylistDto>>> GetPlaylists()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    _logger.LogWarning("Cannot get user ID from token");
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                _logger.LogInformation("Getting playlists for user ID: {UserId}", currentUserId);

                var playlistsData = await _context.Playlists
                    .Where(p => p.UserId == currentUserId)
                    .Include(p => p.PlaylistSongs)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} playlists for user {UserId}", playlistsData.Count, currentUserId);

                var playlists = playlistsData.Select(p => new PlaylistDto
                {
                    PlaylistId = p.PlaylistId,
                    PlaylistName = p.PlaylistName,
                    Description = p.Description,
                    CoverImageUrl = p.CoverImageUrl,
                    IsPublic = p.IsPublic,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    SongCount = p.PlaylistSongs.Count
                }).ToList();

                return Ok(playlists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting playlists");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // GET: api/Playlists/public - Lấy tất cả playlists công khai
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<List<PlaylistDto>>> GetPublicPlaylists()
        {
            try
            {
                _logger.LogInformation("Getting public playlists");

                var playlistsData = await _context.Playlists
                    .Where(p => p.IsPublic == true)
                    .Include(p => p.PlaylistSongs)
                    .Include(p => p.User)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} public playlists", playlistsData.Count);

                var playlists = playlistsData
                    .Where(p => p.PlaylistSongs.Count > 0) // Chỉ lấy playlist có ít nhất 1 bài hát
                    .Select(p => new PlaylistDto
                    {
                        PlaylistId = p.PlaylistId,
                        PlaylistName = p.PlaylistName,
                        Description = p.Description,
                        CoverImageUrl = p.CoverImageUrl,
                        IsPublic = p.IsPublic,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        SongCount = p.PlaylistSongs.Count,
                        OwnerId = p.UserId,
                        OwnerName = p.User != null ? (p.User.FullName ?? p.User.Username) : "Unknown"
                    })
                    .ToList();

                return Ok(playlists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public playlists");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // GET: api/Playlists/{id} - Lấy chi tiết playlist với danh sách bài hát
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PlaylistDetailDto>> GetPlaylist(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                int? currentUserId = null;
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    currentUserId = parsedUserId;
                }

                var playlist = await _context.Playlists
                    .Include(p => p.PlaylistSongs)
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s.Artist)
                    .Include(p => p.PlaylistSongs)
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s.Album)
                    .Include(p => p.PlaylistSongs)
                        .ThenInclude(ps => ps.Song)
                            .ThenInclude(s => s.Genre)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PlaylistId == id);

                if (playlist == null)
                {
                    return NotFound(new { message = "Không tìm thấy danh sách phát" });
                }

                // Chỉ cho phép xem playlist của chính mình hoặc playlist công khai
                if (playlist.UserId != currentUserId && !playlist.IsPublic)
                {
                    return Forbid("Bạn không có quyền xem danh sách phát này");
                }

                var songs = playlist.PlaylistSongs
                    .Where(ps => ps.Song.ApprovalStatus == ApprovalStatus.Approved) // Chỉ hiển thị bài đã duyệt
                    .OrderBy(ps => ps.Position)
                    .Select(ps => new PlaylistSongDto
                    {
                        SongId = ps.Song.SongId,
                        SongTitle = ps.Song.SongTitle,
                        ArtistId = ps.Song.ArtistId,
                        ArtistName = ps.Song.Artist.ArtistName,
                        AlbumId = ps.Song.AlbumId,
                        AlbumTitle = ps.Song.Album != null ? ps.Song.Album.AlbumTitle : null,
                        GenreId = ps.Song.GenreId,
                        GenreName = ps.Song.Genre != null ? ps.Song.Genre.GenreName : null,
                        AudioFileUrl = ps.Song.AudioFileUrl,
                        CoverImageUrl = ps.Song.CoverImageUrl,
                        DurationSeconds = ps.Song.DurationSeconds,
                        PlayCount = ps.Song.PlayCount,
                        LikeCount = ps.Song.LikeCount,
                        Position = ps.Position,
                        AddedAt = ps.AddedAt
                    })
                    .ToList();

                var result = new PlaylistDetailDto
                {
                    PlaylistId = playlist.PlaylistId,
                    PlaylistName = playlist.PlaylistName,
                    Description = playlist.Description,
                    CoverImageUrl = playlist.CoverImageUrl,
                    IsPublic = playlist.IsPublic,
                    CreatedAt = playlist.CreatedAt,
                    UpdatedAt = playlist.UpdatedAt,
                    Songs = songs,
                    OwnerId = playlist.UserId,
                    OwnerName = playlist.User != null ? (playlist.User.FullName ?? playlist.User.Username) : null
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting playlist detail");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // POST: api/Playlists - Tạo playlist mới
        [HttpPost]
        public async Task<ActionResult<PlaylistDto>> CreatePlaylist([FromBody] CreatePlaylistRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                if (string.IsNullOrWhiteSpace(request.PlaylistName))
                {
                    return BadRequest(new { message = "Tên danh sách phát không được để trống" });
                }

                var playlist = new Playlist
                {
                    UserId = currentUserId,
                    PlaylistName = request.PlaylistName.Trim(),
                    Description = request.Description?.Trim(),
                    CoverImageUrl = request.CoverImageUrl?.Trim(),
                    IsPublic = request.IsPublic ?? true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Playlists.Add(playlist);
                await _context.SaveChangesAsync();

                var result = new PlaylistDto
                {
                    PlaylistId = playlist.PlaylistId,
                    PlaylistName = playlist.PlaylistName,
                    Description = playlist.Description,
                    CoverImageUrl = playlist.CoverImageUrl,
                    IsPublic = playlist.IsPublic,
                    CreatedAt = playlist.CreatedAt,
                    UpdatedAt = playlist.UpdatedAt,
                    SongCount = 0
                };

                return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.PlaylistId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating playlist");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // PUT: api/Playlists/{id} - Cập nhật playlist
        [HttpPut("{id}")]
        public async Task<ActionResult<PlaylistDto>> UpdatePlaylist(int id, [FromBody] UpdatePlaylistRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == currentUserId);

                if (playlist == null)
                {
                    return NotFound(new { message = "Không tìm thấy danh sách phát" });
                }

                if (string.IsNullOrWhiteSpace(request.PlaylistName))
                {
                    return BadRequest(new { message = "Tên danh sách phát không được để trống" });
                }

                playlist.PlaylistName = request.PlaylistName.Trim();
                playlist.Description = request.Description?.Trim();
                // Only update CoverImageUrl if it's provided in the request
                // This prevents losing the existing cover image when updating other fields
                if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
                {
                    playlist.CoverImageUrl = request.CoverImageUrl.Trim();
                }
                if (request.IsPublic.HasValue)
                {
                    playlist.IsPublic = request.IsPublic.Value;
                }
                playlist.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var songCount = await _context.PlaylistSongs
                    .CountAsync(ps => ps.PlaylistId == id);

                var result = new PlaylistDto
                {
                    PlaylistId = playlist.PlaylistId,
                    PlaylistName = playlist.PlaylistName,
                    Description = playlist.Description,
                    CoverImageUrl = playlist.CoverImageUrl,
                    IsPublic = playlist.IsPublic,
                    CreatedAt = playlist.CreatedAt,
                    UpdatedAt = playlist.UpdatedAt,
                    SongCount = songCount
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating playlist");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // POST: api/Playlists/{id}/upload-cover - Upload cover image for playlist
        [HttpPost("{id}/upload-cover")]
        public async Task<ActionResult<PlaylistDto>> UploadPlaylistCover(int id, IFormFile coverImage)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == currentUserId);

                if (playlist == null)
                {
                    return NotFound(new { message = "Không tìm thấy danh sách phát" });
                }

                if (coverImage == null || coverImage.Length == 0)
                {
                    return BadRequest(new { message = "Vui lòng chọn file ảnh" });
                }

                // Validate image file
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var imageExtension = Path.GetExtension(coverImage.FileName).ToLowerInvariant();
                if (!allowedImageExtensions.Contains(imageExtension))
                {
                    return BadRequest(new { message = "Định dạng ảnh không được hỗ trợ. Chỉ chấp nhận: jpg, jpeg, png, gif, webp" });
                }

                // Validate image size (max 10MB)
                const long maxImageSize = 10 * 1024 * 1024; // 10MB
                if (coverImage.Length > maxImageSize)
                {
                    return BadRequest(new { message = "Ảnh quá lớn. Kích thước tối đa là 10MB" });
                }

                // Save image to local folder: images/playlists
                var uploadsFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "images", "playlists");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + imageExtension;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(fileStream);
                    }

                    // Return relative path: images/playlists/{filename}
                    var imageUrl = $"images/playlists/{uniqueFileName}";
                    _logger.LogInformation("Cover image saved locally: {Path}", imageUrl);

                    // Update playlist with new cover image URL
                    playlist.CoverImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving cover image to local folder");
                    return StatusCode(500, new { message = "Không thể lưu ảnh: " + ex.Message });
                }
                playlist.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                var songCount = await _context.PlaylistSongs
                    .CountAsync(ps => ps.PlaylistId == id);

                var result = new PlaylistDto
                {
                    PlaylistId = playlist.PlaylistId,
                    PlaylistName = playlist.PlaylistName,
                    Description = playlist.Description,
                    CoverImageUrl = playlist.CoverImageUrl,
                    IsPublic = playlist.IsPublic,
                    CreatedAt = playlist.CreatedAt,
                    UpdatedAt = playlist.UpdatedAt,
                    SongCount = songCount
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading playlist cover");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // DELETE: api/Playlists/{id} - Xóa playlist
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePlaylist(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var playlist = await _context.Playlists
                    .Include(p => p.PlaylistSongs)
                    .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == currentUserId);

                if (playlist == null)
                {
                    return NotFound(new { message = "Không tìm thấy danh sách phát" });
                }

                // Xóa file ảnh cover nếu có (chỉ xóa file local, không xóa Cloudflare)
                if (!string.IsNullOrEmpty(playlist.CoverImageUrl) && playlist.CoverImageUrl.StartsWith("images/playlists/"))
                {
                    try
                    {
                        var imagePath = Path.Combine(_hostingEnvironment.ContentRootPath, playlist.CoverImageUrl.Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                            _logger.LogInformation("Deleted playlist cover image: {Path}", imagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi nhưng không fail việc xóa playlist
                        _logger.LogWarning(ex, "Could not delete playlist cover image: {Path}", playlist.CoverImageUrl);
                    }
                }

                // Xóa tất cả PlaylistSongs trước khi xóa Playlist
                if (playlist.PlaylistSongs.Any())
                {
                    _context.PlaylistSongs.RemoveRange(playlist.PlaylistSongs);
                }

                // Sau đó mới xóa Playlist
                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa danh sách phát thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting playlist");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // POST: api/Playlists/{id}/songs - Thêm bài hát vào playlist
        [HttpPost("{id}/songs")]
        public async Task<ActionResult> AddSongToPlaylist(int id, [FromBody] AddSongRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == currentUserId);

                if (playlist == null)
                {
                    return NotFound(new { message = "Không tìm thấy danh sách phát" });
                }

                var song = await _context.Songs.FindAsync(request.SongId);
                if (song == null)
                {
                    return NotFound(new { message = "Không tìm thấy bài hát" });
                }

                // Kiểm tra xem bài hát đã có trong playlist chưa
                var existing = await _context.PlaylistSongs
                    .FirstOrDefaultAsync(ps => ps.PlaylistId == id && ps.SongId == request.SongId);

                if (existing != null)
                {
                    return BadRequest(new { message = "Bài hát đã có trong danh sách phát" });
                }

                // Lấy position tiếp theo (dùng Count thay vì Max để tránh lỗi translation)
                var maxPosition = await _context.PlaylistSongs
                    .Where(ps => ps.PlaylistId == id)
                    .CountAsync();

                var playlistSong = new PlaylistSong
                {
                    PlaylistId = id,
                    SongId = request.SongId,
                    Position = maxPosition + 1,
                    AddedAt = DateTime.Now
                };

                _context.PlaylistSongs.Add(playlistSong);
                playlist.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã thêm bài hát vào danh sách phát" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding song to playlist");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // DELETE: api/Playlists/{id}/songs/{songId} - Xóa bài hát khỏi playlist
        [HttpDelete("{id}/songs/{songId}")]
        public async Task<ActionResult> RemoveSongFromPlaylist(int id, int songId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == currentUserId);

                if (playlist == null)
                {
                    return NotFound(new { message = "Không tìm thấy danh sách phát" });
                }

                var playlistSong = await _context.PlaylistSongs
                    .FirstOrDefaultAsync(ps => ps.PlaylistId == id && ps.SongId == songId);

                if (playlistSong == null)
                {
                    return NotFound(new { message = "Không tìm thấy bài hát trong danh sách phát" });
                }

                _context.PlaylistSongs.Remove(playlistSong);
                playlist.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã xóa bài hát khỏi danh sách phát" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing song from playlist");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // GET: api/Playlists/search-songs - Tìm kiếm bài hát để thêm vào playlist
        [HttpGet("search-songs")]
        public async Task<ActionResult<List<SearchSongDto>>> SearchSongs([FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Songs
                    .Include(s => s.Artist)
                    .AsQueryable();

                // Case-insensitive search by song title or artist name
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(s =>
                        s.SongTitle.ToLower().Contains(searchLower) ||
                        s.Artist.ArtistName.ToLower().Contains(searchLower));
                }

                var songs = await query
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(100) // Limit to 100 results
                    .Select(s => new SearchSongDto
                    {
                        SongId = s.SongId,
                        SongTitle = s.SongTitle,
                        ArtistId = s.ArtistId,
                        ArtistName = s.Artist.ArtistName
                    })
                    .ToListAsync();

                return Ok(songs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching songs");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
    }

    // DTOs
    public class PlaylistDto
    {
        public int PlaylistId { get; set; }
        public string PlaylistName { get; set; } = null!;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int SongCount { get; set; }
        public int? OwnerId { get; set; }
        public string? OwnerName { get; set; }
    }

    public class PlaylistDetailDto
    {
        public int PlaylistId { get; set; }
        public string PlaylistName { get; set; } = null!;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PlaylistSongDto> Songs { get; set; } = new();
        public int? OwnerId { get; set; }
        public string? OwnerName { get; set; }
    }

    public class PlaylistSongDto
    {
        public int SongId { get; set; }
        public string SongTitle { get; set; } = null!;
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = null!;
        public int? AlbumId { get; set; }
        public string? AlbumTitle { get; set; }
        public int? GenreId { get; set; }
        public string? GenreName { get; set; }
        public string AudioFileUrl { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public int DurationSeconds { get; set; }
        public long PlayCount { get; set; }
        public int LikeCount { get; set; }
        public int Position { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class CreatePlaylistRequest
    {
        public string PlaylistName { get; set; } = null!;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool? IsPublic { get; set; }
    }

    public class UpdatePlaylistRequest
    {
        public string PlaylistName { get; set; } = null!;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public bool? IsPublic { get; set; }
    }

    public class AddSongRequest
    {
        public int SongId { get; set; }
    }

    public class SearchSongDto
    {
        public int SongId { get; set; }
        public string SongTitle { get; set; } = null!;
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = null!;
    }
}

