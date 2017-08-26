using OddsWebsite.Models;
using System.Collections.Generic;

namespace OddsWebsite.ViewModels
{
    public class IndexViewModel
    {
        public double AvailableAmount { get; set; }

        public List<ActiveGame> GamesToGet { get; set; }
    }
}
