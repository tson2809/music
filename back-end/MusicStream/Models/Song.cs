namespace MusicStream.Models
{
    public class Song
    {
        public int SongId { get; set; }
        public string SongTitle { get; set; } = null!;
        public int ArtistId { get; set; }
        public int? AlbumId { get; set; }
        public int? GenreId { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string AudioFileUrl { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public string? Lyrics { get; set; }
        public long PlayCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Approval system
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedByUserId { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation properties
        public virtual Artist Artist { get; set; } = null!;
        public virtual Album? Album { get; set; }
        public virtual Genre? Genre { get; set; }
        public virtual User? ApprovedByUser { get; set; }
        public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
        public virtual ICollection<UserLike> UserLikes { get; set; } = new List<UserLike>();
        public virtual ICollection<ListeningHistory> ListeningHistories { get; set; } = new List<ListeningHistory>();
        public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
    }
}

