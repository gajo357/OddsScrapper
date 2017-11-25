using System.ComponentModel.DataAnnotations;

namespace OddsWebsite.Models
{
    public class League
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Sport Sport { get; set; }
        [Required]
        public Country Country { get; set; }

        [Required]
        public bool IsFirst { get; set; }
        [Required]
        public bool IsWomen { get; set; }
        [Required]
        public bool IsCup { get; set; }
    }
}
