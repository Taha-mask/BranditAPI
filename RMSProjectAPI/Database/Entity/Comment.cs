using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMSProjectAPI.Database.Entity
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Text { get; set; }

        public int? ParentCommentId { get; set; }

        public int Likes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string LastEditedBy { get; set; }

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual Comment ParentComment { get; set; }

        public virtual ICollection<Comment> Replies { get; set; }
        public virtual ICollection<CommentLike> LikedBy { get; set; }
        public virtual ICollection<CommentEditHistory> EditHistory { get; set; }
    }
} 