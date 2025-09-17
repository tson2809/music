namespace MusicStream.Models
{
    /// <summary>
    /// Trạng thái duyệt bài hát
    /// </summary>
    public enum ApprovalStatus
    {
        /// <summary>Chờ duyệt</summary>
        Pending = 0,
        
        /// <summary>Đã duyệt</summary>
        Approved = 1,
        
        /// <summary>Bị từ chối</summary>
        Rejected = 2
    }
}

