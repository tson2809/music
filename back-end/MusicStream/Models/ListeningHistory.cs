namespace MusicStream.Models
{
    public class ListeningHistory
    {
        public int HistoryId { get; set; }
        public int UserId { get; set; }
        public int SongId { get; set; }
        public DateTime PlayedAt { get; set; } = DateTime.Now;
        public int? DurationPlayed { get; set; }
        public bool Completed { get; set; } = false;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Song Song { get; set; } = null!;
    }
}

