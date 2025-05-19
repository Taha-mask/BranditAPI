using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMSProjectAPI.Database.Entity
{
    public class PostMedia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } // image, video, document

        [Required]
        [StringLength(255)]
        public string Url { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        public long? Size { get; set; }

        public int? CurrentPage { get; set; }

        public double? Zoom { get; set; }

        public bool IsLoading { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }
    }
} 