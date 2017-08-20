using System.Collections.Generic;

namespace OddsWebsite.Models
{
    public class League
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Sport Sport { get; set; }
        public Country Country { get; set; }

        public bool IsFirst { get; set; }

        public IList<Game> Games { get; set; }
    }
}
