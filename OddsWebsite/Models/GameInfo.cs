namespace OddsWebsite.Models
{
    public class GameInfo
    {
        public int Id { get; set; }

        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }

        public double HomeOdd { get; set; }
        public double AwayOdd { get; set; }

        public int Winner { get; set; }
    }
}
