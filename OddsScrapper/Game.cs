using System;

namespace OddsScrapper
{
    public class Game
    {
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }

        public double HomeOdd { get; set; }
        public double DrawOdd { get; set; }
        public double AwayOdd { get; set; }

        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }

        public int Bet { get; set; }
        public int Winner { get; set; }

        public int Season { get; set; }

        public DateTime Date { get; set; }
        public bool IsOvertime { get; internal set; }
        public bool IsPlayoffs { get; internal set; }
    }
}
