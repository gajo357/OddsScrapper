using System;

namespace OddsWebsite.Models
{
    public class Game
    {
        public int Id { get; set; }

        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }

        public double WinningOdd { get; set; }

        public int Bet { get; set; }
        public int Winner { get; set; }

        public int Season { get; set; }

        public DateTime Date { get; set; }
    }
}
