using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MusicStream.Data;
using MusicStream.Models;
using System.Security.Claims;

namespace MusicStream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly MusicStreamContext _context;
        private readonly ILogger<FavoritesController> _logger;

        public FavoritesController(
            MusicStreamContext context,
            ILogger<FavoritesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("songs")]
        public async Task<ActionResult<List<FavoriteSongDto>>> GetFavoriteSongs()
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    _logger.LogWarning("Cannot get user ID from token");
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                _logger.LogInformation("Getting favorite songs for user ID: {UserId}", currentUserId);

                // Check if user has any favorites
                var favoriteCount = await _context.UserFavorites
                    .CountAsync(uf => uf.UserId == currentUserId);
                
                _logger.LogInformation("User {UserId} has {Count} favorite songs", currentUserId, favoriteCount);

                // Get favorite songs for current user
                var userFavorites = await _context.UserFavorites
                    .Where(uf => uf.UserId == currentUserId)
                    .Include(uf => uf.Song)
                        .ThenInclude(s => s.Artist)
                    .Include(uf => uf.Song)
                        .ThenInclude(s => s.Album)
                    .Include(uf => uf.Song)
                        .ThenInclude(s => s.Genre)
                    .OrderByDescending(uf => uf.Song.CreatedAt)
                    .ToListAsync();

                var favoriteSongs = userFavorites.Select(uf => new FavoriteSongDto
                {
                    SongId = uf.Song.SongId,
                    SongTitle = uf.Song.SongTitle,
                    ArtistId = uf.Song.ArtistId,
                    ArtistName = uf.Song.Artist.ArtistName,
                    AlbumId = uf.Song.AlbumId,
                    AlbumTitle = uf.Song.Album != null ? uf.Song.Album.AlbumTitle : null,
                    GenreId = uf.Song.GenreId,
                    GenreName = uf.Song.Genre != null ? uf.Song.Genre.GenreName : null,
                    AudioFileUrl = uf.Song.AudioFileUrl,
                    CoverImageUrl = uf.Song.CoverImageUrl,
                    DurationSeconds = uf.Song.DurationSeconds,
                    ReleaseDate = uf.Song.ReleaseDate,
                    PlayCount = uf.Song.PlayCount,
                    LikeCount = uf.Song.LikeCount,
                    CreatedAt = uf.Song.CreatedAt
                }).ToList();

                _logger.LogInformation("Returning {Count} favorite songs for user {UserId}", favoriteSongs.Count, currentUserId);

                return Ok(favoriteSongs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite songs");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPost("songs/{songId}")]
        public async Task<ActionResult> AddToFavorites(int songId)
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                // Check if song exists
                var song = await _context.Songs.FindAsync(songId);
                if (song == null)
                {
                    return NotFound(new { message = "Không tìm thấy bài hát" });
                }

                // Check if already in favorites
                var existingFavorite = await _context.UserFavorites
                    .FirstOrDefaultAsync(uf => uf.UserId == currentUserId && uf.SongId == songId);

                if (existingFavorite != null)
                {
                    return BadRequest(new { message = "Bài hát đã có trong danh sách yêu thích" });
                }

                // Add to favorites
                var favorite = new UserFavorite
                {
                    UserId = currentUserId,
                    SongId = songId
                };

                _context.UserFavorites.Add(favorite);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã thêm vào yêu thích" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to favorites");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpDelete("songs/{songId}")]
        public async Task<ActionResult> RemoveFromFavorites(int songId)
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                // Find favorite
                var favorite = await _context.UserFavorites
                    .FirstOrDefaultAsync(uf => uf.UserId == currentUserId && uf.SongId == songId);

                if (favorite == null)
                {
                    return NotFound(new { message = "Không tìm thấy trong danh sách yêu thích" });
                }

                // Remove from favorites
                _context.UserFavorites.Remove(favorite);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã xóa khỏi yêu thích" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from favorites");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
    }

    // DTOs
    public class FavoriteSongDto
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
    }
}

