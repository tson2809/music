using Microsoft.EntityFrameworkCore;
using MusicStream.Models;

namespace MusicStream.Data
{
    public class MusicStreamContext : DbContext
    {
        public MusicStreamContext(DbContextOptions<MusicStreamContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<UserLike> UserLikes { get; set; }
        public DbSet<ListeningHistory> ListeningHistories { get; set; }
        public DbSet<ArtistUpgradeRequest> ArtistUpgradeRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleName).HasMaxLength(20).IsRequired();
                entity.HasIndex(e => e.RoleName).IsUnique();
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(50);
                entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
                entity.Property(e => e.RoleId).HasDefaultValue(1);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(e => e.RoleId);
            });

            // Genre
            modelBuilder.Entity<Genre>(entity =>
            {
                entity.ToTable("genres");
                entity.HasKey(e => e.GenreId);
                entity.Property(e => e.GenreName).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.GenreName).IsUnique();
            });

            // Artist
            modelBuilder.Entity<Artist>(entity =>
            {
                entity.ToTable("artists");
                entity.HasKey(e => e.ArtistId);
                entity.Property(e => e.ArtistName).HasMaxLength(100).IsRequired();
                // ProfileImageUrl đã được xóa - lấy ảnh từ User.ProfilePictureUrl thay thế
                entity.Property(e => e.Verified).HasDefaultValue(false);
                entity.Property(e => e.MonthlyListeners).HasDefaultValue(0);

                // Unique index chỉ cho user_id không NULL
                entity.HasIndex(e => e.UserId)
                    .IsUnique()
                    .HasFilter("[UserId] IS NOT NULL");

                entity.HasOne(e => e.User)
                    .WithOne(u => u.Artist)
                    .HasForeignKey<Artist>(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.Genres)
                    .WithMany(g => g.Artists)
                    .UsingEntity(
                        "artist_genres",
                        l => l.HasOne(typeof(Genre)).WithMany().HasForeignKey("genre_id"),
                        r => r.HasOne(typeof(Artist)).WithMany().HasForeignKey("artist_id"),
                        j => j.ToTable("artist_genres"));
            });

            // Album
            modelBuilder.Entity<Album>(entity =>
            {
                entity.ToTable("albums");
                entity.HasKey(e => e.AlbumId);
                entity.Property(e => e.AlbumTitle).HasMaxLength(200).IsRequired();
                entity.Property(e => e.AlbumType).HasMaxLength(20).HasDefaultValue("album");
                entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
                entity.Property(e => e.TotalTracks).HasDefaultValue(0);
                entity.Property(e => e.DurationSeconds).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Artist)
                    .WithMany(a => a.Albums)
                    .HasForeignKey(e => e.ArtistId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Song
            modelBuilder.Entity<Song>(entity =>
            {
                entity.ToTable("songs");
                entity.HasKey(e => e.SongId);
                entity.Property(e => e.SongTitle).HasMaxLength(200).IsRequired();
                entity.Property(e => e.AudioFileUrl).HasMaxLength(500).IsRequired();
                entity.Property(e => e.PlayCount).HasDefaultValue(0);
                entity.Property(e => e.LikeCount).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                
                // Approval system
                entity.Property(e => e.ApprovalStatus)
                    .HasConversion<int>()
                    .HasDefaultValue(ApprovalStatus.Pending);
                entity.Property(e => e.RejectionReason).HasMaxLength(500);

                entity.HasOne(e => e.Artist)
                    .WithMany(a => a.Songs)
                    .HasForeignKey(e => e.ArtistId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Album)
                    .WithMany(a => a.Songs)
                    .HasForeignKey(e => e.AlbumId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Genre)
                    .WithMany(g => g.Songs)
                    .HasForeignKey(e => e.GenreId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasMany(e => e.Playlists)
                    .WithMany(p => p.Songs)
                    .UsingEntity<PlaylistSong>(
                        j => j.HasOne(ps => ps.Playlist)
                            .WithMany(p => p.PlaylistSongs)
                            .HasForeignKey(ps => ps.PlaylistId)
                            .OnDelete(DeleteBehavior.NoAction),
                        j => j.HasOne(ps => ps.Song)
                            .WithMany()
                            .HasForeignKey(ps => ps.SongId)
                            .OnDelete(DeleteBehavior.NoAction),
                        j =>
                        {
                            j.ToTable("playlist_songs");
                            j.HasKey(ps => new { ps.PlaylistId, ps.SongId });
                            j.Property(ps => ps.AddedAt).HasDefaultValueSql("GETDATE()");
                        });
            });

            // Playlist
            modelBuilder.Entity<Playlist>(entity =>
            {
                entity.ToTable("playlists");
                entity.HasKey(e => e.PlaylistId);
                entity.Property(e => e.PlaylistName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
                entity.Property(e => e.IsPublic).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Playlists)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // UserFavorite
            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.ToTable("user_favorites");
                entity.HasKey(e => new { e.UserId, e.SongId });

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserFavorites)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Song)
                    .WithMany(s => s.UserFavorites)
                    .HasForeignKey(e => e.SongId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // UserLike - Track user nào đã like bài nào (để tránh like nhiều lần)
            modelBuilder.Entity<UserLike>(entity =>
            {
                entity.ToTable("user_likes");
                entity.HasKey(e => new { e.UserId, e.SongId });

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserLikes)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Song)
                    .WithMany(s => s.UserLikes)
                    .HasForeignKey(e => e.SongId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ListeningHistory
            modelBuilder.Entity<ListeningHistory>(entity =>
            {
                entity.ToTable("listening_history");
                entity.HasKey(e => e.HistoryId);
                entity.Property(e => e.PlayedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Completed).HasDefaultValue(false);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.ListeningHistories)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Song)
                    .WithMany(s => s.ListeningHistories)
                    .HasForeignKey(e => e.SongId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ArtistUpgradeRequest
            modelBuilder.Entity<ArtistUpgradeRequest>(entity =>
            {
                entity.ToTable("artist_upgrade_requests");
                entity.HasKey(e => e.RequestId);
                entity.Property(e => e.ArtistName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Biography).HasMaxLength(2000);
                entity.Property(e => e.ApprovalReason).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.RejectionReason).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Status)
                    .HasConversion<int>()
                    .HasDefaultValue(ApprovalStatus.Pending);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ReviewedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

