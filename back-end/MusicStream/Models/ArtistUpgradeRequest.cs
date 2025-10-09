namespace MusicStream.Models
{
    public class ArtistUpgradeRequest
    {
        public int RequestId { get; set; }
        public int UserId { get; set; }
        public string ArtistName { get; set; } = null!;
        public string? Biography { get; set; }
        public string ApprovalReason { get; set; } = null!; // Lý do xin duyệt
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByUserId { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual User? ReviewedByUser { get; set; }
    }
}

