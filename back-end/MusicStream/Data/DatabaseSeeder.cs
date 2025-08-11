using MusicStream.Models;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace MusicStream.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedData(MusicStreamContext context)
        {
            // Seed Roles
            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { RoleName = "Admin" },
                    new Role { RoleName = "User" }
                };

                context.Roles.AddRange(roles);
                context.SaveChanges();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                var userRole = context.Roles.FirstOrDefault(r => r.RoleName == "User");

                var users = new List<User>
                {
                    // Admin
                    new User
                    {
                        Username = "admin",
                        Email = "admin@musicstream.com",
                        PasswordHash = "admin123",
                        FullName = "Admin",
                        DateOfBirth = new DateTime(1990, 1, 15),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/admin.png",
                        RoleId = adminRole?.RoleId ?? 1,
                        IsActive = true
                    },
                    // Users
                    new User
                    {
                        Username = "sontung",
                        Email = "sontung@gmail.com",
                        PasswordHash = "123456",
                        FullName = "Nguyễn Thanh Tùng",
                        DateOfBirth = new DateTime(1995, 3, 20),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_sontrung.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "denvau",
                        Email = "denvau@gmail.com",
                        PasswordHash = "123456",
                        FullName = "Nguyễn Đức Cường",
                        DateOfBirth = new DateTime(1998, 7, 12),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_den.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "pvc",
                        Email = "pvc@gmail.com",
                        PasswordHash = "123456",
                        FullName = "Phạm Văn C",
                        DateOfBirth = new DateTime(1992, 11, 8),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_hoa_minzy.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "tuan",
                        Email = "tuan@gmail.com",
                        PasswordHash = "123456",
                        FullName = "Trịnh Trần Phương Tuấn",
                        DateOfBirth = new DateTime(2000, 5, 25),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_jack.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "dve",
                        Email = "dve@gmail.com",
                        PasswordHash = "123456",
                        FullName = "Đỗ Văn E",
                        DateOfBirth = new DateTime(1997, 9, 30),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_1.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "min",
                        Email = "min@gmail.com",
                        PasswordHash = "123456",
                        FullName = "Nguyễn Minh Hằng",
                        DateOfBirth = new DateTime(1999, 2, 14),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_min.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "tvg",
                        Email = "tvg@gmail.com",
                        PasswordHash = "123456",
                        FullName = "Trần Văn G",
                        DateOfBirth = new DateTime(1994, 12, 3),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_h_kay.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "juky",
                        Email = "juky@gmail.com",
                        PasswordHash = "123456",
                        FullName = "Trần Thị Dung",
                        DateOfBirth = new DateTime(1996, 6, 18),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_juky_san.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "lvi",
                        Email = "lvi@gmail.com",
                        PasswordHash = "123456",
                        FullName = "Lý Văn I",
                        DateOfBirth = new DateTime(1993, 4, 22),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_2.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "son",
                        Email = "son@gmail.com",
                        PasswordHash = "123456",
                        FullName = "Đặng Thái Sơn",
                        DateOfBirth = new DateTime(2001, 8, 10),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/meo.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    // Thêm user mới (không phải artist)
                    new User
                    {
                        Username = "user1",
                        Email = "user1@gmail.com",
                        PasswordHash = "123456",
                        FullName = "NUser 1",
                        DateOfBirth = new DateTime(1995, 6, 15),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_3.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "user2",
                        Email = "user2@gmail.com",
                        PasswordHash = "123456",
                        FullName = "User 2",
                        DateOfBirth = new DateTime(1998, 9, 22),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_4.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    },
                    new User
                    {
                        Username = "user3",
                        Email = "user3@gmail.com",
                        PasswordHash = "123456",
                        FullName = "User 3",
                        DateOfBirth = new DateTime(1996, 4, 8),
                        Country = "Việt Nam",
                        ProfilePictureUrl = "images/avatar/avt_3.jpg",
                        RoleId = userRole?.RoleId ?? 2,
                        IsActive = true
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // Seed Genres
            if (!context.Genres.Any())
            {
                var genres = new List<Genre>
                {
                    new Genre { GenreName = "V-Pop", Description = "Nhạc Pop Việt Nam" },
                    new Genre { GenreName = "Ballad", Description = "Nhạc Ballad trữ tình" },
                    new Genre { GenreName = "Rock", Description = "Nhạc Rock" },
                    new Genre { GenreName = "Rap/Hip-hop", Description = "Nhạc Rap và Hip-hop Việt" },
                    new Genre { GenreName = "EDM", Description = "Electronic Dance Music" },
                    new Genre { GenreName = "Acoustic", Description = "Nhạc Acoustic nhẹ nhàng" },
                    new Genre { GenreName = "R&B", Description = "Rhythm and Blues" },
                    new Genre { GenreName = "Indie", Description = "Nhạc Indie độc lập" },
                    new Genre { GenreName = "Bolero", Description = "Nhạc Bolero truyền thống" },
                    new Genre { GenreName = "Chill", Description = "Nhạc Chill thư giãn" }
                };

                context.Genres.AddRange(genres);
                context.SaveChanges();
            }

            // Seed Artists
            if (!context.Artists.Any())
            {
                var artists = new List<Artist>
                {
                    new Artist 
                    { 
                        UserId = 2,
                        ArtistName = "Sơn Tùng M-TP", 
                        Biography = "Ca sĩ, nhạc sĩ, nhà sản xuất âm nhạc người Việt Nam",
                        Verified = true,
                        MonthlyListeners = 5000000
                    },
                    new Artist 
                    { 
                        UserId = 3,
                        ArtistName = "Đen Vâu", 
                        Biography = "Rapper, nhạc sĩ nổi tiếng với phong cách rap độc đáo",
                        Verified = true,
                        MonthlyListeners = 4500000
                    },
                    new Artist 
                    { 
                        UserId = 4,
                        ArtistName = "Hòa Minzy", 
                        Biography = "Giọng ca trẻ đầy triển vọng của V-Pop",
                        Verified = true,
                        MonthlyListeners = 2700000
                    },
                    new Artist 
                    { 
                        UserId = 5,
                        ArtistName = "Jack", 
                        Biography = "Ca sĩ trẻ với nhiều bản hit triệu view",
                        Verified = true,
                        MonthlyListeners = 4000000
                    },
                    new Artist
                    {
                        UserId = 7,
                        ArtistName = "Min",
                        Biography = "Giọng ca nữ với nhiều bản pop/dance nhẹ nhàng.",
                        Verified = true,
                        MonthlyListeners = 3200000
                    },
                    new Artist
                    {
                        UserId = 8,
                        ArtistName = "H-Kray",
                        Biography = "Rapper trẻ với phong cách hiện đại, giai điệu bắt tai.",
                        Verified = true,
                        MonthlyListeners = 1800000
                    },
                    new Artist
                    {
                        UserId = 9,
                        ArtistName = "Juky San",
                        Biography = "Ca sĩ Indie với chất giọng trong trẻo, nhiều bản acoustic nổi bật.",
                        Verified = true,
                        MonthlyListeners = 1500000
                    }
                };

                context.Artists.AddRange(artists);
                context.SaveChanges();
            }

            // Seed Albums
            if (!context.Albums.Any())
            {
                var albums = new List<Album>
                {
                    new Album 
                    { 
                        AlbumTitle = "Tùng Lúi", 
                        ArtistId = 1,
                        ReleaseDate = new DateTime(2017, 7, 1),
                        AlbumType = "album",
                        CoverImageUrl = "images/albums/sontung.jpg",
                        TotalTracks = 5,
                        DurationSeconds = 2400
                    },
                    new Album 
                    { 
                        AlbumTitle = "Đen Đen", 
                        ArtistId = 2,
                        ReleaseDate = new DateTime(2019, 5, 15),
                        AlbumType = "album",
                        CoverImageUrl = "images/albums/denvau.jpg",
                        TotalTracks = 2,
                        DurationSeconds = 3000
                    },
                    new Album 
                    { 
                        AlbumTitle = "Hòa minzy collection",
                        ArtistId = 3,
                        ReleaseDate = new DateTime(2022, 2, 22),
                        AlbumType = "album",
                        CoverImageUrl = "images/albums/hoaminzy.jpg",
                        TotalTracks = 2,
                        DurationSeconds = 2100
                    },
                    new Album
                    {
                        AlbumTitle = "Jack K-atm",
                        ArtistId = 4,
                        ReleaseDate = new DateTime(2022, 2, 22),
                        AlbumType = "album",
                        CoverImageUrl = "images/albums/jack.jpg",
                        TotalTracks = 1,
                        DurationSeconds = 2100
                    },
                };

                context.Albums.AddRange(albums);
                context.SaveChanges();
            }

            // Seed Songs
            if (!context.Songs.Any())
            {
                var genreLookup = context.Genres.ToDictionary(g => g.GenreName, g => g.GenreId);
                int GetGenreId(string genreName)
                {
                    if (genreLookup.TryGetValue(genreName, out var genreId))
                    {
                        return genreId;
                    }

                    return genreLookup.Values.FirstOrDefault();
                }

                var songs = new List<Song>
                {
                    new Song
                    {
                        SongTitle = "Chạy ngay đi",
                        ArtistId = 1,
                        AlbumId = 1,
                        GenreId = GetGenreId("V-Pop"),
                        DurationSeconds = 248,
                        ReleaseDate = new DateTime(2018, 5, 12),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/1fdce05c-f71a-43ed-ba31-645008a2b42f_CH%E1%BA%A0Y_NGAY_%C4%90I.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/72e382f2-f71b-42f6-b18d-c689596a7025_1526059033533_300.jpg",
                        Lyrics = """
                        [00:28.29]Good boy
                        [00:29.96]Từng phút cứ mãi trôi xa phai nhòa dần kí ức giữa đôi ta
                        [00:33.66]Từng chút nỗi nhớ hôm qua đâu về lạc bước cứ thế phôi pha
                        [00:37.42]Con tim giờ không cùng chung đôi nhịp
                        [00:39.19]Nụ cười lạnh băng còn đâu nồng ấm thân quen
                        [00:41.18]Vô tâm làm ngơ thờ ơ tương lai ai ngờ
                        [00:42.99]Quên đi mộng mơ ngày thơ tan theo sương mờ
                        [00:44.81]Mưa lặng thầm đường vắng chiều nay
                        [00:46.91]In giọt lệ nhòe khóe mắt sầu cay
                        [00:48.53]Bao hẹn thề tàn úa vụt bay
                        [00:50.70]Trôi dạt chìm vào những giấc nồng say
                        [00:52.06]Quay lưng chia hai lối, còn một mình anh thôi
                        [00:54.63]Giả dối bao trùm bỗng chốc lên ngôi
                        [00:56.03]Trong đêm tối bầu bạn cùng đơn côi
                        [00:57.75]Suy tư anh kìm nén đã bốc cháy yêu thương trao em rồi
                        [01:00.00]Đốt sạch hết
                        [01:00.81]Son môi hồng vương trên môi bấy lâu
                        [01:02.85]Hương thơm dịu êm mê man bấy lâu (đốt sạch hết)
                        [01:04.50]Anh không chờ mong quan tâm nữa đâu
                        [01:06.04]Tương lai từ giờ như bức tranh em quên tô màu (đốt sạch hết)
                        [01:08.33]Xin chôn vùi tên em trong đớn đau
                        [01:10.38]Nơi hiu quạnh tan hoang ngàn nỗi đau (đốt sạch hết)
                        [01:12.11]Dư âm tàn tro vô vọng phía sau
                        [01:13.74]Đua chen dày vò xâu xé quanh thân xác nát nhàu
                        [01:15.23]Chạy ngay đi, trước khi
                        [01:17.44]Mọi điều dần tồi tệ hơn
                        [01:18.91]Chạy ngay đi, trước khi
                        [01:21.24]Lòng hận thù cuộn từng cơn
                        [01:22.79]Tựa giông tố đến bên ghé thăm
                        [01:24.91]Từ nơi hố sâu tối tăm
                        [01:26.99]Chạy đi, trước khi
                        [01:28.68]Mọi điều dần tồi tệ hơn
                        [01:30.65]Không còn ai cạnh bên em ngày mai
                        [01:32.15]Tạm biệt một tương lai ngang trái
                        [01:34.49]Không còn ai cạnh bên em ngày mai
                        [01:35.94]Tạm biệt một tương lai ngang trái
                        [01:38.21]Không còn ai cạnh bên em ngày mai
                        [01:39.69]Tạm biệt một tương lai ngang trái
                        [01:41.95]Không còn ai cạnh bên em ngày mai
                        [01:43.62]Tạm biệt một tương lai ngang trái
                        [01:45.22]Yeah, buông bàn tay
                        [01:47.94]Buông xuôi hi vọng buông bình yên (buông)
                        [01:50.48]Đâu còn nguyên tháng ngày rực rỡ phai úa hằn sâu triền miên
                        [01:53.35]Vết thương cứ thêm, khắc thêm, mãi thêm
                        [01:54.78]Chà đạp vùi dập dẫm lên tiếng yêu ấm êm
                        [01:56.44]Nhìn lại niềm tin từng trao giờ sao
                        [01:58.15]Sau bao ngu muội sai lầm anh vẫn yếu mềm
                        [02:00.99]Căn phòng giam cầm thiêu linh hồn cô độc em trơ trọi kêu gào xót xa
                        [02:04.79]Căm hận tuôn trào dâng lên nhuộm đen ghì đôi vai đừng mong chờ thứ tha
                        [02:07.86](Ah, chính em gây ra mà
                        [02:09.84]Những điều vừa diễn ra
                        [02:11.31]Chính em gây ra mà, chính em gây ra mà
                        [02:13.63]Những điều vừa diễn ra
                        [02:15.06]Hết thật rồi)
                        [02:15.45]Đốt sạch hết
                        [02:16.56]Son môi hồng vương trên môi bấy lâu
                        [02:18.41]Hương thơm dịu êm mê man bấy lâu (đốt sạch hết)
                        [02:20.17]Anh không chờ mong quan tâm nữa đâu
                        [02:21.51]Tương lai từ giờ như bức tranh em quên tô màu (đốt sạch hết)
                        [02:24.09]Xin chôn vùi tên em trong đớn đau
                        [02:25.97]Nơi hiu quạnh tan hoang ngàn nỗi đau (đốt sạch hết)
                        [02:27.78]Dư âm tàn tro vô vọng phía sau
                        [02:29.17]Đua chen dày vò xâu xé quanh thân xác nát nhàu
                        [02:30.82]Chạy ngay đi, trước khi
                        [02:32.83]Mọi điều dần tồi tệ hơn
                        [02:34.58]Chạy ngay đi, trước khi
                        [02:36.76]Lòng hận thù cuộn từng cơn
                        [02:38.54]Tựa giông tố đến bên ghé thăm
                        [02:40.57]Từ nơi hố sâu tối tăm
                        [02:42.52]Chạy đi, trước khi
                        [02:44.26]Mọi điều dần tồi tệ hơn
                        [02:46.18]Không còn ai cạnh bên em ngày mai
                        [02:47.87]Tạm biệt một tương lai ngang trái
                        [02:50.06]Không còn ai cạnh bên em ngày mai
                        [02:51.44]Tạm biệt một tương lai ngang trái
                        [02:53.65]Không còn ai cạnh bên em ngày mai
                        [02:55.21]Tạm biệt một tương lai ngang trái
                        [02:57.58]Không còn ai cạnh bên em ngày mai
                        [02:59.05]Tạm biệt một tương lai ngang trái
                        [03:00.99]Đốt sạch hết
                        [03:02.43]Ohhh...
                        [03:06.98](Chính em gây ra mà, chính em gây ra mà)
                        [03:08.32]Đốt sạch hết
                        [03:10.16]Ohhh...
                        [03:14.65]Haizzz...
                        [03:16.17]Đừng nhìn anh với khuôn mặt xa lạ, xin
                        [03:19.02]Đừng lang thang trong tâm trí anh từng đêm nữa
                        [03:23.36]Quên đi, quên đi hết đi
                        [03:25.34]Quên đi, quên đi hết đi
                        [03:27.83]Thắp lên điều đáng thương lạnh giá ôm trọn giấc mơ vụn vỡ
                        [03:30.75]Bốc cháy lên cơn hận thù trong anh (quên đi, quên đi, quên đi hết đi)
                        [03:33.30]Cơn hận thù trong anh
                        [03:34.40]Bốc cháy lên cơn hận thù trong anh
                        [03:36.41]Ai khơi dậy cơn hận thù trong anh?
                        [03:38.33]Bốc cháy lên cơn hận thù trong anh (quên đi, quên đi, quên đi hết đi)
                        [03:41.04]Cơn hận thù trong anh
                        [03:42.19]Bốc cháy lên cơn hận thù trong anh
                        [03:44.09]Ai khơi dậy cơn hận thù trong anh? (ai cô đơn rồi)
                        [03:46.78]Không còn ai cạnh bên em ngày mai
                        [03:48.25]Tạm biệt một tương lai ngang trái (ai cô đơn rồi)
                        [03:50.30]Không còn ai cạnh bên em ngày mai
                        [03:51.94]Tạm biệt một tương lai ngang trái (ai cô đơn rồi)
                        [03:54.34]Không còn ai cạnh bên em ngày mai
                        [03:55.77]Tạm biệt một tương lai ngang trái (ai cô đơn rồi)
                        [03:58.09]Không còn ai cạnh bên em ngày mai
                        [03:59.49]Tạm biệt một tương lai ngang trái
                        [04:00.95]
                        """,
                        PlayCount = 36,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Hãy trao cho anh",
                        ArtistId = 1,
                        AlbumId = 1,
                        GenreId = GetGenreId("EDM"),
                        DurationSeconds = 245,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/8fc74ca9-2833-4a90-b7b5-d44925932ea8_H%C3%A3y_Trao_Cho_Anh.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/daf99ba0-c477-4d41-a0f3-7f8dfba5291f_1562137543919_300.jpg",
                        Lyrics = """
                        [00:01.29]
                        [00:10.36] La-la-la-la-la-la-la-la-la-la-la-la (Yeah, yeah)
                        [00:12.71] La-la-la-la-la-la-la-la-la-la-la-la (Yeah, yeah)
                        [00:15.27] La-la-la-la-la-la-la-la-la-la-la-la, la (Yeah, yeah)
                        [00:17.89] (Good boy)
                        [00:19.82] Hình bóng ai đó nhẹ nhàng vụt qua nơi đây
                        [00:21.85] Quyến rũ ngây ngất loạn nhịp làm tim mê say
                        [00:24.33] Cuốn lấy áng mây theo cơn sóng xô dập dìu
                        [00:26.74] Nụ cười ngọt ngào cho ta tan vào phút giây miên man quên hết con đường về eh
                        [00:30.33] (Let me know your name)
                        [00:31.35] Chẳng thể tìm thấy lối về eh
                        [00:32.95] (Let me know your name)
                        [00:33.89] Điệu nhạc hòa quyện trong ánh mắt đôi môi
                        [00:36.29] Dẫn lối những bối rối rung động khẽ lên ngôi
                        [00:38.85] (Và rồi khẽ, và rồi khẽ khẽ)
                        [00:39.53] Chạm nhau mang vô vàn
                        [00:40.31] Đắm đuối vấn vương dâng tràn
                        [00:41.52] Lấp kín chốn nhân gian
                        [00:42.50] Làn gió hoá sắc hương mơ màng
                        [00:44.22] Một giây ngang qua đời
                        [00:45.28] Cất tiếng nói không nên lời
                        [00:46.53] Ấm áp đến trao tay ngàn sao trời lòng càng thêm chơi vơi
                        [00:49.38] Dịu êm không gian bừng sáng
                        [00:50.54] Đánh thức muôn hoa mừng
                        [00:51.60] Quấn quít hát ngân nga từng chút níu bước chân em dừng
                        [00:54.09] Bao ý thơ tương tư ngẩn ngơ
                        [00:56.56] Lưu dấu nơi mê cung đẹp thẫn thờ
                        [01:00.42] Hãy trao cho anh
                        [01:01.97] Hãy trao cho anh
                        [01:03.07] Hãy trao cho anh thứ anh đang mong chờ
                        [01:05.54] Hãy trao cho anh
                        [01:06.84] Hãy trao cho anh
                        [01:08.06] Hãy mau làm điều ta muốn vào khoảnh khắc này đê
                        [01:10.55] Hãy trao cho anh
                        [01:11.77] Hãy trao cho anh
                        [01:13.11] Hãy trao anh trao cho anh đi những yêu thương nồng cháy
                        [01:16.12] Trao anh ái ân nguyên vẹn đong đầy
                        [01:18.88] La-la, la-la-la-la-la
                        [01:23.66] La-la, la-la-la-la-la
                        [01:29.10] La-la, la-la-la-la-la
                        [01:33.75] La-la, la-la-la-la-la
                        [01:40.15] Looking at my Gucci is about that time
                        [01:42.60] We can smoke a blunt and pop a bottle of wine
                        [01:44.97] Now get yourself together and be ready by nine
                        [01:47.43] Cuz we gon' do some things that will shatter your spine
                        [01:49.78] Come one, undone, Snoop Dogg, Son Tung
                        [01:53.36] Long Beach is the city that I come from
                        [01:55.84] So if you want some, get some
                        [01:57.56] Better enough take some, take some
                        [01:59.49] Chạm nhau mang vô vàn
                        [02:00.37] Đắm đuối vấn vương dâng tràn
                        [02:01.55] Lấp kín chốn nhân gian làn
                        [02:02.54] Gió hóa sắc hương mơ màng
                        [02:04.31] Một giây ngang qua đời
                        [02:05.33] Cất tiếng nói không nên lời
                        [02:06.56] Ấm áp đến trao tay ngàn sao trời lòng càng thêm chơi vơi
                        [02:09.46] Dịu êm không gian bừng sáng
                        [02:10.51] Đánh thức muôn hoa mừng
                        [02:11.59] Quấn quít hát ngân nga từng chút níu bước chân em dừng
                        [02:14.13] Bao ý thơ tương tư ngẩn ngơ
                        [02:16.63] Lưu dấu nơi mê cung đẹp thẫn thờ
                        [02:20.35] Hãy trao cho anh
                        [02:21.82] Hãy trao cho anh
                        [02:23.06] Hãy trao cho anh thứ anh đang mong chờ
                        [02:25.36] Hãy trao cho anh
                        [02:26.86] Hãy trao cho anh
                        [02:28.17] Hãy mau làm điều ta muốn vào khoảnh khắc này đê
                        [02:30.51] Hãy trao cho anh
                        [02:32.07] Hãy trao cho anh
                        [02:33.17] Hãy trao anh trao cho anh đi những yêu thương nồng cháy
                        [02:36.05] Trao anh ái ân nguyên vẹn đong đầy
                        [02:38.82] La-la, la-la-la-la-la
                        [02:43.78] La-la, la-la-la-la-la
                        [02:49.10] La-la, la-la-la-la-la
                        [02:53.62] La-la, la-la-la-la-la
                        [02:59.53] Em cho ta ngắm thiên đàng vội vàng qua chốc lát
                        [03:01.94] Như thanh âm chứa bao lời gọi mời trong khúc hát
                        [03:04.52] Liêu xiêu ta xuyến xao rạo rực khát khao trông mong
                        [03:06.70] Dịu dàng lại gần nhau hơn dang tay ôm em vào lòng
                        [03:09.46] Trao đi trao hết đi đừng ngập ngừng che dấu nữa
                        [03:11.93] Quên đi quên hết đi ngại ngùng lại gần thêm chút nữa
                        [03:14.47] Chìm đắm giữa khung trời riêng hai ta như dần hòa quyện mắt nhắm mắt tay đan tay hồn lạc về miền trăng sao
                        [03:19.39] Em cho ta ngắm thiên đàng vội vàng qua chốc lát
                        [03:21.86] Như thanh âm chứa bao lời gọi mời trong khúc hát
                        [03:24.30] Liêu xiêu ta xuyến xao rạo rực khát khao trông mong
                        [03:26.76] Dịu dàng lại gần nhau hơn dang tay ôm em vào lòng
                        [03:29.32] Trao đi trao hết đi đừng ngập ngừng che dấu nữa
                        [03:31.97] Quên đi quên hết đi ngại ngùng lại gần thêm chút nữa
                        [03:34.40] Chìm đắm giữa khung trời riêng hai ta như dần hòa quyện mắt nhắm mắt tay đan tay hồn lạc về miền trăng sao
                        [03:41.02] Hãy trao cho anh
                        [03:42.03] Hãy trao cho anh
                        [03:43.08] Hãy trao cho anh
                        [03:44.17] Cho anh, cho anh
                        [03:45.81] Hãy trao cho anh
                        [03:47.01] Hãy trao cho anh
                        [03:48.11] Hãy trao cho anh
                        [03:49.23] Cho anh, cho anh, cho anh
                        [03:50.93] Hãy trao cho anh
                        [03:52.03] Hãy trao cho anh
                        [03:53.27] Hãy trao cho anh
                        [03:54.25] Cho anh, cho anh
                        [03:55.89] Hãy trao cho anh
                        [03:57.00] Hãy trao cho anh
                        [03:58.30] Hãy trao cho anh thứ anh đang mong chờ
                        """,
                        PlayCount = 63,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Lạc trôi",
                        ArtistId = 1,
                        AlbumId = 1,
                        GenreId = GetGenreId("Indie"),
                        DurationSeconds = 233,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/e6040aed-737e-498a-be81-d0eb3eb8d133_L%E1%BA%A1c_Tr%C3%B4i.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/8d824b66-c183-4c31-a26e-d592490d6b74_1710498563935_300.jpg",
                        Lyrics = """
                        [00:14.20]Ah, ah, ah, la-ah-ah, la-ah-ah
                        [00:20.28]La-ah-ah, la-ah-ah
                        [00:23.81]La-ah-ah, la-ah-ah
                        [00:27.37]
                        [00:27.67]Người theo hương hoa, mây mù giăng lối
                        [00:30.98]Làn sương khói phôi phai đưa bước ai xa rồi
                        [00:34.86]Đơn côi mình ta vấn vương
                        [00:37.19]Hồi ức trong men say chiều mưa buồn
                        [00:40.73]Ngăn giọt lệ, ngừng khiến khóe mi sầu bi-i
                        [00:45.22]Đường xưa nơi cố nhân từ giã biệt li
                        [00:48.62]Cánh hoa rụng rơi
                        [00:51.18]Phận duyên mong manh rẽ lối trong mơ ngày tương phùng
                        [00:55.13]Oh-oh
                        [00:56.82]
                        [00:56.79]Tiếng khóc cuốn theo làn gió bay
                        [00:58.61]Oh-oh-oh-oh-oh-oh-oh-oh
                        [00:59.44]Người qua sông nỡ quên vớt ánh trăng tàn nơi này
                        [01:02.08]Oh-oh-oh-oh-oh-oh-oh-oh
                        [01:03.90]Trống vắng bóng ai dần hao gầy
                        [01:06.68]Oh-oh-oh-oh-oh, whoa, whoa
                        [01:09.95]
                        [01:09.98]Lòng ta xin nguyện khắc ghi trong tim tình nồng mê say
                        [01:13.72]Mặc cho tóc mây vương lên đôi môi cay
                        [01:17.08]Bâng khuâng mình ta lạc trôi giữa đời
                        [01:20.76]Ta lạc trôi giữa trời
                        [01:23.47]Oh-oh-ah, ah-ah, ah-ah
                        [01:26.49]
                        [01:25.07]Đôi chân lang thang về nơi đâu?
                        [01:27.07]Bao yêu thương giờ nơi đâu?
                        [01:28.63]Câu thơ tình xưa vội phai mờ
                        [01:30.13]Theo làn sương tan biến trong cõi mơ
                        [01:32.39]Mưa bụi vương trên làn mi mắt
                        [01:34.06]Ngày chia lìa, hoa rơi buồn hiu hắt
                        [01:35.99]Tiếng đàn ai thêm sầu tương tư lặng mình trong chiều hoàng hôn
                        [01:38.17]Tan vào lời ca
                        [01:39.03]
                        [01:39.46]Lối mòn đường vắng một mình ta
                        [01:41.26]Nắng chiều vàng úa nhuộm ngày qua
                        [01:43.01]Xin đừng quay lưng xóa
                        [01:44.76]Đừng mang câu hẹn ước kia rời xa
                        [01:46.62]Yên bình nơi nào đây?
                        [01:48.41]Chôn vùi theo làn mây
                        [01:50.17]Yeah, yeah, yeah
                        [01:51.69]La-la-la-la-la-la-la-la, la-la-la
                        [01:54.37]
                        [01:53.09]Người theo hương hoa, mây mù giăng lối
                        [01:56.39]Làn sương khói phôi phai đưa bước ai xa rồi
                        [02:00.19]Đơn côi mình ta vấn vương
                        [02:02.54]Hồi ức trong men say chiều mưa buồn
                        [02:06.09]Ngăn giọt lệ, ngừng khiến khóe mi sầu bi-i
                        [02:10.54]Đường xưa nơi cố nhân từ giã biệt li
                        [02:13.95]Cánh hoa rụng rơi
                        [02:16.55]Phận duyên mong manh rẽ lối trong mơ ngày tương phùng
                        [02:20.32]Oh-oh
                        [02:22.02]
                        [02:22.17]Tiếng khóc cuốn theo làn gió bay
                        [02:23.87]Oh-oh-oh-oh-oh-oh-oh-oh
                        [02:24.81]Người qua sông nỡ quên vớt ánh trăng tàn nơi này
                        [02:27.47]Oh-oh-oh-oh-oh-oh-oh-oh
                        [02:29.26]Trống vắng bóng ai dần hao gầy
                        [02:31.86]Oh-oh-oh-oh-oh, whoa, whoa
                        [02:35.37]
                        [02:35.36]Lòng ta xin nguyện khắc ghi trong tim tình nồng mê say
                        [02:39.04]Mặc cho tóc mây vương lên đôi môi cay
                        [02:42.41]Bâng khuâng mình ta lạc trôi giữa đời
                        [02:46.14]Ta lạc trôi giữa trời
                        [02:48.64]Oh-oh-ah, ah-ah, ah-ah
                        [02:52.22]
                        [02:53.17]Ta lạc trôi
                        [02:55.82]Lạc trôi
                        [02:56.94]Ta lạc trôi giữa đời
                        [03:01.19]Lạc trôi giữa trời
                        [03:04.47]Yeah, ah, ah, ah-ah-ah-ah-ah-ah
                        [03:11.85]Ah, ah, ah, ah, ah-ah-ah-ah-ah
                        [03:20.56]
                        [03:21.78]Ta đang lạc nơi nào?
                        [03:28.76]Ta đang lạc nơi nào?
                        [03:34.09]Lối mòn đường vắng một mình ta
                        [03:35.97]Ta đang lạc nơi nào?
                        [03:41.26]Nắng chiều vàng úa nhuộm ngày qua
                        [03:43.07]Ta đang lạc nơi nào?
                        """,
                        PlayCount = 120,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Cơn mưa ngang qua",
                        ArtistId = 1,
                        AlbumId = 1,
                        GenreId = GetGenreId("Rap/Hip-hop"),
                        DurationSeconds = 288,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/47988dbc-e5ee-4814-b020-7105b42f5917_C%C6%A1n_M%C6%B0a_Ngang_Qua.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/c57389dc-714a-4757-a9ba-c4587fec99ec_1657368250935_300.jpg",
                        Lyrics = """
                        [00:05.72]Ladykillal
                        [00:06.60]Uh
                        [00:12.69]Can you feel me
                        [00:13.66]Uh.!!!
                        [00:14.76]Youngpilots
                        [00:15.74]Uh Ok!!!!
                        [00:16.71]Eh.Eh.Eh.
                        [00:17.77]Cơn mưa ngang qua
                        [00:18.61]Cơn mưa đi ngang qua
                        [00:20.72]Đừng làm rơi thêm, thêm, thêm, nhiều giọt lệ uh... uh...
                        [00:21.74]Còn đâu đây bao câu ca anh tặng em.
                        [00:23.74]Tình yêu em mang cuốn lấp đi bao nhiêu câu ca.
                        [00:26.71]Và còn lại đây, đôi môi đau thương trong màn đêm.
                        [00:31.75]Phải lẻ loi, gồng mình bước qua niềm đau khi em rời xa...!!!!!!
                        [00:34.78]Cơn mưa rồi lại thêm, lại thêm, lại thêm.
                        [00:36.73]Xé đi không gian ngập tràn nụ cười.
                        [00:38.76]Nhìn lại nơi đây, bao kỉ niệm giờ chìm xuống dưới hố sâu vì em... T.T
                        [00:42.85]Chính em đã đổi thay.
                        [00:44.89]Và đôi bàn tay ấm êm, ngày nào còn lại giữ.
                        [00:47.82]Vì em... Vì em...!!!!
                        [00:50.86]Vì em đã xa rồi, tình anh chìm trong màn đêm.
                        [00:54.87]Là vì em đã quên rồi, tình anh chỉ như giấc mơ.
                        [00:58.85]Em bước đi rồi.Ôi bao cơn mưa.
                        [01:01.86]Em bước đi rồi.Xin hãy xua tan đi em, bóng dáng hình em.Em đã mãi rời xa...!!!
                        [01:07.80]My girl.Em quên đi bao nhiêu.
                        [01:09.87]My girl.Em quên đi bao lâu.
                        [01:11.83]My girl.Em quên đi cuộc tình mà anh trao em, thôi thôi em đi đi đã hết rồi...!
                        [01:15.90]My girl.Em quên đi bao nhiêu.
                        [01:17.97]My girl.Em quên đi bao lâu.
                        [01:19.95]My girl.Em quên đi cuộc tình.Em quên.quên.quên.
                        [01:23.92]Yeah...???
                        [01:26.90]Cơn mưa ngang qua mang em đi xa.
                        [01:28.85]Cơn mưa ngang qua khiến em nhạt nhòa.
                        [01:29.91]Chẳng một lời chào người vội rời bỏ đi không chia li cho con tim anh thêm bao yếu mềm...!
                        [01:34.87]Cơn mưa ngang qua cuốn đi bao yêu thương.
                        [01:36.91]Cơn mưa ngang qua khiến con tim mất phương hướng...
                        [01:37.96]Cơn mưa Kia nặng hạt, ôi mưa thêm nặng hạt.
                        [01:39.98]Em đã rời xa đôi bàn tay trong con tim của anh.
                        [01:42.99]Buông lơi bàn tay em đi, em đi rời xa bên tôi người ơi.
                        [01:46.07]Và buông lơi giấc mơ em cho, em cho con tim tôi đau biết mấy...
                        [01:50.00]Thôi cũng đã đến hồi kết.thật rồi mà người!
                        [01:52.03]Thôi cũng đá đến hồi kết.Đừng nhìn làm gì!
                        [01:54.01]Anh sẽ quên đi một ai, ai, ai, và rồi làm ngơ, ngơ, ngơ, uh
                        [01:58.91]Vì em đã xa rồi, tình anh chìm trong màn đêm.
                        [02:02.08]Là vì em đã quên rồi, tình anh chỉ như giấc mơ.
                        [02:06.00]Em bước đi rồi.Ôi bao cơn mưa.
                        [02:08.05]Em bước đi rồi.Xin hãy xua tan đi em, bóng dáng hình em.Em đã mãi rời xa...!!!
                        [02:14.05]My girl.Em quên đi bao nhiêu.
                        [02:17.05]My girl.Em quên đi bao lâu.
                        [02:19.05]My girl.Em quên đi cuộc tình mà anh trao em, thôi thôi em đi đi đã hết rồi...!
                        [02:23.08]My girl.Em quên đi bao nhiêu.
                        [02:25.12]My girl.Em quên đi bao lâu.
                        [02:27.12]My girl.Em quên đi cuộc tình.Em quên.quên.quên.
                        [02:30.13]Và rồi em đi qua bước qua.
                        [02:36.19]Ở lại chốn đây với bao u buồn.
                        [02:40.13]Để rồi tháng trôi qua, rồi năm trôi qua, thoáng qua.oh... oh.oh.!!!!
                        [02:48.10]Nụ cười em đang ở đâu, người ơi.Ở đâu.????
                        [02:50.12]Và bờ môi em đang ở đâu, anh tìm.
                        [02:55.13]Lục tìm ma không thấy.Nụ cười em.
                        [02:58.18]Người hãy nói trả lời đi.Vì sao em đi đi quên đi bao nhiêu giấc mơ... Bên anh xưa kia.???
                        [03:03.27]Cơn mưa cẫn rơi.rơi rơi.
                        [03:04.25]Cơn cơn mưa vẫn rơi... rơi rơi.
                        [03:07.29]Cơn cơn mưa vẫn rơi... rơi rơi.
                        [03:09.23]Cơn cơn mưa vẫn rơi... rơi rơi.
                        [03:11.17]Cơn mưa cẫn rơi.rơi rơi.
                        [03:13.26]Cơn cơn mưa vẫn rơi... rơi rơi.
                        [03:15.25]Cơn cơn mưa vẫn rơi... rơi rơi.
                        [03:17.23]Cơn cơn mưa vẫn rơi... rơi rơi.
                        [03:19.25]My girl.Em quên đi bao nhiêu.
                        [03:22.24]My girl.Em quên đi bao lâu.
                        [03:24.20]My girl.Em quên đi cuộc tình mà anh trao em, thôi thôi em đi đi đã hết rồi...!
                        [03:28.26]My girl.Em quên đi bao nhiêu.
                        [03:30.27]My girl.Em quên đi bao lâu.
                        [03:32.21]My girl.Em quên đi cuộc tình.Em quên.quên.quên.!
                        [03:35.38]Em quên mất rồi...!!!!
                        [03:48.35]
                        """,
                        PlayCount = 199,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Nơi này có anh",
                        ArtistId = 1,
                        AlbumId = 1,
                        GenreId = GetGenreId("Ballad"),
                        DurationSeconds = 260,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/808f77a7-fb11-4375-9e28-6000e24cd5f4_N%C6%A1i_N%C3%A0y_C%C3%B3_Anh.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/13b76d7e-12df-4e8a-a38e-091d3eb4168e_1710498649541_300.jpg",
                        Lyrics = """
                        [00:22.09] Em là ai từ đâu bước đến nơi đây
                        [00:25.07] Dịu dàng chân phương
                        [00:28.01] Em là ai tựa như ánh nắng ban mai
                        [00:31.00] Ngọt ngào trong sương
                        [00:33.02] Ngắm em thật lâu
                        [00:36.01] Con tim anh yếu mềm
                        [00:38.05] Đắm say từ phút đó
                        [00:40.04] Từng giây trôi yêu thêm
                        [00:43.07] Bao ngày qua bình minh đánh thức
                        [00:45.09] Xua tan bộn bề nơi anh
                        [00:48.09] Bao ngày qua niềm thương nỗi nhớ
                        [00:51.02] Bay theo bầu trời trong xanh
                        [00:54.00] Lướt đôi hàng mi
                        [00:57.01] Mong manh anh thẫn thờ
                        [00:59.04] Muốn hôn nhẹ mái tóc
                        [01:01.03] Bờ môi em anh mơ
                        [01:04.05] Cầm tay anh dựa vai anh
                        [01:07.02] Kề bên anh nơi này có anh
                        [01:09.05] Gió mang câu tình ca
                        [01:10.09] Ngàn ánh sao vụt qua
                        [01:12.01] Nhẹ ôm lấy em
                        [01:15.00] Cầm tay anh dựa vai anh
                        [01:17.07] Kề bên anh nơi này có anh
                        [01:19.09] Khép đôi mi thật lâu
                        [01:21.03] Nguyện mãi bên cạnh nhau
                        [01:22.05] Yêu say đắm như ngày đầu
                        [01:25.02] Mùa xuân đến bình yên
                        [01:28.04] Cho anh những giấc mơ
                        [01:30.04] Hạ lưu giữ ngày mưa
                        [01:33.06] Ngọt ngào nên thơ
                        [01:35.06] Mùa thu lá vàng rơi
                        [01:38.07] Đông sang anh nhớ em
                        [01:40.08] Tình yêu bé nhỏ xin
                        [01:44.00] Dành tặng riêng em
                        [01:57.02] Còn đó tiếng nói ấy
                        [01:58.03] Bên tai vấn vương bao ngày qua
                        [02:00.01] Ánh mắt bối rối
                        [02:00.09] Nhớ thương bao ngày qua
                        [02:02.07] Yêu em anh thẫn thờ
                        [02:03.08] Con tim bâng khuâng đâu có ngờ
                        [02:05.00] Chẳng bao giờ phải mong chờ
                        [02:06.04] Đợi ai trong chiều hoàng hôn mờ
                        [02:07.08] Đắm chìm hoà vào vần thơ
                        [02:09.01] Ngắm nhìn khờ dại mộng mơ
                        [02:10.04] Đừng bước vội vàng rồi làm ngơ
                        [02:11.06] Lạnh lùng đó làm bộ dạng thờ ơ
                        [02:13.00] Nhìn anh đi em nha
                        [02:13.09] Hướng nụ cười cho riêng anh nha
                        [02:15.01] Đơn giản là yêu
                        [02:15.08] Con tim anh lên tiếng thôi
                        [02:17.06] Cầm tay anh dựa vai anh
                        [02:20.03] Kề bên anh nơi này có anh
                        [02:22.05] Gió mang câu tình ca
                        [02:24.00] Ngàn ánh sao vụt qua
                        [02:25.00] Nhẹ ôm lấy em
                        [02:28.00] Cầm tay anh dựa vai anh
                        [02:30.06] Kề bên anh nơi này có anh
                        [02:32.09] Khép đôi mi thật lâu
                        [02:34.04] Nguyện mãi bên cạnh nhau
                        [02:35.05] Yêu say đắm như ngày đầu
                        [02:38.02] Mùa xuân đến bình yên
                        [02:41.04] Cho anh những giấc mơ
                        [02:43.03] Hạ lưu giữ ngày mưa
                        [02:46.06] Ngọt ngào nên thơ
                        [02:48.07] Mùa thu lá vàng rơi
                        [02:51.09] Đông sang anh nhớ em
                        [02:53.08] Tình yêu bé nhỏ xin
                        [02:57.00] Dành tặng riêng em
                        [03:02.05] Nhớ thương em
                        [03:07.07] Nhớ thương em lắm
                        [03:12.02] Phía sau chân trời
                        [03:14.07] Có ai băng qua lối về
                        [03:16.07] Cùng em đi trên đoạn đường dài
                        [03:20.01] Cầm tay anh dựa vai anh
                        [03:22.08] Kề bên anh nơi này có anh
                        [03:25.01] Gió mang câu tình ca
                        [03:26.05] Ngàn ánh sao vụt qua
                        [03:27.06] Nhẹ ôm lấy em
                        [03:30.06] Cầm tay anh dựa vai anh
                        [03:33.02] Kề bên anh nơi này có anh
                        [03:35.05] Khép đôi mi thật lâu
                        [03:37.00] Nguyện mãi bên cạnh nhau
                        [03:38.01] Yêu say đắm như ngày đầu
                        [03:40.08] Mùa xuân đến bình yên
                        [03:44.00] Cho anh những giấc mơ
                        [03:45.09] Hạ lưu giữ ngày mưa
                        [03:49.02] Ngọt ngào nên thơ
                        [03:51.02] Mùa thu lá vàng rơi
                        [03:54.03] Đông sang anh nhớ em
                        [03:56.05] Tình yêu bé nhỏ xin
                        [03:59.07] Dành tặng riêng em
                        """,
                        PlayCount = 2000,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Sóng gió",
                        ArtistId = 4,
                        AlbumId = 4,
                        GenreId = GetGenreId("Chill"),
                        DurationSeconds = 254,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/5186c333-61da-4c0d-9acb-a86b3bc020bb_S%C3%B3ng_Gi%C3%B3.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/66321191-905c-424c-980d-d7a13fb217b8_1683539255051_300.jpg",
                        Lyrics = """
                        [Ver 1:]
                        Hồng trần trên đôi cánh tay, họa đời em trong phút giây
                        Từ ngày thơ ấy còn ngủ mơ đến khi em thờ ơ (hờ, hờ)
                        Lòng người anh đâu có hay một ngày khi vỗ cánh bay
                        Từ người yêu hóa thành người dưng đến khi ta tự xưng à (hà, hà)
                        Thương em bờ vai nhỏ nhoi, đôi mắt hóa mây đêm
                        Thương sao mùi dạ lý hương vương vấn mãi bên thềm
                        Đời phiêu du cố tìm một người thật lòng
                        Dẫu trời mênh mông anh nhớ em (anh nhớ em)
                        Chim kia về vẫn có đôi sao chẳng số phu thê?
                        Em ơi đừng xa cách tôi, trăng cố níu em về
                        Bình yên trên mái nhà nhìn đời ngược dòng
                        Em còn bên anh có phải không? (Có phải không?)

                        [Chorus:]
                        Trời ban ánh sáng, năm tháng tư bề, dáng ai về chung lối
                        Người mang tia nắng nhưng cớ sao còn tăm tối?
                        Một mai em lỡ vấp ngã trên đời thay đổi
                        Nhìn về anh người chẳng khiến em lẻ loi
                        Hỡi ah, hỡi ah, hỡi ah, a-ah (ah, ah)
                        Cùng chạy lòng chẳng nói ra, nói ra nhưng mà (suýt nữa biệt ly)
                        Hỡi ah, hỡi ah, hỡi ah, a-ah (yah, yah)
                        Cùng chạy lời lại xót xa, xót xa, a-ah (way, way)

                        [Ver 2:]
                        Ah! Nếu em có về anh sẽ mang hết những suy tư
                        Mang hết hành trang những ngày sống khổ để cho gió biển di cư
                        Anh thà lênh đênh không có ngày về, hóa kiếp thân trai như Thủy Hử
                        Chẳng đành để em từ một cô bé sóng gió vây quanh thành quỷ dữ
                        Ta tự đẩy mình hay tự ta trói bây giờ có khác gì đâu
                        Ta chả bận lòng hay chẳng thể nói tụi mình có khác gì nhau?
                        Yêu sao cánh điệp phủ mờ nét bút dẫu người chẳng hẹn đến về sau
                        Phố thị đèn màu ta chỉ cần chung lối để rồi sống chết cũng vì nhau
                        Nhặt một nhành hoa rơi, đoạn đường về nhà thật buồn em ơi
                        Dòng người vội vàng giờ này tình ơi, tình ơi, tình ơi, em ở đâu rồi? (Em ở đâu rồi?)
                        Lặng nhìn bờ vai xưa tựa đầu mình hỏi rằng khổ chưa
                        Đành lòng chặn đường giờ đừng đi, đừng đi, đừng đi vì câu hứa
                        (Đừng đi, đừng đi, đừng đi vì câu hứa)

                        [Chorus:]
                        Trời ban ánh sáng năm tháng tư bề, dáng ai về chung lối
                        Người mang tia nắng nhưng cớ sao còn tăm tối
                        Một mai em lỡ vấp ngã trên đời thay đổi
                        Nhìn về anh người chẳng khiến em lẻ loi

                        [Bridge:]
                        Ngày buồn giờ áo ai khâu vá quàng rồi
                        Lặng nhìn dòng nước con sông phút bồi hồi
                        Một lần này hỡi em ơi ở lại đi
                        Vạn trùng cơn đau ngoài kia chỉ là bão tố

                        [Chorus:]
                        Trời ban ánh sáng năm tháng tư bề, dáng ai về chung lối (chung lối)
                        Người mang tia nắng nhưng cớ sao còn tăm tối
                        Một mai em lỡ vấp ngã trên đời thay đổi
                        Nhìn về anh người chẳng khiến em lẻ loi

                        [Outro:]
                        Người thì vẫn ở đây, người thì cách vạn *** ngàn mây không say không về
                        Rượu nào mà chả đắng, đoạn đường dài giờ này quạnh vắng ai buông câu thề?
                        Chỉ còn lại nỗi nhớ ngày nào chuyện tình mình vụn vỡ tơ duyên lỡ làng
                        Lùi lại về đằng xa cuộc đời mình chẳng bằng người ta, tiếng lòng thở than
                        """,
                        PlayCount = 5000,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Miền đất hứa",
                        ArtistId = 2,
                        AlbumId = 2,
                        GenreId = GetGenreId("R&B"),
                        DurationSeconds = 239,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/aaba0a3e-0f6d-495c-9dad-4a51830967da_Mi%E1%BB%81n_%C4%90%E1%BA%A5t_H%E1%BB%A9a.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/0cdd7d7f-a2ab-4010-a9c3-adce47d92a77_1700798106658_300.jpg",
                        Lyrics = """
                        Nắng của trời, tiếng của người
                        Gần bên nhau làm em thấy vui
                        Muốn theo chân người đi khắp nơi
                        Từ khi anh tới lòng thấy vui lên

                        Đối với đầu, ngả với nghiêng
                        Thật nhiều điều làm anh phát điên
                        Muốn tha anh về đây để làm của riêng
                        Và cho anh thấy miền đất hứa kia

                        Không cầm tinh con ngựa nhưng mà vẫn bất kham
                        Không muốn bay lên trời nên không cần nấc thang
                        Anh sẽ gánh tất cả những điều làm em bất an
                        Tình yêu tạo nên miền đất hứa từ nơi đất hoang vu

                        Đen Vâu không đẹp trai nhưng mà không sao
                        Anh không đăng ký thi để xem ai là triệu phú
                        Khi em bên cạnh anh biết người ấy là ai
                        Người ta không tin anh biết cách để xem tinh tú

                        Nhìn em anh thấy ngày mai
                        Thay vì nói với anh nơi nào là miền đất hứa
                        Hãy cho anh biết em đang ở đâu
                        Nếu phải viết ra hết tất cả nỗi niềm chất chứa

                        Nó sẽ bán chạy hàng đầu
                        Em là người khiến anh bớt hao mòn tâm trí
                        Anh không thích những cuộc chuyện trò
                        Mà cứ phải dồn thâm ý quá nhiều thứ mà anh giấu đi

                        Nó vẫn còn âm ỉ em lại khiến cho nó bùng lên
                        Bằng những ngón đòn tâm lý
                        Em và kim cương PNJ, không biệt được.
                        Hôm qua đi Vinmec bác sĩ kê đơn em là biệt dược

                        Lắp thêm pin mặt trời vì cái lúc em cười
                        Anh thu được quang năng người ta bảo anh nói phét
                        Oh fordamac anh không hề quan tâm
                        Em muốn đi với anh, em không cần ta lướt đi quá nhanh

                        Chông chênh tựa như lướt trên phiến băng
                        Em không sợ nắng cháy hay gió hanh
                        Đừng thấy em mềm tưởng em mỏng manh
                        Em muốn được lo nỗi lo của anh

                        Bên trong khu vườn của hai chúng ta
                        Bình yên cùng nhau dưới bóng của cây chanh thần
                        Lúc bóng tối đang lùi xa miền đất hứa đó sẽ hiện ra
                        Màu nắng lấp lánh trên làn da những ngày xanh ngát

                        Anh sẽ không đưa em vào trong cái chuỗi thức ăn
                        Vì hai đứa mình ngang hàng con đường mình sẽ đi
                        Không quyết định bởi mức xăng, không muốn tỏ ra ngang tàng
                        Nhưng cái gì tới nó tới cứ mặc nhiên đón chào

                        Mưa tìm chỗ trú nắng mặc thêm nón vào
                        Mình đều là động vật bậc cao nhưng mà lại thích đứng dưới thấp
                        Có nhược điểm là biết cố gắng, ưu điểm là cái tính cố chấp
                        Mình như là cây trong lô cao su cũng là ông chủ đồn điền

                        Tự cào sâu vào trong lớp vỏ cho nhựa sống hóa dòng tiền
                        Gọi đây là tâm trạng khi yêu khi anh nhập vai như Lương Triều Vỹ
                        Nếu mà không có được tình yêu thì tinh cầu là một vương triều khỉ
                        Một lần nữa chẳng nghĩ gì nhiều, một lần nữa cũng chỉ vì liều

                        Đi theo em về phương trời vĩ đại anh sẽ không cần nghĩ lại
                        Em cũng không cần nghĩ thêm mong mai sau êm vui
                        Không mong tương lai hái ra vàng con tim anh như cái la bàn
                        Khi mà anh tìm đường nó chỉ em
                        """,
                        PlayCount = 300,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Nỗi đau giữa hòa bình",
                        ArtistId = 3,
                        AlbumId = 3,
                        GenreId = GetGenreId("Rock"),
                        DurationSeconds = 319,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/36f3adaf-478d-47e9-9bd1-38757edd47c9_N%E1%BB%97i_%C4%90au_Gi%E1%BB%AFa_H%C3%B2a_B%C3%ACnh.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/1ab1e82a-0d5d-4b98-82a7-c82af0443a96_1755837841402_300.jpg",
                        Lyrics = """
                        Mọi người kể những câu chuyện xưa đã trở thành huyền thoại
                        và viết nên bao bài ca để ngàn năm hát mãi
                        Về người mẹ việt nam anh hùng, đã quên mình
                        Gạt đi nước mắt, tiễn con lên đường

                        Nỗi đau người ở lại mấy ai hiểu được
                        Vì trái tim yêu đàn con và yêu đất nước
                        Người mẹ nào không xót thương con, nhớ thương con
                        Chờ tin chiến thắng về trong hy vọng..

                        Người mẹ ấy tìm con giữa tiếng reo dân tộc
                        Người vợ ấy tìm chồng giữa đám đông
                        Hoà bình đến rồi sao anh vẫn chưa trở về
                        Giữa tiếng cười, mình mẹ rơi nước mắt

                        Đạn bom đã ngừng bay nhưng vết thương sâu này
                        Vẫn âm ỉ ngày đêm làm sao nguôi
                        “hoà bình đến rồi sao những đứa con của tôi
                        Còn ngủ mãi giữa chiến trường thôi?”

                        Một thời chiến tranh qua rồi mang cả con đi rồi
                        Chỉ có chim câu gửi về màu xanh chiếc áo
                        Viên đạn từ muôn hướng ghim vào trái tim mẹ
                        Mừng cho đất nước và đau cho mình

                        Người mẹ ấy tìm con giữa tiếng reo dân tộc
                        Người vợ ấy tìm chồng giữa đám đông
                        Hoà bình đến rồi sao anh vẫn chưa trở về
                        Giữa tiếng cười, mình mẹ rơi nước mắt

                        Đạn bom đã ngừng bay nhưng vết thương sâu này
                        Vẫn âm ỉ ngày đêm làm sao nguôi
                        “hoà bình đến rồi sao những đứa con của tôi
                        Còn ngủ mãi giữa chiến trường thôi?”

                        Độc lập đổi bằng bao nhiêu xương máu
                        Hoà bình đổi bằng bao nhiêu nỗi đau
                        """,
                        PlayCount = 363,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Một ngày nào đó",
                        ArtistId = 3,
                        AlbumId = 3,
                        GenreId = GetGenreId("Chill"),
                        DurationSeconds = 259,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/f0d7ea16-ee63-477a-a34e-bf880ea30c02_M%E1%BB%98T_NG%C3%80Y_N%C3%80O_%C4%90%C3%93.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/7c8f4c9a-e81b-4cb3-b670-fe863909e7df_1757580808679_300.jpg",
                        Lyrics = """
                        Một ngày nào đó rồi mình lại đi với nhau những trưa mây bay ngang đầu
                        Là ngày chẳng còn thời gian để tiêu pha nữa chẳng tiếc những thứ hai ta chưa
                        Một ngày nào đó khi cơn mơ ngã ngũ rồi sướng vui đi không khứ hồi
                        Là ngày anh mang hình dung mà anh yêu dấu về đúng nơi anh mong như ngày đầu

                        Anh như đang cô đơn trên con tàu đếm những thứ vốn trôi qua rất mau
                        Lòng mình còn thổn thức cuối chương một vẫn đang chờ ngày mai
                        Nhớ những lúc lái xe trong mưa chiều nhớ ký ức năm xưa ta luôn yêu
                        Cùng một ngày lời hứa vẫn đang nợ chắc nên hẹn ngày mai ta chưa thể dừng lại

                        Đời cứ cuốn ta hoài ngày ngày một nhanh hơn
                        Ta chưa đến không phải vì anh đã quên
                        Chờ thêm chút thôi

                        Một ngày nào đó rồi mình lại đi với nhau những trưa mây bay ngang đầu
                        Là ngày chẳng còn thời gian để tiêu pha nữa chẳng tiếc những thứ hai ta chưa
                        Một ngày nào đó khi cơn mơ ngã ngũ rồi sướng vui đi không khứ hồi
                        Là ngày anh mang hình dung mà anh yêu dấu về đúng nơi anh mong như ngày đầu

                        [RAP]
                        Anh chưa từng quên giấc mơ trước khi chúng ta bị thấm đòn đời
                        Kéo ga thật sâu đến nơi thật xa có khi chỉ để ngắm vòm trời
                        Rất nhiều thời gian rất nhiều niềm vui hát ca chẳng cần lắm đồng lời
                        Ánh đèn đường xa lướt qua thật mau chỉ còn lại những chấm tròn rồi

                        Những ngày đó thật là hoang dại thật là cuồng điên
                        Anh luôn đi tìm ngày bình yên không vui không buồn
                        Ta không quên những khi ta đã từng gặp người lành, người khó, người đau
                        Lao phăng qua nắng mưa bão bùng để biết rằng sau cùng chỉ muốn cười sâu

                        Chúng ta đều được cấu thành bởi thật nhiều các nguyên tố
                        Và hiện tại là kết quả của hằng hà sa số những nguyên nhân
                        Thế nên mỗi quyết định mỗi bước chân đều tạo ra vô vàn những biến số
                        Đã phải học đủ thứ ở trong đời giờ phải học cả cách để yên tâm

                        Nếu có một ngày nào mà thâm tâm mình có thể được tự do bay lượn
                        Là ngày ta trả lại thế giới này tất cả những điều còn đang vay mượn
                        Anh đang hoàn thiện dáng dấp của khu vườn trong tâm trí mà anh vẫn thường mơ
                        Một ngày nào đó ta muốn thấu tỏ chính mình tận đến cùng kẽ tóc và đường tơ

                        Đời cứ cuốn ta hoài ngày ngày một nhanh hơn
                        Ta chưa đến không phải vì anh đã quên
                        Chờ thêm chút thôi

                        Một ngày nào đó rồi mình lại đi với nhau những trưa mây bay ngang đầu
                        Là ngày chẳng còn thời gian để tiêu pha nữa chẳng tiếc những thứ hai ta chưa
                        Một ngày nào đó khi cơn mơ ngã ngũ rồi sướng vui đi không khứ hồi
                        Là ngày anh mang hình dung mà anh yêu dấu về đúng nơi anh mong như ngày đầu
                        """,
                        PlayCount = 636,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Mở mắt",
                        ArtistId = 2,
                        AlbumId = 2,
                        GenreId = GetGenreId("Rap/Hip-hop"),
                        DurationSeconds = 260,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/db5d1a37-88e5-454b-890e-1301c7483592_M%E1%BB%9F_m%E1%BA%AFt.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/bca58459-c53c-432b-b347-eae59d085410_1717931105818_300.jpg",
                        Lyrics = """
                        Cho con mở mắt, con nhìn đời...
                        Cho con mở mắt, con nhìn trời...

                        [Lil Wuyn]:
                        Ohhh
                        Mở mắt, lặp lại một ngày như khi ta mở tắt
                        Ánh sáng cuộc sống hãy thắp lên thêm
                        Thấy lắm người kêu thở than trắc trở trách
                        Còn người sung sướng nệm ấm chăn êm

                        Chỉ là, chút cảm giác thoáng qua
                        Và còn sợ hãi khi phải đương đầu
                        Đáng tiếc tạm biệt những gì đã xa
                        Vì có cách nào khác khi ngựa quen đường đâu

                        Và rồi được hoặc mất
                        Nhiều người chấp nhận sẽ đánh cược vào tất
                        Nhiều người hối hận nên rút lại và cất
                        Nếu may mắn, sẽ phát tài, rồi phất lên

                        Tay làm hàm nhai, nên tự tin ta vẫn cất lên
                        Dẫu
                        thì không như mong đợi hay trông đợi
                        Thì
                        Cũng đã cố gắng trọn vẹn bằng cả tâm huyết một lần trong đời để đạt được
                        CẢM ƠN!

                        Đưa cho con vòng tay che chở
                        Luôn đi cùng ngày qua nâng đỡ
                        Bao đau đớn khôn lớn như này
                        Cũng đã phải vấp ngã đến nát cả thân mình
                        Và
                        Trong nhà thì khuyên, học sách thánh hiền
                        Ngoài đường thì nên, học cách tránh hiền
                        Vì sói già gian ác, sẵn sàng buông lời
                        Và mặc xác làm đời mày tan nát

                        Có mất mát cũng không sao cả
                        Vì bài học mới là hơn cả
                        Tìm những người mày mang ơn trả
                        Mọi thứ cứ để nhân quả

                        Say,
                        Có mất mát cũng không sao cả
                        Vì bài học mới là hơn cả
                        Tìm những người mày mang ơn trả
                        Mọi thứ cứ để nhân quả

                        [Đen]:
                        Yeah,
                        Anh em mình đi trên con đường lạ
                        Đi mãi rồi cũng thành đường quen
                        Mấy điều xấu chúng nó thường gạ
                        Không chỉ một mà là mười phen

                        Từ những ngày đầu tiên mở mắt
                        Nằm vắt tay lên trán nhiều đêm
                        Nhiều người cơ bản là người lạ
                        Nhạc lên rồi bỗng thành người quen

                        Cuộc sống này ngày càng tròng trành
                        Nên mới cố gắng làm nhạc dễ chịu
                        Viết ra bằng tất cả lòng thành
                        Nên cứ từ từ rồi họ sẽ hiểu

                        Vào đời mở mắt
                        Vài lần thở hắt
                        Vài điều lỡ mất
                        Vài cành lỡ ngắt
                        Vài lần ngã sấp
                        Phủi quần, sửa tóc
                        Cố mà viết đến khi cạn lời trước khi lửa tắt

                        Người xưa chuyện cũ gió thoảng cây lay
                        Như người lữ khách lai vãng đâu đây
                        Mặt trời vẫn cứ ló dạng ngày ngày
                        Bàn tay năm ngón khó cản mây bay x2

                        Tay làm hàm nhai tay quai thì miệng trễ
                        Hôm nay dấn thân ngày mai nhiều chuyện kể
                        Kể những vô vàng hình dung trong miệng kẻ
                        Không thiện chí, những chuyện ý, chỉ là chuyện sẽ

                        Sẽ đến sẽ có sẽ qua
                        Và kẻ mến kẻ ngó kẻ la

                        Bật beat đeo tai nghe hãy làm nhạc và mặc kệ đi Hiếu (Tên thật của Lil Wuyn)
                        Học cách không đôi co vì mỗi người có một hệ quy chiếu, diaaa

                        [Lil Wuyn]:
                        Đúng đúng sai sai
                        Lúng túng với tương lai
                        khi mà mọi thứ vẫn chỉ là ẩn số
                        Và đôi mắt nếu không mở để ta nhìn cho rõ
                        Không tận dụng những ngày còn đang tỏ
                        Để gặp chuyện là bóp phanh hay rụt rè lẫn tránh
                        Thì dám chắc không hôm nay, sau này cũng phải gánh oh

                        Tốt gỗ tốt tánh vẫn hơn tốt mã
                        Sự thật kiên cố khó để đốn ngã
                        Trước khi gieo hạt nghĩ đến hệ quả
                        Từ giã tất cả những ngày chấp mê
                        Học cách chấp nhận
                        Chứ đừng chấp hận
                        Chứ đừng than thân
                        Rồi lại trách phận
                        Ta cứ chấp thuận
                        Những lời phê phán
                        Vì đến một ngày họ cũng sẽ chê CHÁN...

                        Chẳng thể biết phía trước nắng ấm hay bao bão tố
                        Chẳng thể biết phía trước ai đến ai đi ngày mai
                        Chẳng thể nói khốn khó cố níu đôi chân này xuống
                        Để lại là dối gian bao người khiến con ngươi kia mờ phai

                        Hãy mở mắt ra nhìn đời...
                        Hãy mở mắt ra nhìn trời...
                        Cho con mở mắt ra nhìn đời...
                        Cho con mở mắt ra nhìn trời...
                        """,
                        PlayCount = 333,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Hơn là bạn",
                        ArtistId = 5,
                        AlbumId = null,
                        GenreId = GetGenreId("Acoustic"),
                        DurationSeconds = 227,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/ea9bd784-bbe3-457a-bcb6-5ac01585a467_H%C6%A0N_L%C3%80_B%E1%BA%A0N.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/46ea64c3-e2f0-48d2-8885-25372b07ea9e_1762160793118_300.jpg",
                        Lyrics = """
                        Nói cho em nghe những câu chuyện chỉ mong mình sẽ đi xa hơn là bạn
                        Cách em vui cười với yêu đời làm cho lòng anh si mê rối bời
                        Em biết không, em biết không
                        Bao lâu anh cũng đợi, bao lâu em sẽ bước tới?
                        Hứa sẽ không rời, hứa sẽ suốt đời
                        Dành hết cả trái tim cho em thôi đó

                        Làm sao nói cho vừa
                        Nàng còn chờ chi nữa kẻo trời đổ mưa
                        Từng đêm vẫn chờ mong
                        Vì một người anh đã trót yêu thật lòng

                        Nàng như tia nắng ôm hết cả đất trời
                        Làm anh tan chảy theo em giữa dòng đời
                        Nàng như một đóa hoa xinh thật kiêu kỳ
                        My little little little cherry!

                        Nàng như kho báu ai cũng muốn kiếm tìm
                        Làm anh thao thức không thể nào ngồi im
                        Nàng như sao sáng trong đêm nhung đen tuyền
                        My little little little cherry!

                        Hey, mà này người từ nơi đâu?
                        Mà lại thì thầm vài đôi câu
                        Và rồi lại chậm chậm theo sau
                        Và rồi như ta có duyên rất lâu
                        Từ từ nắm tay
                        Từ từ mới hay
                        Mình cũng đã yêu người rồi
                        Nhớ anh từng đêm
                        Nhớ cái ôm thật dịu êm
                        Người đã khiến em đổi thay
                        Người yêu em hết đêm ngày
                        Chính anh là anh khiến trái tim bật đèn xanh
                        Em có thể mở lòng cùng người em yêu trong mộng nhé

                        Làm sao nói cho vừa
                        Nàng còn chờ chi nữa kéo trời đổ mưa
                        Từng đêm vẫn chờ mong
                        Vì một người anh đã trót yêu thật lòng

                        Nàng như tia nắng ôm hết cả đất trời
                        Làm anh tan chảy theo em giữa dòng đời
                        Nàng như một đóa hoa xinh thật kiêu kỳ
                        My little little little cherry!

                        Nàng như kho báu ai cũng muốn kiếm tìm
                        Làm anh thao thức không thể nào ngồi im
                        Nàng như sao sáng trong đêm nhung đen tuyền
                        My little little little cherry!

                        Có nên tin không?
                        Những lời nói rất ngọt ngào kia
                        Em thật có nên tin không?
                        Chàng vẫn cứ theo em và chiều em
                        Little cherry
                        Không ai khác hơn em
                        Người anh yêu nhất trên đời
                        Làm anh say với nụ cười

                        Oh oh oh oh oh oh
                        My little little cherry
                        Không ai khác ngoài em
                        Oh oh oh oh oh oh
                        My little little cherry
                        Không ai khác ngoài em

                        Cho anh theo với
                        Nàng như tia nắng ôm hết cả đất trời
                        Làm anh tan chảy theo em giữa dòng đời
                        Nàng như một đóa hoa xinh thật kiêu kỳ
                        My little little little cherry!

                        Nàng như kho báu ai cũng muốn kiếm tìm
                        Làm anh thao thức không thể nào ngồi im
                        Nàng như sao sáng trong đêm nhung đen tuyền
                        My little little little cherry!

                        Everybody say
                        Oh oh oh oh oh oh
                        My little little cherry
                        Không ai khác ngoài em
                        Oh oh oh oh oh oh
                        My little little cherry
                        Không ai khác ngoài em
                        Oh oh oh oh oh oh
                        My little little cherry
                        Không ai khác ngoài em
                        """,
                        PlayCount = 0,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Pending,
                    },

                    new Song
                    {
                        SongTitle = "Hồng nhan",
                        ArtistId = 4,
                        AlbumId = null,
                        GenreId = GetGenreId("Bolero"),
                        DurationSeconds = 228,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/2e54549e-52e2-4b2d-a0d6-aaa1cbe3b39f_H%E1%BB%93ng_Nhan_(K-ICM_Mix).mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/b2be9819-3d7e-4800-ad30-411092d3070e_1683538881871_300.jpg",
                        Lyrics = """
                        [00:03.37]Ah-ah-ah-ah, ah-ah-ah
                        [00:07.98]Hah-hah-hah, hah-hah-hah, hah-hah
                        [00:11.66]Hah-hah-hah, hah-hah-hah-ah-ah-ah-ah, ah-ah
                        [00:20.69]Hah-hah-hah
                        [00:22.01]Và dòng thư tay em gửi trao anh ngày nào
                        [00:24.66]Giờ còn lại hư vô, em gửi anh đây lời chào
                        [00:27.58]Mà nhìn người đi vội, mình làm gì nên tội?
                        [00:30.48]Tại sao lại cách xa, còn yêu như thế mà?
                        [00:33.11]Để lệ hoen mi khi mùa xuân đang thầm thì
                        [00:35.98]Nhìn người mà ra đi, anh chẳng níu kéo điều gì
                        [00:38.92]Mà nghe sao đáng thương, nhìn nhau như cố hương
                        [00:41.56]Tìm em ở bốn phương, vì say nên vấn vương
                        [00:44.85]Em ơi vô tình, chuyện tình mình gặp không may
                        [00:47.81]Em xa nơi này để giọt lệ ở bên đây
                        [00:50.14]Bầu trời giờ hắt hiu, nhìn về nơi đó đây
                        [00:53.05]Ngoài trời thì có mây, chỉ còn lại là đắng cay
                        [00:56.31]Thương cha thương mẹ để đành lòng mà quay lưng
                        [00:59.02]25 âm lịch, nhìn người cười mà rưng rưng
                        [01:01.60]Kia kia là pháo hoa, rộn ràng người đến xem
                        [01:04.20]Họ hàng mừng kết duyên, còn phần mình là hết duyên
                        [01:07.11]Ah, anh như trẻ lạc còn tăm tối giữa rừng thông
                        [01:09.84]Nơi cánh chim nhỏ lạc đàn tìm bến đỗ để ngừng trông
                        [01:12.70]Anh là một con đom đóm mất ánh sáng nên xoay vòng
                        [01:15.27]Gieo cho anh cả một mầm sống nhưng chẳng chịu khó vun trồng
                        [01:18.63]Vì lúc ấy ta còn trẻ nên đời bạc và mưu sinh
                        [01:21.18]Anh chưa học hết lớp mười, người ta gọi là lưu linh
                        [01:24.08]Anh gắn bó với sông nước và cảnh vật này hữu tình
                        [01:26.80]Còn người ta cho em áo lụa, hỏi tại sao chẳng phụ mình?
                        [01:30.22]Tình yêu ơi, bình yên ơi, về đây đi
                        [01:34.53]Để anh ôm, để gió cuốn đêm nay ai đưa về nhà
                        [01:38.45]Để gió hát vang lên câu tình ca
                        [01:41.14]Để lệ hoen mi khi mùa xuân đang thầm thì
                        [01:43.78]Nhìn người mà ra đi, anh chẳng níu kéo điều gì
                        [01:46.68]Mà nghe sao đáng thương, nhìn nhau như cố hương
                        [01:49.34]Tìm em ở bốn phương, vì say nên vấn vương
                        [01:53.14]Hết rồi, cuối cùng nắng thì cũng đã ngả vàng
                        [01:55.96]Bên người nhân tình, em phải thương bản thân mình
                        [01:58.86]Buồn lắm phải không? Giã tràng lấp biển Đông
                        [02:01.62]Biết người cũng chả trông nên thôi câu chuyện thảy ra sông
                        [02:03.99]Nhưng nếu anh say như thế này thì ai xem?
                        [02:06.32]Người ta sẽ nói anh tệ với những thứ mà em đem
                        [02:09.18]Vì thế nên anh phải sống như cái cách anh từng mơ
                        [02:11.91]Dù cho bản thân này hóa đá nhưng trái tim chẳng ngừng thở
                        [02:14.82]Và ba của anh là lính, má anh từng làm cán bộ
                        [02:17.84]Anh không cho phép mình khóc, xe có hư cũng ráng độ
                        [02:20.64]Đời người là kiếp lãng du, anh chẳng may làm lữ khách
                        [02:23.12]Đi với nhau cả một hành trình, giờ có xa chẳng nỡ trách
                        [02:26.33]Em ước là đời của em bình yên, chẳng buồn phiền như người ta
                        [02:29.72]Mà giờ ra đây mà xem, có người đang lên kiệu hoa
                        [02:32.18]Và rồi sẽ tốt nhưng mà ở nơi khác chẳng còn bận lòng như ở đây
                        [02:34.97]Tình yêu của anh thì có đủ mùi vị nhưng mà chẳng ngọt như ở Tây
                        [02:38.01]Na-na-na, na-na-na-na-na-na
                        [02:40.40]Na-na-na-na-na-na-na-na-na-na-na
                        [02:43.14]Na-na-na-na-na, na-na-na-na-na
                        [02:45.81]Na-na-na-na-na-na-na-na-na-na
                        [02:48.75]Để lệ hoen mi khi mùa xuân đang thầm thì
                        [02:51.58]Nhìn người mà ra đi, anh chẳng níu kéo điều gì
                        [02:54.34]Mà nghe sao đáng thương, nhìn nhau như cố hương
                        [02:57.19]Tìm em ở bốn phương, vì say nên vấn vương
                        [03:00.04]Ah-hah-hah-hah
                        [03:03.51]
                        """,
                        PlayCount = 0,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Pending,
                    },

                    new Song
                    {
                        SongTitle = "Về bên anh",
                        ArtistId = 4,
                        AlbumId = null,
                        GenreId = GetGenreId("Ballad"),
                        DurationSeconds = 260,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/32105b7a-967d-49db-9cec-b4e4937b3b62_V%E1%BB%81_B%C3%AAn_Anh.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/32e0a90f-b3cb-420e-b661-d546996ee05b_1711427484011_300.jpg",
                        Lyrics = """
                        Đã có lúc ấm áp đôi tay, cùng nhau nhìn lên trời cao
                        Đến phút cuối em bước ra đi, làm sao để giữ được em
                        Tìm hoài hình bóng lúc ấy, tìm hoài cảm giác bối rối, lòng này anh đã cố nói đừng đi
                        Buổi chiều hôm ấy khuất lối, chìm vào bóng tối nhứt nhói, lệ nhòe hoen mắt chẳng thể ngừng rơi
                        Ở nơi đó em có vui không, người bên em có giống anh không?
                        Họ có biết những lúc em đau, cần chia sớt với những u sầu
                        Về bên anh gió lộng đồi hoang, ở bên anh yên giấc mơ màng
                        Ngồi đây nghe tiếng lòng thở than, chờ mong ai hơi ấm nhẹ nhàng
                        Dù rằng một giây nữa thôi, ghì chặt bờ môi xiết ôm, đừng vội vàng quên
                        Rời xa anh mãi
                        Xin đừng đi, anh cần em nhớ em
                        Lá vàng rơi xuân hạ tới, đông và thu nhớ em

                        [Ver rap:]
                        Anh có thể vẽ em thật kiêu sa, nét ngọc ngà trên áo còn thêu hoa
                        Phút chạnh lòng anh cứ tưởng là khi xa, sẽ không buồn với những thứ mình đi qua
                        Em ơi! Thanh xuân này ngắn ngủi
                        Đôi giấy nhỏ làm sao viết thành văn, tơ vò còn vương lại khe núi, ôm cả bầu trời niên thiếu có đành chăng?
                        Em biết không mùa xuân chẳng trọn vẹn, tim lạc đường khi ta chẳng thấy nhau
                        Nhìn trăng kia vàng còn treo trước đầu ngõ, giờ em đi rồi mùa hạ hôm ấy đâu?
                        Anh chẳng ước, mình như là cánh chim, bay giữa muôn trùng đất mẹ này bao la
                        Anh muốn được nghe giọng em nói muốn thấy em cười bình yên, chẳng sao cả, nghe anh

                        [Mel 2:]
                        Về bên anh nhé em! Cầm tay anh nhé em, cùng bên nhau và sưởi ấm đêm đông
                        Buốt giá
                        Lòng anh thương nhớ em, chìm vào trong giấc mơ
                        Để con tim, một lần nữa kêu tên

                        Vì đôi lúc anh thấy em giận anh quá nên thôi, hình bóng ấy có thể phai nhòa
                        Nhưng chẳng xa xôi
                        Cành phượng vĩ kia đã đâm chồi thay lá đơm bông, thì thôi nhé em cứ ở lại giây phút
                        Ta mong đưa đôi tay nhìn lên trời, hương thơm kia tựa mây ngàn
                        Anh muốn ôm bờ vai này sao chẳng thấy
                        Chợt bồi hồi vì lòng chưa quên, đoạn đường buồn vội vàng không tên
                        Tiếc nuối ấy cứ thế vẫn mãi khắc sâu trên hàng mi, và rồi nhận ra yêu thương bên nhau dần vỡ nát
                        Giọt lệ anh đã cố giấu bước tiếp để nhìn em bước đi
                        Nơi đó anh nhớ em nhiều
                        Về bên anh gió lộng đồi hoang, ở bên anh yên giấc mơ màng
                        Ngồi đây nghe tiếng lòng thở than, chờ mong ai hơi ấm nhẹ nhàng
                        """,
                        PlayCount = 0,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Pending,
                    },

                    new Song
                    {
                        SongTitle = "Tấm lòng son",
                        ArtistId = 6,
                        AlbumId = null,
                        GenreId = GetGenreId("Bolero"),
                        DurationSeconds = 250,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/327f2c02-0c54-462f-ba30-903d24470257_T%E1%BA%A5m_L%C3%B2ng_Son_(Remix).mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/56c42660-3973-4d78-b3b3-6e5485e035cb_1648256471185_300.jpg",
                        Lyrics = """
                        [00:00.82]Giọt buồn vương trên màu mắt ai
                        [00:02.17]Phía xa xăm có tiếng thở dài
                        [00:04.12]Khắc sâu mối tình không phôi phai
                        [00:05.83]Tiếc cho duyên phận mình ngang trái
                        [00:07.44]Từng chiều cô liêu buồn hắt hiu
                        [00:09.13]Lẻ loi chỉ tôi với cánh diều
                        [00:11.09]Tháng năm cứ ngày chờ đêm trông
                        [00:12.79]Nhưng sao chẳng chút hi vọng
                        [00:14.39]Thuyền giờ sang sông bỏ lại bến xưa
                        [00:16.26]Tiếng ai đang nấc sau ô cửa
                        [00:17.96]Thả trôi nỗi niềm trong cơn mưa
                        [00:19.64]Cánh hoa xưa giờ còn đâu nữa
                        [00:21.34]Hẹn lại lương duyên ở kiếp sau
                        [00:22.94]Sẽ tay nắm tay đến khi bạc đầu
                        [00:24.82]Dẫu cho có ngàn vạn thương đau
                        [00:26.67]Cũng không đành mất nhau
                        [00:29.79]
                        [00:56.78]Thương cho tấm thân cơ hàn, ngậm ngùi lặng nhìn con đò sang ngang
                        [00:59.90]Mộng vàng nay hoá tro tàn khi trên xe hoa ai đón đưa nàng
                        [01:03.64]Nhân duyên từ đây đứt đoạn, để lại một mình thân gầy héo hon
                        [01:07.10]Cuộc tình năm xưa chẳng còn, nhưng sao em mãi giữ tấm lòng son
                        [01:11.17]Hà cớ chi để ta phải xa cách rời
                        [01:14.06]Thiên ý trêu ngươi, cho ta phận duyên đôi lứa đôi nơi
                        [01:17.97]Lời hẹn ước xưa, giờ đây cũng sẽ hoá thừa
                        [01:21.07]Mơ ước bên nhau, đành thôi gửi trao người sau đón đưa
                        [01:24.03]Giọt buồn vương trên màu mắt, mắt
                        [01:27.50]Mắt, mắt, mắt, mắt-mắt, mắt
                        [01:34.63]Mắt, mắt, mắt, mắt-mắt, mắt
                        [01:41.50]Mắt, mắt, mắt, mắt-mắt, mắt
                        [01:48.32](Mắt) mơ ước bên nhau, đành thôi gửi trao người sau đón đưa
                        [01:51.93]Giọt buồn vương trên màu mắt ai
                        [01:53.64]Phía xa xăm có tiếng thở dài
                        [01:55.34]Khắc sâu mối tình không phôi phai
                        [01:56.91]Tiếc cho duyên phận mình ngang trái
                        [01:58.76]Từng chiều cô liêu buồn hắt hiu
                        [02:00.58]Lẻ loi chỉ tôi với cánh diều
                        [02:02.36]Tháng năm cứ ngày chờ đêm trông
                        [02:03.99]Nhưng sao chẳng chút hi vọng
                        [02:05.67]Thuyền giờ sang sông bỏ lại bến xưa
                        [02:07.35]Tiếng ai đang nấc sau ô cửa
                        [02:09.28]Thả trôi nỗi niềm trong cơn mưa
                        [02:11.04]Cánh hoa xưa giờ còn đâu nữa
                        [02:12.52]Hẹn lại lương duyên ở kiếp sau
                        [02:14.32]Sẽ tay nắm tay đến khi bạc đầu
                        [02:16.28]Dẫu cho có ngàn vạn thương đau
                        [02:17.97]Cũng không đành mất nhau (au-au-au-au-au)
                        [02:22.44]Bến đò này đợi ngóng trông, đón đưa em đi theo chồng
                        [02:24.16]Anh không thể phụ lòng này, một lần nhỏ nhoi không đổi thay
                        [02:25.96]Sầu này ta nên biết, không để lạc mất nhau
                        [02:27.64]Vơi sầu với tấm bi hài như ánh lửa vàng đốt tro thiên thai
                        [02:29.30]Kiếp này không thân, cũng không duyên, cũng không phận
                        [02:30.96]Vi vu nàng đón hạnh phúc, chúc em bái đường thành thân
                        [02:32.61]Khóc làm gì nữa cô ơi, em sắp xa vùng làng quê
                        [02:34.68]Bước chân tới thành thị, em cùng tình nghĩa đàn phu thê
                        [02:36.14]Yêu nhau khổ làm chi, để giờ xa cách được biệt từ
                        [02:37.88]Chỉ biết cúi đầu trách do bản thân không đọc thấu hiểu dòng tâm thư
                        [02:39.61]Không nên gặp, cũng đã gặp, không nên thương, cũng đã thương
                        [02:41.45]Vì em đã không thương anh nên bây giờ phải xa lánh
                        [02:43.32]Mong ngày đó quay lại đừng xảy ra
                        [02:44.71]Đừng giữ cô chặt đến tới lúc này, hãy để tâm hồn này thoát ra
                        [02:46.86]Bao nhiêu niềm vui, bây giờ trở thành niềm tin
                        [02:48.46]Là vì do thân phận mình không giấu thật, chỉ giấu kín
                        [02:49.65]Nơi kia có ngờ, nơi đây cứ chờ
                        [02:51.31]Dẫu cho chẳng trọn vẹn bao nhiêu ý thơ
                        [02:53.27]Mông lung đứt đoạn dây tơ, vương bao giấc mơ (ờ-ơ)
                        [02:56.62]Ai đưa chuyến đò xuôi theo cánh cò
                        [02:58.29]Đến nơi giàu sang nhung gấm hoa
                        [03:00.13]Riêng tôi ở lại, ôm thương nhớ hoài hình dung chẳng phai
                        [03:03.12]Giọt buồn vương trên màu mắt ai
                        [03:04.87]Phía xa xăm có tiếng thở dài
                        [03:06.79]Khắc sâu mối tình không phôi phai
                        [03:08.30]Tiếc cho duyên phận mình ngang trái
                        [03:10.01]Từng chiều cô liêu buồn hắt hiu
                        [03:12.02]Lẻ loi chỉ tôi với cánh diều
                        [03:13.52]Tháng năm cứ ngày chờ đêm trông
                        [03:15.20]Nhưng sao chẳng chút hi vọng
                        [03:17.09]Thuyền giờ sang sông bỏ lại bến xưa
                        [03:18.86]Tiếng ai đang nấc sau ô cửa
                        [03:20.66]Thả trôi nỗi niềm trong cơn mưa
                        [03:22.24]Cánh hoa xưa giờ còn đâu nữa
                        [03:23.81]Hẹn lại lương duyên ở kiếp sau
                        [03:25.77]Sẽ tay nắm tay đến khi bạc đầu
                        [03:27.51]Dẫu cho có ngàn vạn thương đau
                        [03:29.12]Cũng không đành mất nhau
                        [03:31.44]Khoé mi ướt đẫm, lệ nhoà ngày em rời xa quên mấy câu hò
                        [03:34.94]Nghiêng nghiêng con nước xuôi dòng, ở bên kia sông ai đang ngóng trông
                        [03:38.31]Quên bao câu hứa câu thề, giờ đây mỗi anh đau đớn ê chề
                        [03:41.85]Thanh xuân như áng mây trời, vậy nên chẳng ai muốn đợi
                        [03:44.88]Thuyền giờ sang sông bỏ lại bến xưa
                        [03:46.63]Tiếng ai đang nấc sau ô cửa
                        [03:48.26]Thả trôi nỗi niềm trong cơn mưa
                        [03:50.15]Cánh hoa xưa giờ còn đâu nữa
                        [03:51.79]Hẹn lại lương duyên ở kiếp sau
                        [03:53.62]Sẽ tay nắm tay đến khi bạc đầu
                        [03:55.26]Dẫu cho có ngàn vạn thương đau
                        [03:56.97]Cũng không đành mất nhau (au-au-au-au-au)
                        [04:00.15]
                        """,
                        PlayCount = 1222,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Bạc phận",
                        ArtistId = 4,
                        AlbumId = null,
                        GenreId = GetGenreId("Rap/Hip-hop"),
                        DurationSeconds = 249,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/4d20c1f9-55e0-4d41-a4d3-6f563558b5ff_B%E1%BA%A1c_Ph%E1%BA%ADn.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/114794ec-f130-4d8b-8e23-ca2bfdc97e37_1683540865390_300.jpg",
                        Lyrics = """
                        J-A-C-K
                        K-I-C-M, hallow
                        Take it, one, two, let's get it!
                        Ai gieo tình này, ai mang tình này để lệ trên khóe mi cay
                        Ai đưa về nhà, ai cho ngọc ngà giờ người xa cách ta
                        Từng là một thời thiếu nữ trong vùng quê nghèo
                        Hồn nhiên cài hoa mái đầu
                        Dòng người vội vàng em hóa thân đời bẽ bàng
                        Rời xa tình anh năm tháng
                        Ôi phút giây tương phùng anh nhớ và mong
                        Dòng lưu bút năm xưa in dấu mãi đậm sâu
                        Trong nỗi đau anh mệt nhoài, trong phút giây anh tìm hoài
                        Muốn giữ em ở lại một lần này vì anh mãi thương
                        Xa cách nhau thật rồi, sương trắng chiều thu
                        Ngày em bước ra đi, nước mắt ấy biệt ly
                        Hoa vẫn rơi bên thềm nhà, lá xát xơ đi nhiều và
                        Anh chúc em yên bình, mối tình mình, hẹn em kiếp sau
                        Thoáng thoáng, ngày miên man
                        Giờ con nước dài thênh thang
                        Không trách người không thương
                        Mà hương tóc còn vương vương
                        Gửi tặng em màu son cỏ dại, chút bình yên trên môi bỏ lại
                        Nước mắt nào thấm đẫm cả hai vai
                        Mắt phượng mày ngài, mình phải tìm đến thiên thai
                        À ơi câu hát, em không cần những lời khuyên
                        Em buông thả mình và chẳng màng đến tình duyên
                        Đời em phiêu bạc, đau đớn lắm lúc cũng vì tiền
                        Thương thân em khổ để một lần cùng chí tuyến
                        Giờ em ở nơi khuê phòng, ngày mai nữa em theo chồng
                        Và tô má em thêm hồng, ôi đớn đau lòng, ôi đớn đau lòng
                        Bình minh dẫn em đi rồi, vòng xoay bánh xe luân hồi
                        Hoàng hôn khuất sau lưng đồi, ôi vỡ tan rồi, ôi vỡ tan rồi
                        Một ngày buồn mây tím, em về thôn làng
                        Mẹ cha của em vỡ òa
                        Giọt lệ chạnh lòng em khóc, thương người sang đò
                        Hồng nhan bạc phận sóng gió!
                        Ôi phút giây tương phùng anh nhớ và mong
                        Dòng lưu bút năm xưa in dấu mãi đậm sâu
                        Trong nỗi đau anh mệt nhoài, trong phút giây anh tìm hoài
                        Muốn giữ em ở lại một lần này vì anh mãi thương
                        Xa cách nhau thật rồi, sương trắng chiều thu
                        Ngày em bước ra đi, nước mắt ấy biệt ly
                        Hoa vẫn rơi bên thềm nhà, lá xát xơ đi nhiều và
                        Anh chúc em yên bình, mối tình mình, hẹn em kiếp sau
                        Em ở nơi khuê phòng, mai nữa em theo chồng
                        Tô má em thêm hồng, ôi đớn đau lòng, ôi đớn đau lòng
                        Bình minh dẫn em đi rồi, vòng xoay bánh xe luân hồi
                        Hoàng hôn khuất sau lưng đồi, ôi vỡ tan rồi, ôi vỡ tan rồi
                        Xa cách nhau thật rồi, sương trắng chiều thu
                        Ngày em bước ra đi, nước mắt ấy biệt ly
                        Hoa vẫn rơi bên thềm nhà, lá xát xơ đi nhiều và
                        Anh chúc em yên bình, mối tình mình, hẹn em kiếp sau
                        """,    
                        PlayCount = 222,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Từng là",
                        ArtistId = 5,
                        AlbumId = null,
                        GenreId = GetGenreId("Indie"),
                        DurationSeconds = 242,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/616d84b4-020e-4114-beb5-446de559b3dd_(t%E1%BB%ABng_l%C3%A0)_Boyfriend%2C_Girlfriend.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/b72b45e9-1add-4c32-a149-b46d2e6b7bfc_1747058051125_300.jpg",
                        Lyrics = """
                        [00:07.28]Anh có còn nhớ mình từng thâu đêm?
                        [00:11.36]Hai, ba giờ sáng phố vẫn lên đèn
                        [00:16.24]Dịu dàng vòng tay của anh khẽ ôm em
                        [00:23.60]Em hay thầm ước mình được bên nhau
                        [00:27.63]Như câu chuyện bé thơ bao nhiệm màu
                        [00:32.46]Và rồi họ sống hạnh phúc đến mãi sau
                        [00:35.40]
                        [00:37.28]Điều gì phải đến đã đến
                        [00:39.33]Một chuyện tình không thể quên
                        [00:41.38]Từng là chàng trai của em
                        [00:43.40]Từng là cô gái của anh
                        [00:45.42]Hôm nay em sẽ không đau buồn
                        [00:47.70]Ngày mai sẽ luôn mỉm cười
                        [00:49.74]Mỗi khi em quay lại nhìn và nói
                        [00:54.06]Cảm ơn đã từng là boyfriend, girlfriend
                        [00:58.12]Cảm ơn đã từng là boyfriend, girlfriend
                        [01:02.20]Cảm ơn đã từng là boyfriend, girlfriend
                        [01:06.28]Cảm ơn đã từng là điều đẹp nhất ở trong đời em
                        [01:11.63]
                        [01:28.63]Em đang học cách để bình yên hơn
                        [01:32.68]Những thú vui mới em có thể làm một mình
                        [01:37.53]Thật khó vì ta từng như bóng với hình
                        [01:42.40]Đôi khi em nghe mùi nước hoa vẫn còn đượm lại trên gối
                        [01:46.70]Khi em soi mình trước gương, không còn ai nhìn em đắm đuối
                        [01:50.80]Nhớ không anh yêu ơi?
                        [01:53.84]Còn em thì không thể quên đến bây giờ
                        [01:56.58]
                        [01:58.63]Điều gì phải đến đã đến
                        [02:00.66]Một chuyện tình không thể quên
                        [02:02.68]Từng là chàng trai của em
                        [02:04.72]Từng là cô gái của anh
                        [02:06.78]Hôm nay em sẽ không đau buồn
                        [02:09.04]Ngày mai sẽ luôn mỉm cười
                        [02:11.06]Mỗi khi em quay lại nhìn và nói
                        [02:15.43]Cảm ơn đã từng là boyfriend, girlfriend
                        [02:19.50]Cảm ơn đã từng là boyfriend, girlfriend
                        [02:23.56]Cảm ơn đã từng là boyfriend, girlfriend
                        [02:27.63]Cảm ơn đã từng là
                        [02:29.43]
                        [02:31.18]Là niềm tự hào của em, là cả cuộc đời của em
                        [02:35.48]Từng là điều phải nhớ, giờ lại là điều chẳng thể quên
                        [02:39.58]Và dù em đã bật khóc từng đêm mỗi khi một mình
                        [02:46.66]Rồi ngày mai sẽ tốt hơn
                        [02:49.18]
                        [02:51.53]Điều gì phải đến đã đến
                        [02:53.53]Một chuyện tình không thể quên
                        [02:55.58]Từng là chàng trai của em
                        [02:57.63]Từng là cô gái của anh
                        [02:59.64]Hôm nay em sẽ không đau buồn
                        [03:01.93]Ngày mai sẽ luôn mỉm cười
                        [03:03.96]Mỗi khi em quay lại nhìn và nói
                        [03:08.30]Cảm ơn đã từng là boyfriend, girlfriend
                        [03:12.38]Cảm ơn đã từng là boyfriend, girlfriend
                        [03:16.46]Cảm ơn đã từng là boyfriend, girlfriend
                        [03:20.53]Cảm ơn đã từng là điều đẹp nhất ở trong đời em
                        [03:26.08]Boyfriend, girlfriend
                        [03:30.16]Boyfriend, girlfriend
                        [03:34.23]Boyfriend, girlfriend
                        [03:38.30]Boyfriend, girlfriend
                        [03:42.36]Boyfriend, girlfriend
                        [03:46.43]Boyfriend, girlfriend
                        [03:50.48]Boyfriend, girlfriend
                        [03:54.56]Boyfriend, girlfriend
                        [03:56.71]
                        
                        """,
                        PlayCount = 10000,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Người đầu tiên",
                        ArtistId = 7,
                        AlbumId = null,
                        GenreId = GetGenreId("R&B"),
                        DurationSeconds = 215,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/9c34473a-cc59-40a8-aa38-0c63cbe29d16_Ng%C6%B0%E1%BB%9Di_%C4%90%E1%BA%A7u_Ti%C3%AAn.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/85b4cde5-b591-4d54-9cfc-58f0b0f9242b_1761622345473_300.jpg",
                        Lyrics = """
                        Là người đầu tiên cầm tay
                        Là người đầu tiên ôm em dưới bầu trời này
                        Là người đầu tiên chạm môi
                        Là người đầu tiên em dâng cả cuộc đời
                        Chỉ có anh làm trái tim em biết đập nồng nàn
                        Nỗi đau hóa tiếng cười rộn ràng
                        Mọi lo toan cứ để anh mang
                        Hay là em đi theo anh đến nơi
                        Nơi mà ta không chia đôi quãng đời
                        Nơi mà có anh cười như năm tháng đôi mươi
                        Nơi mà bão giông đều tan vì có nhau rồi
                        Hay để em đi theo anh cho rồi
                        Ly biệt tội lắm, em đau hết đời
                        Vắng người, đời em chỉ còn bóng tối vây quanh
                        Làm ơn đừng bỏ em giữa cuộc đời hiu quạnh

                        Những câu yêu thương ngọt ngào anh dành cho riêng mỗi em
                        Vẫn luôn dịu dàng như lời mình hứa với nhau
                        Anh muốn mang hết những khoảnh khắc anh nói yêu em
                        Để anh mong nhớ và để em thôi khóc mỗi đêm
                        Muốn em thôi gọi tên và cũng muốn cho em yêu đừng quên mình
                        Ánh dương đang dần lên là nơi bình yên trái tim em thuộc về
                        Chỉ là một người còn trong quá khứ
                        Mong rằng em luôn thật sự hạnh phúc

                        Hay là em đi theo anh đến nơi
                        Nơi mà ta không chia đôi quãng đời
                        Nơi mà có anh cười như năm tháng đôi mươi
                        Nơi mà bão giông đều tan vì có nhau rồi
                        Hay để em đi theo anh cho rồi
                        Ly biệt tội lắm, em đau hết đời
                        Vắng người, đời em chỉ còn bóng tối vây quanh
                        Làm ơn đừng bỏ em giữa cuộc đời hiu quạnh
                        Em đâu cần gì ngoài người em yêu
                        Ấm áp nơi anh làm sao có thể thiếu
                        Nước mắt giờ này chẳng còn bao nhiêu
                        Thời gian của hai ta giờ hoá tro tàn
                        Hah-hah-hah-hah-hah
                        Hah-hah-hah-hah-hah
                        Hah-hah-hah-hah
                        Hah, hah, hah, hah
                        Hah-hah-hah-hah-hah
                        Hah-hah-hah-hah-hah
                        Hah-hah-hah-hah
                        Hah, hah, hah, hah (hah-hah-hah-hah-hah)
                        Hah-hah-hah-hah-hah
                        Cớ sao ông trời (hah-hah-hah-hah)
                        Lấy đi cả thế giới? (Hah, hah, hah, hah)

                        Em đừng trông mong theo anh đến nơi
                        Em còn thanh xuân còn cả quãng đời
                        Anh được ngắm em cười trong năm tháng đôi mươi
                        Đã được nói ra lời yêu anh sẽ mang theo suốt đời

                        Là người đầu tiên cầm tay
                        Là người đầu tiên ôm em dưới bầu trời này
                        Là người đầu tiên chạm môi
                        Là người đầu tiên em dâng cả cuộc đời
                        """,
                        PlayCount = 1000,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },

                    new Song
                    {
                        SongTitle = "Chẳng phải tình đầu sao đau đến thế",
                        ArtistId = 5,
                        AlbumId = null,
                        GenreId = GetGenreId("Ballad"),
                        DurationSeconds = 283,
                        ReleaseDate = new DateTime(2018, 5, 22),
                        AudioFileUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/b72b1bf5-f40b-453d-9e70-52f7aee1335d_ch%E1%BA%B3ng_ph%E1%BA%A3i_t%C3%ACnh_%C4%91%E1%BA%A7u_sao_%C4%91au_%C4%91%E1%BA%BFn_th%E1%BA%BF.mp3",
                        CoverImageUrl = "https://pub-1f82c8bee952472d9a067c2eb0ef8d5e.r2.dev/images/952a23a2-5f7f-4ade-afc9-751c81bd2845_1761146738378_300.jpg",
                        Lyrics = """
                        [00:01.00]Chẳng phải tình đầu sao đau đến thế?
                        [00:04.75]Tại sao vẫn yêu si mê?
                        [00:07.85]Tại sao trái tim vẫn ngô nghê, vẫn dành trọn lòng mình như thế?
                        [00:14.78]Phải chăng là vì lá rơi?
                        [00:18.01]Phải chăng là vì trái tim yêu rơi vào người?
                        [00:22.62]Phải chăng là vì mưa rơi, tình cảm này hoá biển khơi?
                        [00:27.55]
                        [00:30.00]Nếu có lúc chúng ta đau nhói vì quá thiết tha
                        [00:36.64]Lúc ấy mới biết ra, đã yêu quá nhiều
                        [00:43.01]Tình yêu là một bài học lớn
                        [00:46.46]Đâu phải là ta cứ yêu hơn
                        [00:49.74]Là sẽ bên nhau hoài, là sẽ yên tâm hoài, là sẽ không ai rời đi
                        [00:56.42]Tình yêu đâu có đúng sai
                        [00:59.72]Làm sao có thể trách ai?
                        [01:03.30]Vốn dĩ chẳng có ai bên mình được mãi
                        [01:09.78]Chẳng qua là vì ngày hôm đó
                        [01:13.06]Trời mưa phùn rơi, gió đông về
                        [01:16.42]Làm trái tim rung động, falling into you
                        [01:20.60]
                        [01:20.98]Chẳng phải tình đầu sao đau đến thế?
                        [01:24.74]Tại sao vẫn yêu si mê?
                        [01:27.86]Tại sao trái tim vẫn ngô nghê, vẫn dành trọn lòng mình như thế?
                        [01:34.72]Phải chăng là vì lá rơi?
                        [01:38.10]Phải chăng là vì trái tim yêu rơi vào người?
                        [01:42.66]Phải chăng là vì mưa rơi, tình cảm này hoá biển khơi
                        [01:47.70]Chẳng phải tình đầu sao đau đến thế?
                        [01:51.22]Chỉ một tin nhắn vang lặng lẽ
                        [01:54.34]Từ giờ phải sống tốt nhé, em à
                        [01:57.44]Đường phải đi còn dài và xa
                        [02:01.42]Cảm ơn vì mình đã yêu
                        [02:04.75]Và yêu đến khi không thể yêu tiếp được nữa
                        [02:09.14]Cám ơn vì đã dám buông tay, không níu giữ giấc mộng này
                        [02:14.40]
                        [02:16.00]Lả Lướt
                        [02:16.82]Chia tay cũng đâu phải là lần đầu mà sao không quen?
                        [02:20.60]Đêm đen chỉ còn vài ngọn đèn mà lòng bon chen
                        [02:23.86]Ta đã có chặng đường thật đẹp mà phải không em?
                        [02:26.00]Dù chẳng thể quên ngay bao vấn vương
                        [02:27.70]Kệ dòng lệ tuôn, hôm nay phải buông thật rồi
                        [02:30.55]Đâu ai hay sau ánh mắt dịu dàng là toàn thương đau
                        [02:33.88]Buông câu chia tay nhẹ nhàng mình dành cho nhau
                        [02:37.20]Thất hứa vì chẳng thể gọi nàng là cô dâu
                        [02:39.40]Mà sao vết cứa lại càng đâm sâu hơn tình đầu?
                        [02:42.25]Thà vụt tan như mây
                        [02:44.72]Bàn tay anh chẳng thể ôm lấy
                        [02:48.10]Thời gian làm nhoè đi hết
                        [02:51.45]Những rung động ngày hôm ấy, bên nhau
                        [02:56.42]Anh biết, không cố níu giữ em giờ thì suốt đời sẽ đánh mất
                        [03:02.44]Vì ôm bao hoài bão nên thôi đành phải cất em về kí ức
                        [03:07.12]
                        [03:07.65]Chẳng phải tình đầu sao đau đến thế?
                        [03:11.40]Tại sao vẫn yêu si mê?
                        [03:14.55]Tại sao trái tim vẫn ngô nghê, vẫn dành trọn lòng mình như thế?
                        [03:21.40]Phải chăng là vì lá rơi?
                        [03:24.75]Phải chăng là vì trái tim yêu rơi vào người?
                        [03:29.34]Phải chăng là vì mưa rơi, tình cảm này hoá biển khơi
                        [03:34.34]Chẳng phải tình đầu sao đau đến thế?
                        [03:37.88]Chỉ một tin nhắn vang lặng lẽ
                        [03:41.00]Từ giờ phải sống tốt nhé, em à
                        [03:44.12]Đường phải đi còn dài và xa
                        [03:48.08]Cảm ơn vì mình đã yêu
                        [03:51.40]Và yêu đến khi không thể yêu tiếp được nữa
                        [03:55.74]Cám ơn vì đã dám buông tay, không níu giữ giấc mộng này
                        [04:01.10]
                        [04:26.65]Tình đầu lúc không thành có đau như vậy?
                        [04:31.20]
                        """,
                        PlayCount = 7000,
                        LikeCount = 0,
                        CreatedAt = DateTime.UtcNow,
                        ApprovalStatus = ApprovalStatus.Approved,
                    },
                };
               
                context.Songs.AddRange(songs);
                context.SaveChanges();
            }

            if (!context.Playlists.Any())
            {
                var playlists = new List<Playlist>
                {
                    new Playlist { UserId = 2, PlaylistName = "Top V-Pop", Description = "Những bản V-Pop đình đám", CoverImageUrl = "images/playlists/1.jpg", IsPublic = true },
                    new Playlist { UserId = 3, PlaylistName = "Rap Life", Description = "Những bản rap truyền cảm hứng", CoverImageUrl = "images/playlists/2.jpg", IsPublic = true },
                    new Playlist { UserId = 4, PlaylistName = "Chill Night", Description = "Nhạc chill nhẹ nhàng cho buổi tối", CoverImageUrl = "images/playlists/3.jpg", IsPublic = true },
                    new Playlist { UserId = 5, PlaylistName = "Road Trip Mix", Description = "Nhạc cho những chuyến đi xa", CoverImageUrl = "images/playlists/meov2.jpg", IsPublic = false },
                    new Playlist { UserId = 6, PlaylistName = "Indie Gems", Description = "Các ca khúc indie yêu thích", CoverImageUrl = "images/playlists/meov2.jpg", IsPublic = true }
                };
                context.Playlists.AddRange(playlists);
                context.SaveChanges();
            }

            if (!context.PlaylistSongs.Any())
            {
                var playlistLookup = context.Playlists.ToDictionary(p => p.PlaylistName, p => p.PlaylistId);
                var songLookup = context.Songs.ToDictionary(s => s.SongTitle, s => s.SongId);

                var playlistSongs = new List<PlaylistSong>
                {
                    new PlaylistSong { PlaylistId = playlistLookup["Top V-Pop"], SongId = songLookup["Chạy ngay đi"], AddedAt = DateTime.UtcNow },
                    new PlaylistSong { PlaylistId = playlistLookup["Top V-Pop"], SongId = songLookup["Hãy trao cho anh"], AddedAt = DateTime.UtcNow },
                    new PlaylistSong { PlaylistId = playlistLookup["Rap Life"], SongId = songLookup["Miền đất hứa"], AddedAt = DateTime.UtcNow },
                    new PlaylistSong { PlaylistId = playlistLookup["Chill Night"], SongId = songLookup["Nơi này có anh"], AddedAt = DateTime.UtcNow },
                    new PlaylistSong { PlaylistId = playlistLookup["Road Trip Mix"], SongId = songLookup["Sóng gió"], AddedAt = DateTime.UtcNow },
                    new PlaylistSong { PlaylistId = playlistLookup["Indie Gems"], SongId = songLookup["Một ngày nào đó"], AddedAt = DateTime.UtcNow },
                    new PlaylistSong { PlaylistId = playlistLookup["Chill Night"], SongId = songLookup["Mở mắt"], AddedAt = DateTime.UtcNow }
                };

                context.PlaylistSongs.AddRange(playlistSongs);
                context.SaveChanges();
            }

            if (!context.ListeningHistories.Any())
            {
                var songLookup = context.Songs.ToDictionary(s => s.SongTitle, s => s.SongId);
                var listeningHistories = new List<ListeningHistory>
                {
                    new ListeningHistory { UserId = 2, SongId = songLookup["Chạy ngay đi"], PlayedAt = DateTime.UtcNow.AddHours(-2), DurationPlayed = 240, Completed = true },
                    new ListeningHistory { UserId = 3, SongId = songLookup["Miền đất hứa"], PlayedAt = DateTime.UtcNow.AddHours(-4), DurationPlayed = 200, Completed = true },
                    new ListeningHistory { UserId = 4, SongId = songLookup["Nơi này có anh"], PlayedAt = DateTime.UtcNow.AddDays(-1), DurationPlayed = 210, Completed = true },
                    new ListeningHistory { UserId = 5, SongId = songLookup["Sóng gió"], PlayedAt = DateTime.UtcNow.AddHours(-6), DurationPlayed = 225, Completed = true },
                    new ListeningHistory { UserId = 6, SongId = songLookup["Một ngày nào đó"], PlayedAt = DateTime.UtcNow.AddHours(-8), DurationPlayed = 215, Completed = false },
                    new ListeningHistory { UserId = 3, SongId = songLookup["Mở mắt"], PlayedAt = DateTime.UtcNow.AddHours(-3), DurationPlayed = 150, Completed = true }
                };

                context.ListeningHistories.AddRange(listeningHistories);
                context.SaveChanges();
            }
           
        }
    }
}

