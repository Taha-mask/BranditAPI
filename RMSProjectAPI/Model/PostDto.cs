using System.ComponentModel.DataAnnotations;

namespace RMSProjectAPI.Model
{
    public class PostDto
    {
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public string Category { get; set; }
        
        public string SubCategory { get; set; }
        
        public decimal? Price { get; set; }
        
        public bool IsPinned { get; set; }
        
        public DateTime? Date { get; set; }
        
        // Media files to upload with the post
        public List<string> MediaFiles { get; set; }
    }
    
    public class PostResponseDto
    {
        public int Id { get; set; }
        
        public Guid UserId { get; set; }
        
        public string Content { get; set; }
        
        public string Category { get; set; }
        
        public string SubCategory { get; set; }
        
        public decimal? Price { get; set; }
        
        public bool IsPinned { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public UserBasicInfoDto User { get; set; }
        
        public ICollection<MediaDto> Media { get; set; }
        
        public int CommentsCount { get; set; }
        
        public int ReactionsCount { get; set; }
    }
    
    public class UserBasicInfoDto
    {
        public Guid Id { get; set; }
        
        public string UserName { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string ProfilePictureUrl { get; set; }
    }
    
    public class MediaDto
    {
        public int Id { get; set; }
        
        public int PostId { get; set; }
        
        public string Type { get; set; }
        
        public string Url { get; set; }
        
        public string Name { get; set; }
        
        public long? Size { get; set; }
    }
}
