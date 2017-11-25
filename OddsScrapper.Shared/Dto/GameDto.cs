using OddsWebsite.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace OddsScrapper.Shared.Dto
{
    public class GameDto
    {
        public int Id { get; set; }

        [Required]
        public League League { get; set; }

        [Required]
        public Team HomeTeam { get; set; }
        [Required]
        public Team AwayTeam { get; set; }

        [Required]
        [Range(1e-10, double.MaxValue)]
        public double? HomeOdd { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double? DrawOdd { get; set; }
        [Required]
        [Range(1e-10, double.MaxValue)]
        public double? AwayOdd { get; set; }

        [Required]
        public DateTime? Date { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int? HomeTeamScore { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int? AwayTeamScore { get; set; }

        [Required]
        public bool IsPlayoffs { get; set; }
        [Required]
        public bool IsOvertime { get; set; }

        public Game ToGame()
        {
            return new Game
            {
                Id = Id,
                League = League,
                HomeTeam = HomeTeam,
                AwayTeam = AwayTeam,

                HomeOdd = HomeOdd.Value,
                DrawOdd = DrawOdd.Value,
                AwayOdd = AwayOdd.Value,

                Date = Date.Value,

                HomeTeamScore = HomeTeamScore.Value,
                AwayTeamScore = AwayTeamScore.Value,

                IsPlayoffs = IsPlayoffs,
                IsOvertime = IsOvertime
            };
        }
    }
}
