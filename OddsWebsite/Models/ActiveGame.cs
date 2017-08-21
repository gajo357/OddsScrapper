namespace OddsWebsite.Models
{
    public class ActiveGame
    {
        public string Country { get; set; }
        public string League { get; set; }
        public string Sport { get; set; }

        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public double BestOdd { get; set; }

        public LeagueProcessedData LeagueData { get; set; }

        /// <summary>
        /// Is this game to be included in the calculation
        /// </summary>
        public bool IncludeInBet { get; set; }

        /// <summary>
        /// Calculated amount to bet based on Kelly criteria and the best odd for this game
        /// </summary>
        public double AmountToBet { get; set; }
    }
}
