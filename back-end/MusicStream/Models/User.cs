namespace MusicStream.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Country { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Role Role { get; set; } = null!;
        public virtual Artist? Artist { get; set; }
        public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
        public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
        public virtual ICollection<UserLike> UserLikes { get; set; } = new List<UserLike>();
        public virtual ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();
    }
}

