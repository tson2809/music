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
    public class StatisticsController : ControllerBase
    {
        private readonly MusicStreamContext _context;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(
            MusicStreamContext context,
            ILogger<StatisticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("overview")]
        public async Task<ActionResult<StatisticsOverviewDto>> GetOverview()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalArtists = await _context.Artists.CountAsync();
                var totalSongs = await _context.Songs.CountAsync();
                var totalAlbums = await _context.Albums.CountAsync();
                var approvedSongs = await _context.Songs.CountAsync(s => s.ApprovalStatus == ApprovalStatus.Approved);
                var pendingSongs = await _context.Songs.CountAsync(s => s.ApprovalStatus == ApprovalStatus.Pending);
                var rejectedSongs = await _context.Songs.CountAsync(s => s.ApprovalStatus == ApprovalStatus.Rejected);

                return Ok(new StatisticsOverviewDto
                {
                    TotalUsers = totalUsers,
                    TotalArtists = totalArtists,
                    TotalSongs = totalSongs,
                    TotalAlbums = totalAlbums,
                    ApprovedSongs = approvedSongs,
                    PendingSongs = pendingSongs,
                    RejectedSongs = rejectedSongs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics overview");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("genres")]
        public async Task<ActionResult<List<GenreStatisticsDto>>> GetGenreStatistics()
        {
            try
            {
                var genreStats = await _context.Genres
                    .Select(g => new GenreStatisticsDto
                    {
                        GenreId = g.GenreId,
                        GenreName = g.GenreName,
                        SongCount = g.Songs.Count,
                        TotalPlayCount = g.Songs.Sum(s => s.PlayCount),
                        TotalLikeCount = g.Songs.Sum(s => s.LikeCount)
                    })
                    .OrderByDescending(g => g.SongCount)
                    .ToListAsync();

                return Ok(genreStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting genre statistics");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult<UsersListResponse>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Users
                    .Include(u => u.Role)
                    .AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(u =>
                        u.Username.Contains(search) ||
                        u.Email.Contains(search) ||
                        (u.FullName != null && u.FullName.Contains(search)));
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.UserId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new StatisticsUserDto
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        FullName = u.FullName,
                        Country = u.Country,
                        RoleId = u.RoleId,
                        RoleName = u.Role.RoleName,
                        IsActive = u.IsActive,
                        DateOfBirth = u.DateOfBirth,
                        ProfilePictureUrl = u.ProfilePictureUrl
                    })
                    .ToListAsync();

                return Ok(new UsersListResponse
                {
                    Users = users,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("artists")]
        public async Task<ActionResult<ArtistsListResponse>> GetArtists(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Artists
                    .Include(a => a.User)
                    .AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(a =>
                        a.ArtistName.Contains(search) ||
                        (a.Biography != null && a.Biography.Contains(search)) ||
                        (a.User != null && a.User.Country != null && a.User.Country.Contains(search)));
                }

                var totalCount = await query.CountAsync();

                var artists = await query
                    .OrderByDescending(a => a.ArtistId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new StatisticsArtistDto
                    {
                        ArtistId = a.ArtistId,
                        ArtistName = a.ArtistName,
                        Biography = a.Biography,
                        Country = a.User != null ? a.User.Country : null,
                        Verified = a.Verified,
                        MonthlyListeners = a.MonthlyListeners,
                        UserId = a.UserId,
                        Username = a.User != null ? a.User.Username : null,
                        Email = a.User != null ? a.User.Email : null,
                        SongCount = a.Songs.Count,
                        AlbumCount = a.Albums.Count
                    })
                    .ToListAsync();

                return Ok(new ArtistsListResponse
                {
                    Artists = artists,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting artists");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("songs")]
        public async Task<ActionResult<SongsListResponse>> GetSongs(
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
                    .AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(s =>
                        s.SongTitle.Contains(search) ||
                        s.Artist.ArtistName.Contains(search) ||
                        (s.Album != null && s.Album.AlbumTitle.Contains(search)));
                }

                var totalCount = await query.CountAsync();

                var songs = await query
                    .OrderByDescending(s => s.SongId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new StatisticsSongDto
                    {
                        SongId = s.SongId,
                        SongTitle = s.SongTitle,
                        ArtistId = s.ArtistId,
                        ArtistName = s.Artist.ArtistName,
                        AlbumId = s.AlbumId,
                        AlbumTitle = s.Album != null ? s.Album.AlbumTitle : null,
                        GenreId = s.GenreId,
                        GenreName = s.Genre != null ? s.Genre.GenreName : null,
                        DurationSeconds = s.DurationSeconds,
                        PlayCount = s.PlayCount,
                        LikeCount = s.LikeCount,
                        ApprovalStatus = s.ApprovalStatus.ToString(),
                        CreatedAt = s.CreatedAt,
                        ReleaseDate = s.ReleaseDate
                    })
                    .ToListAsync();

                return Ok(new SongsListResponse
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
                _logger.LogError(ex, "Error getting songs");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("albums")]
        public async Task<ActionResult<AlbumsListResponse>> GetAlbums(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null)
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

                var totalCount = await query.CountAsync();

                var albums = await query
                    .OrderByDescending(a => a.AlbumId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new StatisticsAlbumDto
                    {
                        AlbumId = a.AlbumId,
                        AlbumTitle = a.AlbumTitle,
                        ArtistId = a.ArtistId,
                        ArtistName = a.Artist.ArtistName,
                        AlbumType = a.AlbumType,
                        TotalTracks = a.TotalTracks,
                        DurationSeconds = a.DurationSeconds,
                        ReleaseDate = a.ReleaseDate,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new AlbumsListResponse
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
                _logger.LogError(ex, "Error getting albums");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("listening-history")]
        public async Task<ActionResult<ListeningHistoryResponse>> GetListeningHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.ListeningHistories
                    .Include(lh => lh.User)
                    .Include(lh => lh.Song)
                        .ThenInclude(s => s.Artist)
                    .AsQueryable();

                var totalCount = await query.CountAsync();

                var history = await query
                    .OrderByDescending(lh => lh.PlayedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(lh => new ListeningHistoryDto
                    {
                        HistoryId = lh.HistoryId,
                        UserId = lh.UserId,
                        UserName = lh.User.Username,
                        SongId = lh.SongId,
                        SongTitle = lh.Song.SongTitle,
                        ArtistName = lh.Song.Artist.ArtistName,
                        PlayedAt = lh.PlayedAt,
                        DurationPlayed = lh.DurationPlayed,
                        Completed = lh.Completed
                    })
                    .ToListAsync();

                return Ok(new ListeningHistoryResponse
                {
                    History = history,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting listening history");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("trends")]
        public async Task<ActionResult<TrendsResponse>> GetTrends(
            [FromQuery] string period = "daily", // daily, weekly, monthly
            [FromQuery] int days = 30)
        {
            try
            {
                var endDate = DateTime.UtcNow.Date;
                var startDate = period switch
                {
                    "weekly" => endDate.AddDays(-days * 7),
                    "monthly" => endDate.AddMonths(-days),
                    _ => endDate.AddDays(-days)
                };

                var trends = new List<TrendDataDto>();

                if (period == "daily")
                {
                    var dailyStats = await _context.ListeningHistories
                        .Where(lh => lh.PlayedAt >= startDate && lh.Completed)
                        .GroupBy(lh => lh.PlayedAt.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            PlayCount = g.Count(),
                            UniqueSongs = g.Select(lh => lh.SongId).Distinct().Count()
                        })
                        .OrderBy(x => x.Date)
                        .ToListAsync();

                    // Fill missing dates
                    for (var date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        var dayStat = dailyStats.FirstOrDefault(s => s.Date.Date == date);
                        trends.Add(new TrendDataDto
                        {
                            Date = date,
                            Label = date.ToString("dd/MM"),
                            PlayCount = dayStat?.PlayCount ?? 0,
                            UniqueSongs = dayStat?.UniqueSongs ?? 0,
                            TopSongs = new List<TopSongDto>()
                        });
                    }
                }
                else if (period == "weekly")
                {
                    var weeklyStats = await _context.ListeningHistories
                        .Where(lh => lh.PlayedAt >= startDate && lh.Completed)
                        .GroupBy(lh => new { Year = lh.PlayedAt.Year, Week = EF.Functions.DateDiffWeek(lh.PlayedAt, startDate) })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Week = g.Key.Week,
                            PlayCount = g.Count(),
                            UniqueSongs = g.Select(lh => lh.SongId).Distinct().Count()
                        })
                        .ToListAsync();

                    var currentDate = startDate;
                    while (currentDate <= endDate)
                    {
                        var weekStart = currentDate.AddDays(-(int)currentDate.DayOfWeek + (int)DayOfWeek.Monday);
                        var weekNum = (int)Math.Floor((currentDate - startDate).TotalDays / 7);
                        var weekStat = weeklyStats.FirstOrDefault(s => s.Week == weekNum);
                        
                        trends.Add(new TrendDataDto
                        {
                            Date = weekStart,
                            Label = $"Tuần {weekNum + 1}",
                            PlayCount = weekStat?.PlayCount ?? 0,
                            UniqueSongs = weekStat?.UniqueSongs ?? 0,
                            TopSongs = new List<TopSongDto>()
                        });

                        currentDate = weekStart.AddDays(7);
                    }
                }
                else // monthly
                {
                    var monthlyStats = await _context.ListeningHistories
                        .Where(lh => lh.PlayedAt >= startDate && lh.Completed)
                        .GroupBy(lh => new { lh.PlayedAt.Year, lh.PlayedAt.Month })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            PlayCount = g.Count(),
                            UniqueSongs = g.Select(lh => lh.SongId).Distinct().Count()
                        })
                        .OrderBy(x => x.Year).ThenBy(x => x.Month)
                        .ToListAsync();

                    var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
                    while (currentDate <= endDate)
                    {
                        var monthStat = monthlyStats.FirstOrDefault(s => s.Year == currentDate.Year && s.Month == currentDate.Month);
                        
                        trends.Add(new TrendDataDto
                        {
                            Date = currentDate,
                            Label = $"{currentDate:MM/yyyy}",
                            PlayCount = monthStat?.PlayCount ?? 0,
                            UniqueSongs = monthStat?.UniqueSongs ?? 0,
                            TopSongs = new List<TopSongDto>()
                        });

                        currentDate = currentDate.AddMonths(1);
                    }
                }

                // Get overall top songs by play count
                var overallTopSongs = await _context.Songs
                    .Include(s => s.Artist)
                    .Where(s => s.ApprovalStatus == ApprovalStatus.Approved)
                    .OrderByDescending(s => s.PlayCount)
                    .ThenByDescending(s => s.LikeCount)
                    .Take(10)
                    .Select(s => new TopSongDto
                    {
                        SongId = s.SongId,
                        SongTitle = s.SongTitle,
                        ArtistName = s.Artist.ArtistName,
                        PlayCount = s.PlayCount,
                        LikeCount = s.LikeCount
                    })
                    .ToListAsync();

                // Get overall top songs by like count
                var overallTopLikedSongs = await _context.Songs
                    .Include(s => s.Artist)
                    .Where(s => s.ApprovalStatus == ApprovalStatus.Approved)
                    .OrderByDescending(s => s.LikeCount)
                    .ThenByDescending(s => s.PlayCount)
                    .Take(10)
                    .Select(s => new TopSongDto
                    {
                        SongId = s.SongId,
                        SongTitle = s.SongTitle,
                        ArtistName = s.Artist.ArtistName,
                        PlayCount = s.PlayCount,
                        LikeCount = s.LikeCount
                    })
                    .ToListAsync();

                return Ok(new TrendsResponse
                {
                    Trends = trends,
                    TopSongsByPlays = overallTopSongs,
                    TopSongsByLikes = overallTopLikedSongs,
                    Period = period,
                    Days = days
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trends");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
    }

    // DTOs for Statistics
    public class StatisticsOverviewDto
    {
        public int TotalUsers { get; set; }
        public int TotalArtists { get; set; }
        public int TotalSongs { get; set; }
        public int TotalAlbums { get; set; }
        public int ApprovedSongs { get; set; }
        public int PendingSongs { get; set; }
        public int RejectedSongs { get; set; }
    }

    public class GenreStatisticsDto
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; } = null!;
        public int SongCount { get; set; }
        public long TotalPlayCount { get; set; }
        public int TotalLikeCount { get; set; }
    }

    public class ListeningHistoryDto
    {
        public int HistoryId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public int SongId { get; set; }
        public string SongTitle { get; set; } = null!;
        public string ArtistName { get; set; } = null!;
        public DateTime PlayedAt { get; set; }
        public int? DurationPlayed { get; set; }
        public bool Completed { get; set; }
    }

    public class ListeningHistoryResponse
    {
        public List<ListeningHistoryDto> History { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class StatisticsUserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Country { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class UsersListResponse
    {
        public List<StatisticsUserDto> Users { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class StatisticsArtistDto
    {
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = null!;
        public string? Biography { get; set; }
        public string? Country { get; set; }
        public bool Verified { get; set; }
        public int MonthlyListeners { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public int SongCount { get; set; }
        public int AlbumCount { get; set; }
    }

    public class ArtistsListResponse
    {
        public List<StatisticsArtistDto> Artists { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class StatisticsSongDto
    {
        public int SongId { get; set; }
        public string SongTitle { get; set; } = null!;
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = null!;
        public int? AlbumId { get; set; }
        public string? AlbumTitle { get; set; }
        public int? GenreId { get; set; }
        public string? GenreName { get; set; }
        public int DurationSeconds { get; set; }
        public long PlayCount { get; set; }
        public int LikeCount { get; set; }
        public string ApprovalStatus { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }

    public class SongsListResponse
    {
        public List<StatisticsSongDto> Songs { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class StatisticsAlbumDto
    {
        public int AlbumId { get; set; }
        public string AlbumTitle { get; set; } = null!;
        public int ArtistId { get; set; }
        public string ArtistName { get; set; } = null!;
        public string AlbumType { get; set; } = null!;
        public int TotalTracks { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AlbumsListResponse
    {
        public List<StatisticsAlbumDto> Albums { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class TrendDataDto
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = null!;
        public int PlayCount { get; set; }
        public int UniqueSongs { get; set; }
        public List<TopSongDto> TopSongs { get; set; } = [];
    }

    public class TopSongDto
    {
        public int SongId { get; set; }
        public string SongTitle { get; set; } = null!;
        public string ArtistName { get; set; } = null!;
        public long PlayCount { get; set; }
        public int LikeCount { get; set; }
    }

    public class TrendsResponse
    {
        public List<TrendDataDto> Trends { get; set; } = [];
        public List<TopSongDto> TopSongsByPlays { get; set; } = [];
        public List<TopSongDto> TopSongsByLikes { get; set; } = [];
        public string Period { get; set; } = null!;
        public int Days { get; set; }
    }
}

