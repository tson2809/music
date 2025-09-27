namespace MusicStream.Models
{
    public class PlaylistSong
    {
        public int PlaylistId { get; set; }
        public int SongId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;
        public int Position { get; set; }

        // Navigation properties
        public virtual Playlist Playlist { get; set; } = null!;
        public virtual Song Song { get; set; } = null!;
    }
}

