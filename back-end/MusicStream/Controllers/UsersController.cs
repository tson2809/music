using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStream.Data;

namespace MusicStream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "1")] // Only allow admins
    public class UsersController : ControllerBase
    {
        private readonly MusicStreamContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(MusicStreamContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<UserListResponse>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? roleId = null,
            [FromQuery] bool? isArtist = null,
            [FromQuery] bool? isActive = null)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest(new { message = "Page và pageSize phải lớn hơn 0" });
            }

            pageSize = Math.Min(pageSize, 100); // Avoid oversized queries

            try
            {
                var query = _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Artist)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var pattern = $"%{search.Trim()}%";
                    query = query.Where(u =>
                        EF.Functions.Like(u.Username, pattern) ||
                        EF.Functions.Like(u.Email, pattern) ||
                        (u.FullName != null && EF.Functions.Like(u.FullName, pattern)));
                }

                if (roleId.HasValue && roleId.Value > 0)
                {
                    query = query.Where(u => u.RoleId == roleId.Value);
                }

                if (isArtist.HasValue)
                {
                    if (isArtist.Value)
                    {
                        // Filter for users who are artists (have ArtistId)
                        query = query.Where(u => u.Artist != null);
                    }
                    else
                    {
                        // Filter for users who are not artists
                        // When filtering by roleId = 2 (User) and isArtist = false, 
                        // this means regular users (not admins, not artists)
                        query = query.Where(u => u.Artist == null);
                    }
                }

                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .OrderByDescending(u => u.UserId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserListItemDto
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        FullName = u.FullName,
                        DateOfBirth = u.DateOfBirth,
                        Country = u.Country,
                        ProfilePictureUrl = u.ProfilePictureUrl,
                        RoleId = u.RoleId,
                        RoleName = u.Role.RoleName,
                        IsActive = u.IsActive,
                        ArtistId = u.Artist != null ? u.Artist.ArtistId : (int?)null
                    })
                    .ToListAsync();

                return Ok(new UserListResponse
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
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPatch("{userId}/status")]
        public async Task<IActionResult> UpdateUserStatus(int userId, [FromBody] UpdateUserStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                user.IsActive = request.IsActive;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status for {UserId}", userId);
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPatch("{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateUserRoleRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == request.RoleId);
                if (!roleExists)
                {
                    return BadRequest(new { message = "Quyền không hợp lệ" });
                }

                user.RoleId = request.RoleId;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user role for {UserId}", userId);
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
    }

    public class UserListItemDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Country { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public bool IsActive { get; set; }
        public int? ArtistId { get; set; }
    }

    public class UserListResponse
    {
        public List<UserListItemDto> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UpdateUserStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class UpdateUserRoleRequest
    {
        public int RoleId { get; set; }
    }
}

