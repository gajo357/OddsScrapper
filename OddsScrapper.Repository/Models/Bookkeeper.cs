using System.ComponentModel.DataAnnotations;

namespace OddsScrapper.Repository.Models
{
    public class Bookkeeper
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
