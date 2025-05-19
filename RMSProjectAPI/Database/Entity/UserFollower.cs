using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMSProjectAPI.Database.Entity
{
    public class UserFollower
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid FollowerId { get; set; }

        [Required]
        public Guid FollowingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("FollowerId")]
        public virtual User Follower { get; set; }

        [ForeignKey("FollowingId")]
        public virtual User Following { get; set; }
    }
} 