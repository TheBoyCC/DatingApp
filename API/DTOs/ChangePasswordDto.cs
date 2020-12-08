using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ChangePasswordDto
    {
        public string Username { get; set; }
        public string OldPassword { get; set; }
        
        [Required]
        [StringLength(8, MinimumLength = 4)]
        public string NewPassword { get; set; }
    }
}