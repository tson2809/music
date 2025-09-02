namespace MusicStream.Models
{
    public class Genre
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; } = null!;
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
        public virtual ICollection<Artist> Artists { get; set; } = new List<Artist>();
    }
}

