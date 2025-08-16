using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MusicStream.Data;
using MusicStream.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.IO;
using System.Linq;

namespace MusicStream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MusicStreamContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public AuthController(MusicStreamContext context, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _context = context;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Tìm user theo username
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Artist)
                    .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

                if (user == null)
                {
                    return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng" });
                }

                if (user.PasswordHash != request.Password)
                {
                    return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng" });
                }

                // Tạo JWT token
                var token = GenerateJwtToken(user);

                var response = new LoginResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        DateOfBirth = user.DateOfBirth,
                        Country = user.Country,
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        RoleId = user.RoleId,
                        IsActive = user.IsActive,
                        ArtistId = user.Artist?.ArtistId
                    },
                    Message = "Đăng nhập thành công"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Kiểm tra username đã tồn tại
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                {
                    return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });
                }

                // Kiểm tra email đã tồn tại
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new { message = "Email đã được sử dụng" });
                }

                var newUser = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = request.Password,
                    FullName = request.FullName,
                    RoleId = 2, // Default role
                    IsActive = true
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Load role để trả về
                await _context.Entry(newUser).Reference(u => u.Role).LoadAsync();

                // Tạo token
                var token = GenerateJwtToken(newUser);

                var response = new LoginResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        UserId = newUser.UserId,
                        Username = newUser.Username,
                        Email = newUser.Email,
                        FullName = newUser.FullName,
                        DateOfBirth = newUser.DateOfBirth,
                        Country = newUser.Country,
                        ProfilePictureUrl = newUser.ProfilePictureUrl,
                        RoleId = newUser.RoleId,
                        IsActive = newUser.IsActive
                    },
                    Message = "Đăng ký thành công"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPost("check-username")]
        public async Task<ActionResult> CheckUsername([FromBody] CheckUsernameRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

                return Ok(new { exists = user != null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

                if (user == null)
                {
                    return BadRequest(new { message = "Tên đăng nhập không tồn tại" });
                }

                // Cập nhật mật khẩu
                user.PasswordHash = request.NewPassword;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpGet("profile/{userId}")]
        public async Task<ActionResult<UserDto>> GetProfile(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Artist)
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    DateOfBirth = user.DateOfBirth,
                    Country = user.Country,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    RoleId = user.RoleId,
                    IsActive = user.IsActive,
                    ArtistId = user.Artist?.ArtistId
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPut("profile/{userId}")]
        public async Task<ActionResult<UserDto>> UpdateProfile(int userId, [FromBody] UpdateProfileRequest request)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Artist)
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }

                // Cập nhật thông tin
                user.FullName = request.FullName ?? user.FullName;
                user.Email = request.Email ?? user.Email;
                user.DateOfBirth = request.DateOfBirth ?? user.DateOfBirth;
                user.Country = request.Country ?? user.Country;
                user.ProfilePictureUrl = request.ProfilePictureUrl ?? user.ProfilePictureUrl;

                await _context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    DateOfBirth = user.DateOfBirth,
                    Country = user.Country,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    RoleId = user.RoleId,
                    IsActive = user.IsActive,
                    ArtistId = user.Artist?.ArtistId
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        [HttpPost("profile/{userId}/avatar")]
        public async Task<ActionResult<UserDto>> UploadAvatar(int userId, IFormFile? avatar)
        {
            try
            {
                if (avatar == null || avatar.Length == 0)
                {
                    return BadRequest(new { message = "Vui lòng chọn ảnh hợp lệ" });
                }

                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (avatar.Length > maxFileSize)
                {
                    return BadRequest(new { message = "Ảnh quá lớn (tối đa 5MB)" });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Định dạng ảnh không hỗ trợ" });
                }

                var user = await _context.Users
                    .Include(u => u.Artist)
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }

                var avatarFolder = Path.Combine(_environment.ContentRootPath, "images", "avatar");
                Directory.CreateDirectory(avatarFolder);

                var fileName = $"user_{userId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
                var filePath = Path.Combine(avatarFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && user.ProfilePictureUrl.StartsWith("/images/avatar"))
                {
                    var oldPath = Path.Combine(_environment.ContentRootPath, user.ProfilePictureUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                user.ProfilePictureUrl = $"/images/avatar/{fileName}";
                await _context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    DateOfBirth = user.DateOfBirth,
                    Country = user.Country,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    RoleId = user.RoleId,
                    IsActive = user.IsActive,
                    ArtistId = user.Artist?.ArtistId
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi khi tải ảnh: " + ex.Message });
            }
        }

        [HttpPost("change-password/{userId}")]
        public async Task<ActionResult> ChangePassword(int userId, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }

                if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 6)
                {
                    return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự" });
                }

                user.PasswordHash = request.NewPassword;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!@#$%";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "MusicStreamAPI",
                audience: jwtSettings["Audience"] ?? "MusicStreamClient",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // DTOs
    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? FullName { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public UserDto User { get; set; } = null!;
        public string Message { get; set; } = null!;
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Country { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public int? ArtistId { get; set; }
    }

    public class CheckUsernameRequest
    {
        public string Username { get; set; } = null!;
    }

    public class ResetPasswordRequest
    {
        public string Username { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Country { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string NewPassword { get; set; } = null!;
    }
}

