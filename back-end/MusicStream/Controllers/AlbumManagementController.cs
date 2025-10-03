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
    public class AlbumManagementController : ControllerBase
    {
        private readonly MusicStreamContext _context;
        private readonly ILogger<AlbumManagementController> _logger;

        public AlbumManagementController(
            MusicStreamContext context,
            ILogger<AlbumManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("all")]
        public async Task<ActionResult<AdminAlbumListResponse>> GetAllAlbums(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? artistId = null)
        {
            try
            {
                var query = _context.Albums
                    .Include(a => a.Artist)
                    .AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(a =>
                        a.AlbumTitle.Contains(search) ||
                        a.Artist.ArtistName.Contains(search));
                }

                // Artist filter
                if (artistId.HasValue && artistId.Value > 0)
                {
                    query = query.Where(a => a.ArtistId == artistId.Value);
                }

                var totalCount = await query.CountAsync();

                var albums = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new AdminAlbumDetailDto
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

                return Ok(new AdminAlbumListResponse
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
                _logger.LogError(ex, "Error getting all albums");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminAlbumDetailDto>> GetAlbumById(int id)
        {
            try
            {
                var album = await _context.Albums
                    .Include(a => a.Artist)
                    .Where(a => a.AlbumId == id)
                    .Select(a => new AdminAlbumDetailDto
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
                    .FirstOrDefaultAsync();

                if (album == null)
                {
                    return NotFound(new { message = "Không tìm thấy album" });
                }

                return Ok(album);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting album by id");
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

                // Xóa tất cả bài hát trong album và các bảng liên quan
                if (album.Songs.Any())
                {
                    var songIds = album.Songs.Select(s => s.SongId).ToList();

                    // Xóa PlaylistSongs (bài hát trong playlist)
                    var playlistSongs = await _context.PlaylistSongs
                        .Where(ps => songIds.Contains(ps.SongId))
                        .ToListAsync();
                    if (playlistSongs.Any())
                    {
                        _context.PlaylistSongs.RemoveRange(playlistSongs);
                    }

                    // Xóa UserFavorites (yêu thích)
                    var userFavorites = await _context.UserFavorites
                        .Where(uf => songIds.Contains(uf.SongId))
                        .ToListAsync();
                    if (userFavorites.Any())
                    {
                        _context.UserFavorites.RemoveRange(userFavorites);
                    }

                    // Xóa UserLikes (like)
                    var userLikes = await _context.UserLikes
                        .Where(ul => songIds.Contains(ul.SongId))
                        .ToListAsync();
                    if (userLikes.Any())
                    {
                        _context.UserLikes.RemoveRange(userLikes);
                    }

                    // Xóa ListeningHistories (lịch sử nghe)
                    var listeningHistories = await _context.ListeningHistories
                        .Where(lh => songIds.Contains(lh.SongId))
                        .ToListAsync();
                    if (listeningHistories.Any())
                    {
                        _context.ListeningHistories.RemoveRange(listeningHistories);
                    }

                    // Xóa Songs
                    _context.Songs.RemoveRange(album.Songs);
                }

                // Xóa Album
                _context.Albums.Remove(album);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Album deleted: {album.AlbumId} - {album.AlbumTitle}");

                return Ok(new { message = "Xóa album thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting album");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
    }

    // DTOs for Album Management
    public class AdminAlbumDetailDto
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

    public class AdminAlbumListResponse
    {
        public List<AdminAlbumDetailDto> Albums { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}

