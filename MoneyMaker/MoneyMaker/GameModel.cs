using System;

namespace MoneyMaker
{
    public class GameModel
    {
        public string Sport { get; set; }
        public string Country { get; set; }
        public string League { get; set; }

        public string AwayTeam { get; set; }
        public string HomeTeam { get; set; }

        public string GameLink { get; set; }

        public double AwayMeanOdd { get; set; }
        public double DrawMeanOdd { get; set; }
        public double HomeMeanOdd { get; set; }

        public DateTime Date { get; set; }

        public double HomeOdd { get; set; }
        public double DrawOdd { get; set; }
        public double AwayOdd { get; set; }
    }
}
