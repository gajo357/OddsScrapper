using System;

namespace OddsWebsite.Models
{
    public class Game
    {
        public int Id { get; set; }

        public League League { get; set; }

        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }

        public double HomeOdd { get; set; }
        public double DrawOdd { get; set; }
        public double AwayOdd { get; set; }

        public DateTime Date { get; set; }

        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }

        public bool IsPlayoffs { get; set; }
        public bool IsOvertime { get; set; }
    }
}
