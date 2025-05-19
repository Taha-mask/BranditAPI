using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RMSProjectAPI.Database;
using RMSProjectAPI.Database.Entity;
using System.Security.Claims;

namespace RMSProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReactionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReactionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Reaction/post/5
        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<Reaction>>> GetReactionsByPost(int postId)
        {
            return await _context.Reactions
                .Where(r => r.PostId == postId)
                .Include(r => r.User)
                .ToListAsync();
        }

        // POST: api/Reaction
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Reaction>> CreateReaction(Reaction reaction)
        {
            // Get the current user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var userGuid = Guid.Parse(userId);
            
            // Check if the user already reacted to this post
            var existingReaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.PostId == reaction.PostId && r.UserId == userGuid);
                
            if (existingReaction != null)
            {
                // Update the existing reaction if the type is different
                if (existingReaction.ReactionType != reaction.ReactionType)
                {
                    existingReaction.ReactionType = reaction.ReactionType;
                    // No UpdatedAt property, so we'll just update the existing record
                    await _context.SaveChangesAsync();
                    return Ok(existingReaction);
                }
                
                return BadRequest("You have already reacted to this post with the same reaction type");
            }

            // Add the new reaction
            reaction.UserId = userGuid;
            reaction.CreatedAt = DateTime.UtcNow;
            
            _context.Reactions.Add(reaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReactionsByPost), new { postId = reaction.PostId }, reaction);
        }

        // DELETE: api/Reaction/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReaction(int id)
        {
            // Get the current user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var reaction = await _context.Reactions.FindAsync(id);
            if (reaction == null)
            {
                return NotFound();
            }

            // Check if the reaction belongs to the current user
            if (reaction.UserId != Guid.Parse(userId))
            {
                return Forbid();
            }

            _context.Reactions.Remove(reaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Reaction/post/5
        [HttpDelete("post/{postId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReactionByPost(int postId)
        {
            // Get the current user ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var userGuid = Guid.Parse(userId);
            
            // Find the reaction
            var reaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userGuid);
                
            if (reaction == null)
            {
                return NotFound("You have not reacted to this post");
            }

            // Remove the reaction
            _context.Reactions.Remove(reaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
