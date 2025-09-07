using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MusicStream.Data;
using MusicStream.Models;
using MusicStream.Services;

namespace MusicStream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SongController : ControllerBase
    {
        private readonly MusicStreamContext _context;
        private readonly R2Service _r2Service;
        private readonly ILogger<SongController> _logger;

        public SongController(
            MusicStreamContext context, 
            R2Service r2Service,
            ILogger<SongController> logger)
        {
            _context = context;
            _r2Service = r2Service;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<SongResponse>> UploadSong([FromForm] UploadSongRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                // Check if user is an artist
                var currentUser = await _context.Users
                    .Include(u => u.Artist)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);
                
                if (currentUser?.Artist == null)
                {
                    return StatusCode(403, new { message = "Chỉ nghệ sĩ mới có quyền upload nhạc" });
                }

                // Validate file
                if (request.AudioFile == null || request.AudioFile.Length == 0)
                {
                    return BadRequest(new { message = "Vui lòng chọn file nhạc" });
                }

                // Validate file type (chỉ cho phép audio files)
                var allowedExtensions = new[] { ".mp3", ".wav", ".m4a", ".flac", ".ogg" };
                var fileExtension = Path.GetExtension(request.AudioFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new { message = "Định dạng file không được hỗ trợ. Chỉ chấp nhận: mp3, wav, m4a, flac, ogg" });
                }

                // Validate file size (max 50MB)
                const long maxFileSize = 50 * 1024 * 1024; // 50MB
                if (request.AudioFile.Length > maxFileSize)
                {
                    return BadRequest(new { message = "File quá lớn. Kích thước tối đa là 50MB" });
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.SongTitle))
                {
                    return BadRequest(new { message = "Vui lòng nhập tên bài hát" });
                }

                if (request.ArtistId <= 0)
                {
                    return BadRequest(new { message = "Vui lòng chọn nghệ sĩ" });
                }

                // Check if artist exists
                var artist = await _context.Artists.FindAsync(request.ArtistId);
                if (artist == null)
                {
                    return NotFound(new { message = "Không tìm thấy nghệ sĩ" });
                }

                // Ensure user can only upload for their own artist profile (unless admin)
                var isAdmin = currentUser.RoleId == 1;
                if (!isAdmin && artist.ArtistId != currentUser.Artist.ArtistId)
                {
                    return StatusCode(403, new { message = "Bạn chỉ có thể upload nhạc cho hồ sơ nghệ sĩ của chính mình" });
                }

                // Check album if provided
                if (request.AlbumId.HasValue && request.AlbumId.Value > 0)
                {
                    var album = await _context.Albums.FindAsync(request.AlbumId.Value);
                    if (album == null)
                    {
                        return NotFound(new { message = "Không tìm thấy album" });
                    }
                }

                // Check genre if provided
                if (request.GenreId.HasValue && request.GenreId.Value > 0)
                {
                    var genre = await _context.Genres.FindAsync(request.GenreId.Value);
                    if (genre == null)
                    {
                        return NotFound(new { message = "Không tìm thấy thể loại" });
                    }
                }

                // Upload audio file to R2
                string audioUrl;
                try
                {
                    audioUrl = await _r2Service.UploadMusicAsync(request.AudioFile);
                    _logger.LogInformation($"Audio file uploaded to R2: {audioUrl}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading audio file to R2");
                    return StatusCode(500, new { message = "Lỗi khi upload file nhạc lên Cloudflare R2: " + ex.Message });
                }

                // Upload image file to R2 (if provided)
                string? imageUrl = null;
                if (request.ImageFile != null && request.ImageFile.Length > 0)
                {
                    // Validate image file
                    var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var imageExtension = Path.GetExtension(request.ImageFile.FileName).ToLowerInvariant();
                    if (!allowedImageExtensions.Contains(imageExtension))
                    {
                        return BadRequest(new { message = "Định dạng ảnh không được hỗ trợ. Chỉ chấp nhận: jpg, jpeg, png, gif, webp" });
                    }

                    // Validate image size (max 10MB)
                    const long maxImageSize = 10 * 1024 * 1024; // 10MB
                    if (request.ImageFile.Length > maxImageSize)
                    {
                        return BadRequest(new { message = "Ảnh quá lớn. Kích thước tối đa là 10MB" });
                    }

                    try
                    {
                        imageUrl = await _r2Service.UploadImageAsync(request.ImageFile);
                        _logger.LogInformation($"Image file uploaded to R2: {imageUrl}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image file to R2");
                        // Don't fail the entire upload if image fails, just log the error
                        _logger.LogWarning("Continuing without image upload");
                    }
                }

                // Calculate duration (simplified - in production, use audio library to get actual duration)
                // For now, we'll set a default or calculate from file size
                int durationSeconds = 180; // Default 3 minutes, should be calculated from actual audio file

                // Create song entity
                var song = new Song
                {
                    SongTitle = request.SongTitle.Trim(),
                    ArtistId = request.ArtistId,
                    AlbumId = request.AlbumId > 0 ? request.AlbumId : null,
                    GenreId = request.GenreId > 0 ? request.GenreId : null,
                    AudioFileUrl = audioUrl,
                    CoverImageUrl = imageUrl,
                    DurationSeconds = durationSeconds,
                    ReleaseDate = request.ReleaseDate,
                    Lyrics = request.Lyrics?.Trim(),
                    PlayCount = 0,
                    LikeCount = 0,
                    CreatedAt = DateTime.Now,
                    ApprovalStatus = ApprovalStatus.Pending // Chờ admin duyệt
                };

                _context.Songs.Add(song);
                await _context.SaveChangesAsync();

                // Load related data
                await _context.Entry(song)
                    .Reference(s => s.Artist)
                    .LoadAsync();
                await _context.Entry(song)
                    .Reference(s => s.Album)
                    .LoadAsync();
                await _context.Entry(song)
                    .Reference(s => s.Genre)
                    .LoadAsync();

                var response = new SongResponse
                {
                    SongId = song.SongId,
                    SongTitle = song.SongTitle,
                    ArtistId = song.ArtistId,
                    ArtistName = song.Artist.ArtistName,
                    AlbumId = song.AlbumId,
                    AlbumTitle = song.Album?.AlbumTitle,
                    GenreId = song.GenreId,
                    GenreName = song.Genre?.GenreName,
                    AudioFileUrl = song.AudioFileUrl,
                    CoverImageUrl = song.CoverImageUrl,
                    DurationSeconds = song.DurationSeconds,
                    ReleaseDate = song.ReleaseDate,
                    ApprovalStatus = song.ApprovalStatus.ToString(),
                    Message = "Upload thành công! Bài hát đang chờ admin duyệt."
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading song");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("artists")]
        public async Task<ActionResult<List<ArtistDto>>> GetArtists()
        {
            try
            {
                var artists = await _context.Artists
                    .OrderBy(a => a.ArtistName)
                    .Select(a => new ArtistDto
                    {
                        ArtistId = a.ArtistId,
                        ArtistName = a.ArtistName
                    })
                    .ToListAsync();

                return Ok(artists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting artists");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("albums")]
        public async Task<ActionResult<List<AlbumDto>>> GetAlbums([FromQuery] int? artistId = null)
        {
            try
            {
                var query = _context.Albums.AsQueryable();
                
                if (artistId.HasValue && artistId.Value > 0)
                {
                    query = query.Where(a => a.ArtistId == artistId.Value);
                }

                var albums = await query
                    .OrderBy(a => a.AlbumTitle)
                    .Select(a => new AlbumDto
                    {
                        AlbumId = a.AlbumId,
                        AlbumTitle = a.AlbumTitle,
                        ArtistId = a.ArtistId,
                        ArtistName = a.Artist.ArtistName
                    })
                    .ToListAsync();

                return Ok(albums);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting albums");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("genres")]
        public async Task<ActionResult<List<GenreDto>>> GetGenres()
        {
            try
            {
                var genres = await _context.Genres
                    .OrderBy(g => g.GenreName)
                    .Select(g => new GenreDto
                    {
                        GenreId = g.GenreId,
                        GenreName = g.GenreName
                    })
                    .ToListAsync();

                return Ok(genres);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting genres");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("my-songs")]
        public async Task<ActionResult<MySongsResponse>> GetMySongs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? approvalStatus = null)
        {
            try
            {
                // Get current user ID
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                // Check if user is an artist
                var currentUser = await _context.Users
                    .Include(u => u.Artist)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);
                
                if (currentUser?.Artist == null)
                {
                    return StatusCode(403, new { message = "Chỉ nghệ sĩ mới có thể xem danh sách bài hát của mình" });
                }

                var query = _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .Include(s => s.Genre)
                    .Where(s => s.ArtistId == currentUser.Artist.ArtistId);

                // Filter by approval status nếu có
                if (approvalStatus.HasValue)
                {
                    query = query.Where(s => (int)s.ApprovalStatus == approvalStatus.Value);
                }

                var totalCount = await query.CountAsync();

                var songs = await query
                    .OrderByDescending(s => s.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new MySongDto
                    {
                        SongId = s.SongId,
                        SongTitle = s.SongTitle,
                        AlbumId = s.AlbumId,
                        AlbumTitle = s.Album != null ? s.Album.AlbumTitle : null,
                        GenreId = s.GenreId,
                        GenreName = s.Genre != null ? s.Genre.GenreName : null,
                        AudioFileUrl = s.AudioFileUrl,
                        CoverImageUrl = s.CoverImageUrl,
                        DurationSeconds = s.DurationSeconds,
                        ReleaseDate = s.ReleaseDate,
                        PlayCount = s.PlayCount,
                        LikeCount = s.LikeCount,
                        CreatedAt = s.CreatedAt,
                        ApprovalStatus = s.ApprovalStatus.ToString(),
                        ApprovedAt = s.ApprovedAt,
                        RejectionReason = s.RejectionReason
                    })
                    .ToListAsync();

                return Ok(new MySongsResponse
                {
                    Songs = songs,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my songs");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPost("{songId}/like")]
        public async Task<ActionResult> LikeSong(int songId)
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var song = await _context.Songs.FindAsync(songId);
                if (song == null)
                {
                    return NotFound(new { message = "Không tìm thấy bài hát" });
                }

                // Kiểm tra user đã like chưa
                var existingLike = await _context.UserLikes
                    .FirstOrDefaultAsync(ul => ul.UserId == currentUserId && ul.SongId == songId);

                if (existingLike != null)
                {
                    return BadRequest(new { message = "Bạn đã thích bài hát này rồi" });
                }

                // Thêm vào UserLikes và tăng likeCount
                var userLike = new UserLike
                {
                    UserId = currentUserId,
                    SongId = songId
                };
                _context.UserLikes.Add(userLike);
                
                song.LikeCount++;
                await _context.SaveChangesAsync();

                // Reload song để lấy likeCount mới nhất
                await _context.Entry(song).ReloadAsync();

                return Ok(new { 
                    message = "Đã thích bài hát",
                    likeCount = song.LikeCount 
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error when liking song. SongId: {SongId}", songId);
                // Có thể là bảng user_likes chưa tồn tại
                if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("user_likes"))
                {
                    return StatusCode(500, new { message = "Lỗi database: Bảng user_likes chưa được tạo. Vui lòng chạy migration." });
                }
                return StatusCode(500, new { message = "Lỗi database: " + dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking song. SongId: {SongId}, Exception: {Exception}", songId, ex.ToString());
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpDelete("{songId}/like")]
        public async Task<ActionResult> UnlikeSong(int songId)
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var song = await _context.Songs.FindAsync(songId);
                if (song == null)
                {
                    return NotFound(new { message = "Không tìm thấy bài hát" });
                }

                // Tìm UserLike record
                var userLike = await _context.UserLikes
                    .FirstOrDefaultAsync(ul => ul.UserId == currentUserId && ul.SongId == songId);

                if (userLike == null)
                {
                    return BadRequest(new { message = "Bạn chưa thích bài hát này" });
                }

                // Xóa UserLike và giảm likeCount
                _context.UserLikes.Remove(userLike);
                
                if (song.LikeCount > 0)
                {
                    song.LikeCount--;
                }
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Đã bỏ thích bài hát",
                    likeCount = song.LikeCount 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking song");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("{songId}/like-status")]
        public async Task<ActionResult> GetLikeStatus(int songId)
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                // Kiểm tra user đã like chưa
                var userLike = await _context.UserLikes
                    .FirstOrDefaultAsync(ul => ul.UserId == currentUserId && ul.SongId == songId);

                return Ok(new { 
                    isLiked = userLike != null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting like status");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPost("track-play")]
        public async Task<ActionResult> TrackPlay([FromBody] TrackPlayRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var song = await _context.Songs.FindAsync(request.SongId);
                if (song == null)
                {
                    return NotFound(new { message = "Không tìm thấy bài hát" });
                }

                // Reload để đảm bảo có dữ liệu mới nhất
                await _context.Entry(song).ReloadAsync();

                // Kiểm tra xem đã có listening history cho bài này trong 1 phút gần đây chưa
                // (để tránh tạo duplicate khi gọi nhiều lần)
                var recentHistory = await _context.ListeningHistories
                    .Where(lh => lh.UserId == currentUserId 
                        && lh.SongId == request.SongId 
                        && lh.PlayedAt >= DateTime.UtcNow.AddMinutes(-1))
                    .OrderByDescending(lh => lh.PlayedAt)
                    .FirstOrDefaultAsync();

                if (recentHistory == null)
                {
                    // Tạo listening history mới cho lần nghe này.
                    // Frontend đã đảm bảo chỉ gọi API này khi bắt đầu phát (duration = 0)
                    // và khi người dùng đã nghe >= 30% bài hát.
                    var listeningHistory = new ListeningHistory
                    {
                        UserId = currentUserId,
                        SongId = request.SongId,
                        PlayedAt = DateTime.UtcNow,
                        DurationPlayed = request.DurationPlayed,
                        Completed = true
                    };
                    _context.ListeningHistories.Add(listeningHistory);

                    // Mỗi lần nghe mới (trong khoảng thời gian > 1 phút) tăng playCount 1 lần
                    song.PlayCount++;
                }
                else if (!recentHistory.Completed)
                {
                    // Lần đầu trong khoảng 1 phút mà frontend báo đã nghe đủ,
                    // cập nhật history và tăng playCount 1 lần.
                    recentHistory.DurationPlayed = request.DurationPlayed;
                    recentHistory.Completed = true;
                    song.PlayCount++;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã cập nhật play count", playCount = song.PlayCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking play count");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPut("update-album")]
        public async Task<IActionResult> UpdateSongAlbum([FromBody] UpdateSongAlbumRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var currentUser = await _context.Users
                    .Include(u => u.Artist)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (currentUser == null)
                {
                    return Unauthorized(new { message = "Người dùng không tồn tại" });
                }

                var song = await _context.Songs
                    .Include(s => s.Artist)
                    .FirstOrDefaultAsync(s => s.SongId == request.SongId);

                if (song == null)
                {
                    return NotFound(new { message = "Không tìm thấy bài hát" });
                }

                var isAdmin = currentUser.RoleId == 1;
                var isOwner = currentUser.Artist != null && currentUser.Artist.ArtistId == song.ArtistId;

                if (!isAdmin && !isOwner)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền chỉnh sửa bài hát này" });
                }

                // If albumId is provided, check if album exists and belongs to the same artist
                if (request.AlbumId.HasValue)
                {
                    var album = await _context.Albums.FindAsync(request.AlbumId.Value);
                    if (album == null)
                    {
                        return NotFound(new { message = "Không tìm thấy album" });
                    }

                    if (album.ArtistId != song.ArtistId)
                    {
                        return BadRequest(new { message = "Album không thuộc về nghệ sĩ của bài hát" });
                    }
                }

                // Save old album to update statistics
                var oldAlbumId = song.AlbumId;

                // Update album for the song
                song.AlbumId = request.AlbumId;
                await _context.SaveChangesAsync();

                // Update statistics for the old album (if any)
                if (oldAlbumId.HasValue)
                {
                    await UpdateAlbumStatistics(oldAlbumId.Value);
                }

                // Update statistics for the new album (if any)
                if (request.AlbumId.HasValue)
                {
                    await UpdateAlbumStatistics(request.AlbumId.Value);
                }

                return Ok(new { message = "Đã cập nhật album cho bài hát" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating song album");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // Helper method to update album statistics
        private async Task UpdateAlbumStatistics(int albumId)
        {
            var album = await _context.Albums
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(a => a.AlbumId == albumId);

            if (album != null)
            {
                album.TotalTracks = album.Songs.Count;
                album.DurationSeconds = album.Songs.Sum(s => s.DurationSeconds);
                await _context.SaveChangesAsync();
            }
        }

    }

    // DTOs
    public class UploadSongRequest
    {
        public IFormFile AudioFile { get; set; } = null!;
        public IFormFile? ImageFile { get; set; }
        public string SongTitle { get; set; } = null!;
        public int ArtistId { get; set; }
        public int? AlbumId { get; set; }
        public int? GenreId { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Lyrics { get; set; }
    }

    public class SongResponse
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
        public DateTime? ReleaseDate { get; set; }
        public string ApprovalStatus { get; set; } = null!;
        public string Message { get; set; } = null!;
    }

    public class ArtistDto
    {
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = null!;
    }

    public class AlbumDto
    {
        public int AlbumId { get; set; }
        public string AlbumTitle { get; set; } = null!;
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = null!;
    }

    public class GenreDto
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; } = null!;
    }

    public class MySongDto
    {
        public int SongId { get; set; }
        public string SongTitle { get; set; } = null!;
        public int? AlbumId { get; set; }
        public string? AlbumTitle { get; set; }
        public int? GenreId { get; set; }
        public string? GenreName { get; set; }
        public string AudioFileUrl { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public long PlayCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ApprovalStatus { get; set; } = null!;
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class MySongsResponse
    {
        public List<MySongDto> Songs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class TrackPlayRequest
    {
        public int SongId { get; set; }
        public int DurationPlayed { get; set; } // Thời gian đã nghe (giây)
    }

    public class UpdateSongAlbumRequest
    {
        public int SongId { get; set; }
        public int? AlbumId { get; set; }
    }
}

