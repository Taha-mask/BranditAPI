using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMSProjectAPI.Database.Entity
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(100)]
        public string SubCategory { get; set; }

        public decimal? Price { get; set; }

        public bool IsPinned { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public virtual ICollection<PostMedia> Media { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Reaction> Reactions { get; set; }
        public virtual ICollection<PostShare> Shares { get; set; }
        public virtual ICollection<PostSave> Saves { get; set; }
    }
} 