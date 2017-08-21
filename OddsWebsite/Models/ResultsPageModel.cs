using System.Collections.Generic;

namespace OddsWebsite.Models
{
    public class ResultsPageModel
    {
        public double AvailableAmount { get; set; }

        public List<ActiveGame> GamesToGet { get; set; }
    }
}
