using System.ComponentModel.DataAnnotations;

namespace OddsWebsite.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public League League { get; set; }
    }
}
