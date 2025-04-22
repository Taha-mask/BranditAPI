using System.ComponentModel.DataAnnotations;

namespace RMSProjectAPI.Model
{
    public class ChangePasswordDto
    {
        [Required]
        public string Email { get;  set; }
        public string OldPassword { get;  set; }
        public string NewPassword { get;  set; }
    }
}