namespace MusicStream.Models
{
    public class Artist
    {
        public int ArtistId { get; set; }
        public int? UserId { get; set; }
        public string ArtistName { get; set; } = null!;
        public string? Biography { get; set; }
        // ProfileImageUrl đã được xóa - lấy ảnh từ User.ProfilePictureUrl thay thế
        public bool Verified { get; set; } = false;
        public int MonthlyListeners { get; set; } = 0;

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ICollection<Album> Albums { get; set; } = new List<Album>();
        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
        public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
    }
}

