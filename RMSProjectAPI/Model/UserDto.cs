using System.ComponentModel.DataAnnotations;
using RMSProjectAPI.Database.Entity;

namespace RMSProjectAPI.Model
{
    public class UserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string UserName { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Street { get; set; }

        public char? Gender { get; set; }
        [Required]
        public DateOnly BirthDate { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
       
        public string? Status { get; set; }
        public string? Region { get; set; }
        public string? Role { get; set; }

        public DateOnly CreatedAt { get; set; }
        public string Password { get; set; }

        public string ProfilePicturePath { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public string? Companywebsite { get; set; }
        public string UserIDPath { get; set; }
        public bool AcceptTerms { get; set; }
        public UserType UserType { get; set; }
    }
}
