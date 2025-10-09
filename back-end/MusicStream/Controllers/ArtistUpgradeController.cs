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
    public class ArtistUpgradeController : ControllerBase
    {
        private readonly MusicStreamContext _context;
        private readonly ILogger<ArtistUpgradeController> _logger;

        public ArtistUpgradeController(
            MusicStreamContext context,
            ILogger<ArtistUpgradeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Submit upgrade request (for users)
        [HttpPost("submit")]
        public async Task<ActionResult> SubmitUpgradeRequest([FromBody] SubmitUpgradeRequestRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var user = await _context.Users
                    .Include(u => u.Artist)
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                // Check if user is already an artist
                if (user.Artist != null)
                {
                    return BadRequest(new { message = "Bạn đã là nghệ sĩ rồi" });
                }

                // Check if user already has a pending request
                var existingRequest = await _context.ArtistUpgradeRequests
                    .FirstOrDefaultAsync(r => r.UserId == currentUserId && r.Status == ApprovalStatus.Pending);

                if (existingRequest != null)
                {
                    return BadRequest(new { message = "Bạn đã có một yêu cầu đang chờ duyệt" });
                }

                // Validate request
                if (string.IsNullOrWhiteSpace(request.ArtistName))
                {
                    return BadRequest(new { message = "Vui lòng nhập tên nghệ sĩ" });
                }

                if (string.IsNullOrWhiteSpace(request.ApprovalReason))
                {
                    return BadRequest(new { message = "Vui lòng nhập lý do xin duyệt" });
                }

                // Check if artist name already exists
                var existingArtist = await _context.Artists
                    .FirstOrDefaultAsync(a => a.ArtistName == request.ArtistName.Trim());

                if (existingArtist != null)
                {
                    return BadRequest(new { message = "Tên nghệ sĩ đã tồn tại" });
                }

                // Create upgrade request
                var upgradeRequest = new ArtistUpgradeRequest
                {
                    UserId = currentUserId,
                    ArtistName = request.ArtistName.Trim(),
                    Biography = request.Biography?.Trim(),
                    ApprovalReason = request.ApprovalReason.Trim(),
                    Status = ApprovalStatus.Pending,
                    CreatedAt = DateTime.Now
                };

                _context.ArtistUpgradeRequests.Add(upgradeRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} submitted upgrade request {RequestId}", currentUserId, upgradeRequest.RequestId);

                return Ok(new { message = "Gửi yêu cầu nâng cấp thành công", requestId = upgradeRequest.RequestId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting upgrade request");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // Get user's upgrade request status
        [HttpGet("my-request")]
        public async Task<ActionResult<UpgradeRequestResponse>> GetMyRequest()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var request = await _context.ArtistUpgradeRequests
                    .Include(r => r.User)
                    .Include(r => r.ReviewedByUser)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync(r => r.UserId == currentUserId);

                if (request == null)
                {
                    return NotFound(new { message = "Không tìm thấy yêu cầu" });
                }

                return Ok(new UpgradeRequestResponse
                {
                    RequestId = request.RequestId,
                    UserId = request.UserId,
                    UserName = request.User.Username,
                    UserEmail = request.User.Email,
                    UserFullName = request.User.FullName,
                    UserProfilePictureUrl = request.User.ProfilePictureUrl,
                    ArtistName = request.ArtistName,
                    Biography = request.Biography,
                    ApprovalReason = request.ApprovalReason,
                    Status = request.Status.ToString(),
                    CreatedAt = request.CreatedAt,
                    ReviewedAt = request.ReviewedAt,
                    ReviewedByUserId = request.ReviewedByUserId,
                    ReviewedByUserName = request.ReviewedByUser?.FullName ?? request.ReviewedByUser?.Username,
                    RejectionReason = request.RejectionReason
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user upgrade request");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // Get all pending requests (for admin)
        [HttpGet("pending")]
        [Authorize(Roles = "1")] // Only admin
        public async Task<ActionResult<UpgradeRequestListResponse>> GetPendingRequests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.ArtistUpgradeRequests
                    .Include(r => r.User)
                    .Include(r => r.ReviewedByUser)
                    .Where(r => r.Status == ApprovalStatus.Pending)
                    .OrderByDescending(r => r.CreatedAt);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var requests = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var requestsData = requests.Select(r => new UpgradeRequestResponse
                {
                    RequestId = r.RequestId,
                    UserId = r.UserId,
                    UserName = r.User.Username,
                    UserEmail = r.User.Email,
                    UserFullName = r.User.FullName,
                    UserProfilePictureUrl = r.User.ProfilePictureUrl,
                    ArtistName = r.ArtistName,
                    Biography = r.Biography,
                    ApprovalReason = r.ApprovalReason,
                    Status = r.Status.ToString(),
                    CreatedAt = r.CreatedAt,
                    ReviewedAt = r.ReviewedAt,
                    ReviewedByUserId = r.ReviewedByUserId,
                    ReviewedByUserName = r.ReviewedByUser?.FullName ?? r.ReviewedByUser?.Username,
                    RejectionReason = r.RejectionReason
                }).ToList();

                return Ok(new UpgradeRequestListResponse
                {
                    Requests = requestsData,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending upgrade requests");
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // Approve upgrade request (for admin)
        [HttpPost("{requestId}/approve")]
        [Authorize(Roles = "1")] // Only admin
        public async Task<ActionResult> ApproveRequest(int requestId)
        {
            try
            {
                var adminUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (adminUserIdClaim == null || !int.TryParse(adminUserIdClaim.Value, out int adminUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var request = await _context.ArtistUpgradeRequests
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.RequestId == requestId);

                if (request == null)
                {
                    return NotFound(new { message = "Không tìm thấy yêu cầu" });
                }

                if (request.Status != ApprovalStatus.Pending)
                {
                    return BadRequest(new { message = "Yêu cầu này đã được xử lý" });
                }

                // Check if user already has an artist profile
                var existingArtist = await _context.Artists
                    .FirstOrDefaultAsync(a => a.UserId == request.UserId);

                if (existingArtist != null)
                {
                    return BadRequest(new { message = "Người dùng này đã có hồ sơ nghệ sĩ" });
                }

                // Check if artist name is still available
                var artistNameExists = await _context.Artists
                    .AnyAsync(a => a.ArtistName == request.ArtistName);

                if (artistNameExists)
                {
                    return BadRequest(new { message = "Tên nghệ sĩ đã được sử dụng" });
                }

                // Create artist profile
                var artist = new Artist
                {
                    UserId = request.UserId,
                    ArtistName = request.ArtistName,
                    Biography = request.Biography,
                    Verified = false,
                    MonthlyListeners = 0
                };

                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();

                // Update request status
                request.Status = ApprovalStatus.Approved;
                request.ReviewedAt = DateTime.Now;
                request.ReviewedByUserId = adminUserId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin {AdminId} approved upgrade request {RequestId} for user {UserId}", 
                    adminUserId, requestId, request.UserId);

                return Ok(new { message = "Duyệt yêu cầu thành công", artistId = artist.ArtistId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving upgrade request {RequestId}", requestId);
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // Reject upgrade request (for admin)
        [HttpPost("{requestId}/reject")]
        [Authorize(Roles = "1")] // Only admin
        public async Task<ActionResult> RejectRequest(int requestId, [FromBody] RejectRequestRequest rejectRequest)
        {
            try
            {
                var adminUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (adminUserIdClaim == null || !int.TryParse(adminUserIdClaim.Value, out int adminUserId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng" });
                }

                var request = await _context.ArtistUpgradeRequests
                    .FirstOrDefaultAsync(r => r.RequestId == requestId);

                if (request == null)
                {
                    return NotFound(new { message = "Không tìm thấy yêu cầu" });
                }

                if (request.Status != ApprovalStatus.Pending)
                {
                    return BadRequest(new { message = "Yêu cầu này đã được xử lý" });
                }

                // Update request status
                request.Status = ApprovalStatus.Rejected;
                request.ReviewedAt = DateTime.Now;
                request.ReviewedByUserId = adminUserId;
                request.RejectionReason = rejectRequest.RejectionReason?.Trim();

                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin {AdminId} rejected upgrade request {RequestId}. Reason: {Reason}", 
                    adminUserId, requestId, rejectRequest.RejectionReason);

                return Ok(new { message = "Từ chối yêu cầu thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting upgrade request {RequestId}", requestId);
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // Request DTOs
        public class SubmitUpgradeRequestRequest
        {
            public string ArtistName { get; set; } = null!;
            public string? Biography { get; set; }
            public string ApprovalReason { get; set; } = null!;
        }

        public class RejectRequestRequest
        {
            public string? RejectionReason { get; set; }
        }

        public class UpgradeRequestResponse
        {
            public int RequestId { get; set; }
            public int UserId { get; set; }
            public string? UserName { get; set; }
            public string? UserEmail { get; set; }
            public string? UserFullName { get; set; }
            public string? UserProfilePictureUrl { get; set; }
            public string ArtistName { get; set; } = null!;
            public string? Biography { get; set; }
            public string ApprovalReason { get; set; } = null!;
            public string Status { get; set; } = null!;
            public DateTime CreatedAt { get; set; }
            public DateTime? ReviewedAt { get; set; }
            public int? ReviewedByUserId { get; set; }
            public string? ReviewedByUserName { get; set; }
            public string? RejectionReason { get; set; }
        }

        public class UpgradeRequestListResponse
        {
            public List<UpgradeRequestResponse> Requests { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }
    }
}

