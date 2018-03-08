using System.ComponentModel.DataAnnotations;

namespace OddsScrapper.Repository.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        
        public Sport Sport { get; set; }
    }
}
