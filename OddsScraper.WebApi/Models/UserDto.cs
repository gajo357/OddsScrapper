using System.ComponentModel.DataAnnotations;

namespace OddsScraper.WebApi.Models
{
    public class UserDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
