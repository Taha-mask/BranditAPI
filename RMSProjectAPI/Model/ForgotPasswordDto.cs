using System.ComponentModel.DataAnnotations;

namespace RMSProjectAPI.Model
{
    public class ForgotPasswordDto
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}
