using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMSProjectAPI.Database.Entity
{
    public class User: IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public char? Gender { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string? Role { get; set; }
        public DateOnly CreatedAt { get; set; }
        public string? Status { get; set; }
        public string ProfilePicturePath { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public string? Companywebsite{ get; set; }
        public string UserIDPath { get; set; }
        public bool AcceptTerms { get; set; }








    }
}
