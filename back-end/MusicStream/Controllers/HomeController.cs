using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStream.Data;
using MusicStream.Models;

namespace MusicStream.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MusicStreamContext _context;

        public HomeController(ILogger<HomeController> logger, MusicStreamContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("/api/home/songs")]
        public async Task<IActionResult> GetSongs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            if (page <= 0) { page = 1; }
            if (pageSize <= 0) { pageSize = 20; }

            try
            {
                var query = _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .Include(s => s.Genre)
                    .Where(s => s.ApprovalStatus == ApprovalStatus.Approved) // Chỉ lấy bài đã duyệt
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(s =>
                        s.SongTitle.Contains(search) ||
                        s.Artist.ArtistName.Contains(search));
                }

                var totalCount = await query.CountAsync();
                var songs = await query
                    .OrderByDescending(s => s.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new HomeSongDto
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
                    .ToListAsync();

                var response = new HomeSongListResponse
                {
                    Songs = songs,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching songs for home");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("/api/home/songs/{id}")]
        public async Task<IActionResult> GetSongById(int id)
        {
            try
            {
                var song = await _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .Include(s => s.Genre)
                    .Where(s => s.SongId == id && s.ApprovalStatus == ApprovalStatus.Approved) // Chỉ lấy bài đã duyệt
                    .Select(s => new HomeSongDetailDto
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
                        Lyrics = s.Lyrics,
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
                _logger.LogError(ex, "Error fetching song detail");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("/api/home/artists/{id}")]
        public async Task<IActionResult> GetArtistById(int id)
        {
            try
            {
                var artist = await _context.Artists
                    .Include(a => a.User)
                    .Where(a => a.ArtistId == id)
                    .Select(a => new ArtistDetailDto
                    {
                        ArtistId = a.ArtistId,
                        ArtistName = a.ArtistName,
                        Biography = a.Biography,
                        Country = a.User != null ? a.User.Country : null,
                        ProfileImageUrl = a.User != null ? a.User.ProfilePictureUrl : null, // Lấy ảnh từ User
                        Verified = a.Verified,
                        MonthlyListeners = a.MonthlyListeners
                    })
                    .FirstOrDefaultAsync();

                if (artist == null)
                {
                    return NotFound(new { message = "Không tìm thấy nghệ sĩ" });
                }

                return Ok(artist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching artist detail");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("/api/home/artists/{id}/songs")]
        public async Task<IActionResult> GetArtistSongs(
            int id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page <= 0) { page = 1; }
            if (pageSize <= 0) { pageSize = 50; }

            try
            {
                var query = _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .Include(s => s.Genre)
                    .Where(s => s.ArtistId == id && s.ApprovalStatus == ApprovalStatus.Approved)
                    .AsQueryable();

                var totalCount = await query.CountAsync();
                var songs = await query
                    .OrderByDescending(s => s.PlayCount)
                    .ThenByDescending(s => s.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new HomeSongDto
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
                    .ToListAsync();

                var response = new HomeSongListResponse
                {
                    Songs = songs,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching artist songs");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("/api/home/songs/popular")]
        public async Task<IActionResult> GetPopularSongs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) { page = 1; }
            if (pageSize <= 0) { pageSize = 20; }

            try
            {
                var query = _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .Include(s => s.Genre)
                    .Where(s => s.ApprovalStatus == ApprovalStatus.Approved)
                    .AsQueryable();

                var totalCount = await query.CountAsync();
                var songs = await query
                    .OrderByDescending(s => s.PlayCount)
                    .ThenByDescending(s => s.LikeCount)
                    .ThenByDescending(s => s.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new HomeSongDto
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
                    .ToListAsync();

                var response = new HomeSongListResponse
                {
                    Songs = songs,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching popular songs");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("/api/home/songs/genre/{genreId}")]
        public async Task<IActionResult> GetSongsByGenre(
            int genreId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) { page = 1; }
            if (pageSize <= 0) { pageSize = 20; }

            try
            {
                var query = _context.Songs
                    .Include(s => s.Artist)
                    .Include(s => s.Album)
                    .Include(s => s.Genre)
                    .Where(s => s.ApprovalStatus == ApprovalStatus.Approved && s.GenreId == genreId)
                    .AsQueryable();

                var totalCount = await query.CountAsync();
                var songs = await query
                    .OrderByDescending(s => s.PlayCount)
                    .ThenByDescending(s => s.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new HomeSongDto
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
                    .ToListAsync();

                var response = new HomeSongListResponse
                {
                    Songs = songs,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching songs by genre");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("/api/home/artists")]
        public async Task<IActionResult> GetAllArtists(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            if (page <= 0) { page = 1; }
            if (pageSize <= 0) { pageSize = 20; }

            try
            {
                var query = _context.Artists
                    .Include(a => a.User)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(a => a.ArtistName.Contains(search));
                }

                var totalCount = await query.CountAsync();
                var artists = await query
                    .OrderBy(a => a.ArtistName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new PublicArtistDto
                    {
                        ArtistId = a.ArtistId,
                        ArtistName = a.ArtistName,
                        ProfileImageUrl = a.User != null ? a.User.ProfilePictureUrl : null,
                        Verified = a.Verified,
                        MonthlyListeners = a.MonthlyListeners
                    })
                    .ToListAsync();

                var response = new PublicArtistListResponse
                {
                    Artists = artists,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching artists");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("/api/home/albums")]
        public async Task<IActionResult> GetAllAlbums(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            if (page <= 0) { page = 1; }
            if (pageSize <= 0) { pageSize = 20; }

            try
            {
                var query = _context.Albums
                    .Include(a => a.Artist)
                    .AsQueryable();

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
                    .Select(a => new PublicAlbumDto
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

                var response = new PublicAlbumListResponse
                {
                    Albums = albums,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching albums");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
    

        [HttpGet("/api/home/recently-played")]
        [Authorize]
        public async Task<IActionResult> GetRecentlyPlayed([FromQuery] int limit = 20)
        {
            try
            {
                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                if (limit <= 0) { limit = 20; }
                if (limit > 50) { limit = 50; }

                // Get recently played songs from listening history (include both completed and not completed)
                // Only return songs, not artists or albums
                // Get all listening histories, then group by SongId in memory to get distinct songs
                var allHistories = await _context.ListeningHistories
                    .Include(lh => lh.Song)
                        .ThenInclude(s => s.Artist)
                    .Include(lh => lh.Song)
                        .ThenInclude(s => s.Album)
                    .Where(lh => lh.UserId == currentUserId)
                    .OrderByDescending(lh => lh.PlayedAt)
                    .ToListAsync();

                // Group by SongId and take the most recent play for each song
                var recentSongs = allHistories
                    .GroupBy(lh => lh.SongId)
                    .Select(g => g.First()) // Take the first (most recent) for each song
                    .Take(limit)
                    .Select(lh => new RecentlyPlayedItemDto
                    {
                        Type = "song",
                        Id = lh.Song.SongId,
                        Title = lh.Song.SongTitle,
                        Subtitle = lh.Song.Artist.ArtistName,
                        ImageUrl = lh.Song.CoverImageUrl,
                        PlayedAt = lh.PlayedAt
                    })
                    .ToList();

                // Only return songs
                var allItems = recentSongs;

                return Ok(new RecentlyPlayedResponse
                {
                    Items = allItems,
                    TotalCount = allItems.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recently played items");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
    }
}

public class ArtistDetailDto
{
    public int ArtistId { get; set; }
    public string ArtistName { get; set; } = null!;
    public string? Biography { get; set; }
    public string? Country { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool Verified { get; set; }
    public int MonthlyListeners { get; set; }
}

public class HomeSongDetailDto
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
    public string? Lyrics { get; set; }
    public long PlayCount { get; set; }
    public int LikeCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HomeSongDto
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

public class HomeSongListResponse
{
    public List<HomeSongDto> Songs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PublicArtistDto
{
    public int ArtistId { get; set; }
    public string ArtistName { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public bool Verified { get; set; }
    public int MonthlyListeners { get; set; }
}

public class PublicArtistListResponse
{
    public List<PublicArtistDto> Artists { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PublicAlbumDto
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

public class PublicAlbumListResponse
{
    public List<PublicAlbumDto> Albums { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
public class RecentlyPlayedItemDto
{
    public string Type { get; set; } = null!; // "song", "artist", "album", "playlist"
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Subtitle { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public DateTime PlayedAt { get; set; }
}

public class RecentlyPlayedResponse
{
    public List<RecentlyPlayedItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}