using System.ComponentModel.DataAnnotations;

namespace OddsScrapper.Shared.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }        
    }
}
