using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMSProjectAPI.Database.Entity
{
    public enum UserType
    {
        Customer,
        Marketer
    }

    public class User : IdentityUser<Guid>
    {
        [StringLength(100)]
        public string FirstName { get; set; }

        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(500)]
        public string Bio { get; set; }

        [StringLength(200)]
        public string ProfilePictureUrl { get; set; }

        [StringLength(200)]
        public string ProfilePicturePath { get; set; }

        [StringLength(200)]
        public string UserIDPath { get; set; }

        public DateOnly BirthDate { get; set; }

        public char? Gender { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(200)]
        public string Street { get; set; }

        [StringLength(100)]
        public string Role { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [StringLength(200)]
        public string CompanyName { get; set; }

        [StringLength(200)]
        public string Companywebsite { get; set; }

        public bool AcceptTerms { get; set; }

        public UserType UserType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Reaction> Reactions { get; set; }
        public virtual ICollection<UserFollower> Followers { get; set; }
        public virtual ICollection<UserFollower> Following { get; set; }
        public virtual ICollection<PostShare> SharedPosts { get; set; }
        public virtual ICollection<PostSave> SavedPosts { get; set; }
        public virtual ICollection<CommentLike> LikedComments { get; set; }
        public virtual ICollection<Chat> Chats { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }
    }
}
