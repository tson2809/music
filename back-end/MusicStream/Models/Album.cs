namespace MusicStream.Models
{
    public class Album
    {
        public int AlbumId { get; set; }
        public string AlbumTitle { get; set; } = null!;
        public int ArtistId { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string AlbumType { get; set; } = "album"; // single, EP, album, compilation
        public string? CoverImageUrl { get; set; }
        public int TotalTracks { get; set; } = 0;
        public int DurationSeconds { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Artist Artist { get; set; } = null!;
        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}

