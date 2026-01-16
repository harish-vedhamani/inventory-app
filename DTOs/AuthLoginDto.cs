using System.ComponentModel.DataAnnotations;

namespace Playground.DTOs
{
    public class AuthLoginDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
