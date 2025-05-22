using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMSProjectAPI.Database;
using RMSProjectAPI.Database.Entity;
using RMSProjectAPI.Model;
using System.Security.Claims;

namespace RMSProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PostController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Post
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Media)
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .ToListAsync();

            // Map to DTOs
            var postDtos = posts.Select(p => new PostResponseDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Content = p.Content,
                Category = p.Category,
                SubCategory = p.SubCategory,
                Price = p.Price,
                IsPinned = p.IsPinned,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                User = new UserBasicInfoDto
                {
                    Id = p.User.Id,
                    UserName = p.User.UserName,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    ProfilePictureUrl = p.User.ProfilePictureUrl
                },
                Media = p.Media.Select(m => new MediaDto
                {
                    Id = m.Id,
                    PostId = m.PostId,
                    Type = m.Type,
                    Url = m.Url,
                    Name = m.Name,
                    Size = m.Size
                }).ToList(),
                CommentsCount = p.Comments?.Count ?? 0,
                ReactionsCount = p.Reactions?.Count ?? 0
            }).ToList();

            return Ok(postDtos);
        }

        // GET: api/Post/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostResponseDto>> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Media)
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Map to DTO
            var postDto = new PostResponseDto
            {
                Id = post.Id,
                UserId = post.UserId,
                Content = post.Content,
                Category = post.Category,
                SubCategory = post.SubCategory,
                Price = post.Price,
                IsPinned = post.IsPinned,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                User = new UserBasicInfoDto
                {
                    Id = post.User.Id,
                    UserName = post.User.UserName,
                    FirstName = post.User.FirstName,
                    LastName = post.User.LastName,
                    ProfilePictureUrl = post.User.ProfilePictureUrl
                },
                Media = post.Media.Select(m => new MediaDto
                {
                    Id = m.Id,
                    PostId = m.PostId,
                    Type = m.Type,
                    Url = m.Url,
                    Name = m.Name,
                    Size = m.Size
                }).ToList(),
                CommentsCount = post.Comments?.Count ?? 0,
                ReactionsCount = post.Reactions?.Count ?? 0
            };

            return Ok(postDto);
        }

        // GET: api/Post/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPostsByUser(Guid userId)
        {
            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .Include(p => p.Media)
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .ToListAsync();

            // Map to DTOs
            var postDtos = posts.Select(p => new PostResponseDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Content = p.Content,
                Category = p.Category,
                SubCategory = p.SubCategory,
                Price = p.Price,
                IsPinned = p.IsPinned,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                User = new UserBasicInfoDto
                {
                    Id = p.User.Id,
                    UserName = p.User.UserName,
                    FirstName = p.User.FirstName,
                    LastName = p.User.LastName,
                    ProfilePictureUrl = p.User.ProfilePictureUrl
                },
                Media = p.Media.Select(m => new MediaDto
                {
                    Id = m.Id,
                    PostId = m.PostId,
                    Type = m.Type,
                    Url = m.Url,
                    Name = m.Name,
                    Size = m.Size
                }).ToList(),
                CommentsCount = p.Comments?.Count ?? 0,
                ReactionsCount = p.Reactions?.Count ?? 0
            }).ToList();

            return Ok(postDtos);
        }

        // POST: api/Post
        [HttpPost]
       // [Authorize(Roles = "marketer")]
        public async Task<ActionResult<PostResponseDto>> CreatePost(PostDto postDto)
        {
            // Get the current user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Map DTO to entity
            var post = new Post
            {
                UserId = Guid.Parse(userId),
                Content = postDto.Content,
                Category = postDto.Category,
                SubCategory = postDto.SubCategory,
                Price = postDto.Price,
                IsPinned = postDto.IsPinned,
                CreatedAt = postDto.Date ?? DateTime.UtcNow
            };

            // Add post to database
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Get the user info for the response
            var user = await _context.Users.FindAsync(Guid.Parse(userId));

            // Create response DTO
            var responseDto = new PostResponseDto
            {
                Id = post.Id,
                UserId = post.UserId,
                Content = post.Content,
                Category = post.Category,
                SubCategory = post.SubCategory,
                Price = post.Price,
                IsPinned = post.IsPinned,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                User = new UserBasicInfoDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfilePictureUrl = user.ProfilePictureUrl
                },
                Media = new List<MediaDto>(),
                CommentsCount = 0,
                ReactionsCount = 0
            };

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, responseDto);
        }

        // PUT: api/Post/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(int id, PostDto postDto)
        {
            if (id != postDto.Id)
            {
                return BadRequest();
            }

            // Get the current user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Check if the post belongs to the current user
            var existingPost = await _context.Posts.FindAsync(id);
            if (existingPost == null)
            {
                return NotFound();
            }

            if (existingPost.UserId != Guid.Parse(userId))
            {
                return Forbid();
            }

            // Update only allowed fields
            existingPost.Content = postDto.Content;
            existingPost.Category = postDto.Category;
            existingPost.SubCategory = postDto.SubCategory;
            existingPost.Price = postDto.Price;
            existingPost.IsPinned = postDto.IsPinned;
            existingPost.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Post/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id)
        {
            // Get the current user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // Check if the post belongs to the current user
            if (post.UserId != Guid.Parse(userId))
            {
                return Forbid();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }

        [HttpGet("GetHello")]
        public IActionResult GetOk()
        {
            return Ok("That's Ok ya man");
        }
    }
}
