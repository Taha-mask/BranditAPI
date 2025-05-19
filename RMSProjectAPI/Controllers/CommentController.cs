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
    public class CommentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Comment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetComments()
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.LikedBy)
                .Include(c => c.Replies)
                .ToListAsync();

            // Map to DTOs
            var commentDtos = comments.Select(c => new CommentResponseDto
            {
                Id = c.Id,
                PostId = c.PostId,
                UserId = c.UserId,
                Text = c.Text,
                ParentCommentId = c.ParentCommentId,
                Likes = c.Likes,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                LastEditedBy = c.LastEditedBy,
                User = new UserBasicInfoDto
                {
                    Id = c.User.Id,
                    UserName = c.User.UserName,
                    FirstName = c.User.FirstName,
                    LastName = c.User.LastName,
                    ProfilePictureUrl = c.User.ProfilePictureUrl
                },
                Replies = c.Replies?.Select(r => new CommentResponseDto
                {
                    Id = r.Id,
                    PostId = r.PostId,
                    UserId = r.UserId,
                    Text = r.Text,
                    ParentCommentId = r.ParentCommentId,
                    Likes = r.Likes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList() ?? new List<CommentResponseDto>(),
                RepliesCount = c.Replies?.Count ?? 0
            }).ToList();

            return Ok(commentDtos);
        }

        // GET: api/Comment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommentResponseDto>> GetComment(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.LikedBy)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            // Map to DTO
            var commentDto = new CommentResponseDto
            {
                Id = comment.Id,
                PostId = comment.PostId,
                UserId = comment.UserId,
                Text = comment.Text,
                ParentCommentId = comment.ParentCommentId,
                Likes = comment.Likes,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                LastEditedBy = comment.LastEditedBy,
                User = new UserBasicInfoDto
                {
                    Id = comment.User.Id,
                    UserName = comment.User.UserName,
                    FirstName = comment.User.FirstName,
                    LastName = comment.User.LastName,
                    ProfilePictureUrl = comment.User.ProfilePictureUrl
                },
                Replies = comment.Replies?.Select(r => new CommentResponseDto
                {
                    Id = r.Id,
                    PostId = r.PostId,
                    UserId = r.UserId,
                    Text = r.Text,
                    ParentCommentId = r.ParentCommentId,
                    Likes = r.Likes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    User = new UserBasicInfoDto
                    {
                        Id = r.User.Id,
                        UserName = r.User.UserName,
                        FirstName = r.User.FirstName,
                        LastName = r.User.LastName,
                        ProfilePictureUrl = r.User.ProfilePictureUrl
                    }
                }).ToList() ?? new List<CommentResponseDto>(),
                RepliesCount = comment.Replies?.Count ?? 0
            };

            return Ok(commentDto);
        }

        // GET: api/Comment/post/5
        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetCommentsByPost(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .Include(c => c.User)
                .Include(c => c.LikedBy)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .ToListAsync();

            // Map to DTOs
            var commentDtos = comments.Select(c => new CommentResponseDto
            {
                Id = c.Id,
                PostId = c.PostId,
                UserId = c.UserId,
                Text = c.Text,
                ParentCommentId = c.ParentCommentId,
                Likes = c.Likes,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                LastEditedBy = c.LastEditedBy,
                User = new UserBasicInfoDto
                {
                    Id = c.User.Id,
                    UserName = c.User.UserName,
                    FirstName = c.User.FirstName,
                    LastName = c.User.LastName,
                    ProfilePictureUrl = c.User.ProfilePictureUrl
                },
                Replies = c.Replies?.Select(r => new CommentResponseDto
                {
                    Id = r.Id,
                    PostId = r.PostId,
                    UserId = r.UserId,
                    Text = r.Text,
                    ParentCommentId = r.ParentCommentId,
                    Likes = r.Likes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    User = new UserBasicInfoDto
                    {
                        Id = r.User.Id,
                        UserName = r.User.UserName,
                        FirstName = r.User.FirstName,
                        LastName = r.User.LastName,
                        ProfilePictureUrl = r.User.ProfilePictureUrl
                    }
                }).ToList() ?? new List<CommentResponseDto>(),
                RepliesCount = c.Replies?.Count ?? 0
            }).ToList();

            return Ok(commentDtos);
        }

        // POST: api/Comment
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CommentResponseDto>> CreateComment(CommentDto commentDto)
        {
            // Get the current user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Map DTO to entity
            var comment = new Comment
            {
                PostId = commentDto.PostId,
                UserId = Guid.Parse(userId),
                Text = commentDto.Text,
                ParentCommentId = commentDto.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                Likes = 0
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Get the user info for the response
            var user = await _context.Users.FindAsync(Guid.Parse(userId));

            // Create response DTO
            var responseDto = new CommentResponseDto
            {
                Id = comment.Id,
                PostId = comment.PostId,
                UserId = comment.UserId,
                Text = comment.Text,
                ParentCommentId = comment.ParentCommentId,
                Likes = comment.Likes,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                User = new UserBasicInfoDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfilePictureUrl = user.ProfilePictureUrl
                },
                Replies = new List<CommentResponseDto>(),
                RepliesCount = 0
            };

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, responseDto);
        }

        // PUT: api/Comment/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(int id, CommentDto commentDto)
        {
            if (id != commentDto.Id)
            {
                return BadRequest();
            }

            // Get the current user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Check if the comment belongs to the current user
            var existingComment = await _context.Comments.FindAsync(id);
            if (existingComment == null)
            {
                return NotFound();
            }

            if (existingComment.UserId != Guid.Parse(userId))
            {
                return Forbid();
            }

            // Save the old content to edit history
            var editHistory = new CommentEditHistory
            {
                CommentId = existingComment.Id,
                PreviousText = existingComment.Text,
                EditedBy = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier),
                EditedAt = DateTime.UtcNow
            };
            _context.CommentEditHistory.Add(editHistory);

            // Update comment
            existingComment.Text = commentDto.Text;
            existingComment.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
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

        // DELETE: api/Comment/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            // Get the current user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Check if the comment belongs to the current user
            if (comment.UserId != Guid.Parse(userId))
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Comment/like/5
        [HttpPost("like/{id}")]
        [Authorize]
        public async Task<IActionResult> LikeComment(int id)
        {
            // Get the current user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var userGuid = Guid.Parse(userId);
            
            // Check if the user already liked this comment
            var existingLike = await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == id && cl.UserId == userGuid);
                
            if (existingLike != null)
            {
                return BadRequest("You have already liked this comment");
            }

            // Add the like
            var commentLike = new CommentLike
            {
                CommentId = id,
                UserId = userGuid,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommentLikes.Add(commentLike);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Comment/unlike/5
        [HttpDelete("unlike/{id}")]
        [Authorize]
        public async Task<IActionResult> UnlikeComment(int id)
        {
            // Get the current user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var userGuid = Guid.Parse(userId);
            
            // Find the like
            var like = await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == id && cl.UserId == userGuid);
                
            if (like == null)
            {
                return NotFound("You have not liked this comment");
            }

            // Remove the like
            _context.CommentLikes.Remove(like);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
