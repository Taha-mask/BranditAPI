using System.ComponentModel.DataAnnotations;

namespace RMSProjectAPI.Model
{
    public class CommentDto
    {
        public int Id { get; set; }
        
        [Required]
        public int PostId { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Text { get; set; }
        
        public int? ParentCommentId { get; set; }
    }
    
    public class CommentResponseDto
    {
        public int Id { get; set; }
        
        public int PostId { get; set; }
        
        public Guid UserId { get; set; }
        
        public string Text { get; set; }
        
        public int? ParentCommentId { get; set; }
        
        public int Likes { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public string LastEditedBy { get; set; }
        
        public UserBasicInfoDto User { get; set; }
        
        public List<CommentResponseDto> Replies { get; set; }
        
        public int RepliesCount { get; set; }
    }
}
