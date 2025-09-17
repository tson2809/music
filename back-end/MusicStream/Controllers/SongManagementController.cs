using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MusicStream.Data;
using MusicStream.Models;

namespace MusicStream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1")] // Chỉ cho phép Admin (RoleId = 1)
    public class SongManagementController : ControllerBase
    {
        private readonly MusicStreamContext _context;
        private readonly ILogger<SongManagementController> _logger;

        public SongManagementController(
            MusicStreamContext context,
            ILogger<SongManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<ActionResult<SongListResponse>> GetAllSongs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? approvalStatus = null,
            [FromQuery] int? genreId = null)
        {
            try
            {
                var query = _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .Include(s => s.Genre)
                    .AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(s =>
                        s.SongTitle.Contains(search) ||
                        s.Artist.ArtistName.Contains(search));
                }

                // Approval status filter
                if (approvalStatus.HasValue)
                {
                    query = query.Where(s => (int)s.ApprovalStatus == approvalStatus.Value);
                }

                // Genre filter
                if (genreId.HasValue)
                {
                    query = query.Where(s => s.GenreId == genreId.Value);
                }

                var totalCount = await query.CountAsync();

                var songs = await query
                    .OrderByDescending(s => s.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new SongDetailDto
                    {
                        SongId = s.SongId,
                        SongTitle = s.SongTitle,
                        ArtistId = s.ArtistId,
                        ArtistName = s.Artist.ArtistName,
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

                return Ok(new SongListResponse
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
                _logger.LogError(ex, "Error getting all songs");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("pending")]
        public async Task<ActionResult<SongListResponse>> GetPendingSongs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .Include(s => s.Genre)
                    .Where(s => s.ApprovalStatus == ApprovalStatus.Pending);

                // Search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(s =>
                        s.SongTitle.Contains(search) ||
                        s.Artist.ArtistName.Contains(search));
                }

                var totalCount = await query.CountAsync();

                var songs = await query
                    .OrderBy(s => s.CreatedAt) // Bài cũ nhất ưu tiên duyệt trước
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new SongDetailDto
                    {
                        SongId = s.SongId,
                        SongTitle = s.SongTitle,
                        ArtistId = s.ArtistId,
                        ArtistName = s.Artist.ArtistName,
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
                        ApprovalStatus = s.ApprovalStatus.ToString()
                    })
                    .ToListAsync();

                return Ok(new SongListResponse
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
                _logger.LogError(ex, "Error getting pending songs");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPut("approve/{id}")]
        public async Task<ActionResult> ApproveSong(int id)
        {
            try
            {
                // Get current admin user ID
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int adminUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var song = await _context.Songs.FindAsync(id);
                if (song == null)
                {
                    return NotFound(new { message = "Không tìm thấy bài hát" });
                }

                if (song.ApprovalStatus == ApprovalStatus.Approved)
                {
                    return BadRequest(new { message = "Bài hát đã được duyệt trước đó" });
                }

                song.ApprovalStatus = ApprovalStatus.Approved;
                song.ApprovedAt = DateTime.Now;
                song.ApprovedByUserId = adminUserId;
                song.RejectionReason = null; // Clear rejection reason nếu có

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Song approved: {song.SongId} - {song.SongTitle} by admin {adminUserId}");

                return Ok(new { message = "Duyệt bài hát thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving song");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPut("reject/{id}")]
        public async Task<ActionResult> RejectSong(int id, [FromBody] RejectSongRequest request)
        {
            try
            {
                // Get current admin user ID
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int adminUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var song = await _context.Songs.FindAsync(id);
                if (song == null)
                {
                    return NotFound(new { message = "Không tìm thấy bài hát" });
                }

                if (song.ApprovalStatus == ApprovalStatus.Rejected)
                {
                    return BadRequest(new { message = "Bài hát đã bị từ chối trước đó" });
                }

                song.ApprovalStatus = ApprovalStatus.Rejected;
                song.ApprovedAt = DateTime.Now; // Lưu thời điểm từ chối
                song.ApprovedByUserId = adminUserId;
                song.RejectionReason = request.RejectionReason?.Trim();

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Song rejected: {song.SongId} - {song.SongTitle} by admin {adminUserId}. Reason: {song.RejectionReason}");

                return Ok(new { message = "Từ chối bài hát thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting song");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SongDetailDto>> GetSongById(int id)
        {
            try
            {
                var song = await _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .Include(s => s.Genre)
                    .Where(s => s.SongId == id)
                    .Select(s => new SongDetailDto
                    {
                        SongId = s.SongId,
                        SongTitle = s.SongTitle,
                        ArtistId = s.ArtistId,
                        ArtistName = s.Artist.ArtistName,
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
                        CreatedAt = s.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (song == null)
                {
                    return NotFound(new { message = "Không tìm thấy bài hát" });
                }

                return Ok(song);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting song by id");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSong(int id)
        {
            try
            {
                var song = await _context.Songs.FindAsync(id);
                if (song == null)
                {
                    return NotFound(new { message = "Không tìm thấy bài hát" });
                }

                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Song deleted: {song.SongId} - {song.SongTitle}");

                return Ok(new { message = "Xóa bài hát thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting song");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
    }

    // DTOs for Song Management
    public class SongDetailDto
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
        public long PlayCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ApprovalStatus { get; set; } = null!;
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class SongListResponse
    {
        public List<SongDetailDto> Songs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class RejectSongRequest
    {
        public string? RejectionReason { get; set; }
    }
}

