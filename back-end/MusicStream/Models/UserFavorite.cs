namespace MusicStream.Models
{
    public class UserFavorite
    {
        public int UserId { get; set; }
        public int SongId { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Song Song { get; set; } = null!;
    }
}

