namespace MusicStream.Models
{
    /// <summary>
    /// Model để track user nào đã like bài hát nào
    /// Tránh việc user like nhiều lần
    /// </summary>
    public class UserLike
    {
        public int UserId { get; set; }
        public int SongId { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Song Song { get; set; } = null!;
    }
}

