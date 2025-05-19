using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMSProjectAPI.Database.Entity
{
    public class CommentEditHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CommentId { get; set; }

        [Required]
        [StringLength(1000)]
        public string PreviousText { get; set; }

        [Required]
        [StringLength(100)]
        public string EditedBy { get; set; }

        public DateTime EditedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; }
    }
} 