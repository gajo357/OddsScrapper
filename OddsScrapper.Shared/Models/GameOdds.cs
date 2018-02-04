using System.ComponentModel.DataAnnotations;

namespace OddsScrapper.Shared.Models
{
    public class GameOdds
    {
        public int Id { get; set; }

        public Bookkeeper Bookkeeper { get; set; }

        [Required]
        [Range(1e-10, double.MaxValue)]
        public double HomeOdd { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double DrawOdd { get; set; }

        [Required]
        [Range(1e-10, double.MaxValue)]
        public double AwayOdd { get; set; }

        public bool IsValid { get; set; }
    }
}
