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
    public class UserFollowerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserFollowerController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserFollower/followers/5
        [HttpGet("followers/{userId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowers(Guid userId)
        {
            var followers = await _context.UserFollowers
                .Where(uf => uf.FollowingId == userId)
                .Include(uf => uf.Follower)
                .Select(uf => uf.Follower)
                .ToListAsync();

            return followers;
        }

        // GET: api/UserFollower/following/5
        [HttpGet("following/{userId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowing(Guid userId)
        {
            var following = await _context.UserFollowers
                .Where(uf => uf.FollowerId == userId)
                .Include(uf => uf.Following)
                .Select(uf => uf.Following)
                .ToListAsync();

            return following;
        }

        // POST: api/UserFollower/follow/5
        [HttpPost("follow/{userId}")]
        [Authorize]
        public async Task<IActionResult> FollowUser(Guid userId)
        {
            // Get the current user ID from the claims
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var followerGuid = Guid.Parse(currentUserId);
            
            // Check if the user is trying to follow themselves
            if (followerGuid == userId)
            {
                return BadRequest("You cannot follow yourself");
            }

            // Check if the user to follow exists
            var userToFollow = await _context.Users.FindAsync(userId);
            if (userToFollow == null)
            {
                return NotFound("User to follow not found");
            }

            // Check if already following
            var existingFollow = await _context.UserFollowers
                .FirstOrDefaultAsync(uf => uf.FollowerId == followerGuid && uf.FollowingId == userId);
                
            if (existingFollow != null)
            {
                return BadRequest("You are already following this user");
            }

            // Create the follow relationship
            var userFollower = new UserFollower
            {
                FollowerId = followerGuid,
                FollowingId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserFollowers.Add(userFollower);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/UserFollower/unfollow/5
        [HttpDelete("unfollow/{userId}")]
        [Authorize]
        public async Task<IActionResult> UnfollowUser(Guid userId)
        {
            // Get the current user ID from the claims
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var followerGuid = Guid.Parse(currentUserId);
            
            // Find the follow relationship
            var follow = await _context.UserFollowers
                .FirstOrDefaultAsync(uf => uf.FollowerId == followerGuid && uf.FollowingId == userId);
                
            if (follow == null)
            {
                return NotFound("You are not following this user");
            }

            // Remove the follow relationship
            _context.UserFollowers.Remove(follow);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/UserFollower/isFollowing/5
        [HttpGet("isFollowing/{userId}")]
        [Authorize]
        public async Task<ActionResult<bool>> IsFollowing(Guid userId)
        {
            // Get the current user ID from the claims
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var followerGuid = Guid.Parse(currentUserId);
            
            // Check if the follow relationship exists
            var isFollowing = await _context.UserFollowers
                .AnyAsync(uf => uf.FollowerId == followerGuid && uf.FollowingId == userId);
                
            return isFollowing;
        }

        // GET: api/UserFollower/stats/5
        [HttpGet("stats/{userId}")]
        public async Task<ActionResult<object>> GetUserFollowStats(Guid userId)
        {
            // Check if the user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Count followers and following
            var followersCount = await _context.UserFollowers
                .CountAsync(uf => uf.FollowingId == userId);
                
            var followingCount = await _context.UserFollowers
                .CountAsync(uf => uf.FollowerId == userId);
                
            return new
            {
                UserId = userId,
                FollowersCount = followersCount,
                FollowingCount = followingCount
            };
        }
    }
}
