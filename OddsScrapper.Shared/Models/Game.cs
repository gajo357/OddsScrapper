using System;
using System.Collections.Generic;

namespace OddsScrapper.Shared.Models
{
    public class Game
    {
        public int Id { get; set; }

        public League League { get; set; }

        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }

        public List<GameOdds> Odds { get; } = new List<GameOdds>();

        public DateTime? Date { get; set; }

        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }

        public bool IsPlayoffs { get; set; }
        public bool IsOvertime { get; set; }

        public string GameLink { get; set; }
    }
}
