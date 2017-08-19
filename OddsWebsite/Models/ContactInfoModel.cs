using System.ComponentModel.DataAnnotations;

namespace OddsWebsite.Models
{
    public class ContactInfoModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        //[Required]
        //public string Subject { get; set; }
        //[Required]
        //public string Body { get; set; }
    }
}
