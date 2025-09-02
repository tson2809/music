using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicStream.Data;
using MusicStream.Models;

namespace MusicStream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly MusicStreamContext _context;

        public GenresController(MusicStreamContext context)
        {
            _context = context;
        }

        // GET: api/Genres
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenreDto>>> GetGenres()
        {
            try
            {
                var genres = await _context.Genres
                    .OrderBy(g => g.GenreName)
                    .ToListAsync();

                var genreDtos = genres.Select(g => new GenreDto1
                {
                    GenreId = g.GenreId,
                    GenreName = g.GenreName,
                    Description = g.Description
                }).ToList();

                return Ok(genreDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // GET: api/Genres/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GenreDto>> GetGenre(int id)
        {
            try
            {
                var genre = await _context.Genres
                    .FirstOrDefaultAsync(g => g.GenreId == id);

                if (genre == null)
                {
                    return NotFound(new { message = "Không tìm thấy thể loại" });
                }

                var genreDto = new GenreDto1
                {
                    GenreId = genre.GenreId,
                    GenreName = genre.GenreName,
                    Description = genre.Description
                };

                return Ok(genreDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // POST: api/Genres
        [HttpPost]
        public async Task<ActionResult<GenreDto>> CreateGenre([FromBody] CreateGenreRequest request)
        {
            try
            {
                // Kiểm tra tên thể loại đã tồn tại
                if (await _context.Genres.AnyAsync(g => g.GenreName == request.GenreName))
                {
                    return BadRequest(new { message = "Tên thể loại đã tồn tại" });
                }

                var newGenre = new Genre
                {
                    GenreName = request.GenreName,
                    Description = request.Description
                };

                _context.Genres.Add(newGenre);
                await _context.SaveChangesAsync();

                var genreDto = new GenreDto1
                {
                    GenreId = newGenre.GenreId,
                    GenreName = newGenre.GenreName,
                    Description = newGenre.Description
                };

                return CreatedAtAction(nameof(GetGenre), new { id = newGenre.GenreId }, genreDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // PUT: api/Genres/5
        [HttpPut("{id}")]
        public async Task<ActionResult<GenreDto>> UpdateGenre(int id, [FromBody] UpdateGenreRequest request)
        {
            try
            {
                var genre = await _context.Genres.FindAsync(id);

                if (genre == null)
                {
                    return NotFound(new { message = "Không tìm thấy thể loại" });
                }

                // Kiểm tra tên thể loại đã tồn tại (trừ chính nó)
                if (await _context.Genres.AnyAsync(g => g.GenreName == request.GenreName && g.GenreId != id))
                {
                    return BadRequest(new { message = "Tên thể loại đã tồn tại" });
                }

                genre.GenreName = request.GenreName;
                genre.Description = request.Description;

                await _context.SaveChangesAsync();

                var genreDto = new GenreDto1
                {
                    GenreId = genre.GenreId,
                    GenreName = genre.GenreName,
                    Description = genre.Description
                };

                return Ok(genreDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }

        // DELETE: api/Genres/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteGenre(int id)
        {
            try
            {
                var genre = await _context.Genres.FindAsync(id);

                if (genre == null)
                {
                    return NotFound(new { message = "Không tìm thấy thể loại" });
                }

                // Hard delete - xóa vĩnh viễn
                _context.Genres.Remove(genre);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa thể loại thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
            }
        }
    }

    // DTOs
    public class GenreDto1
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class CreateGenreRequest
    {
        public string GenreName { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class UpdateGenreRequest
    {
        public string GenreName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
