using OddsScrapper.Repository.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OddsScrapper.Repository.Dto
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
        
        public List<GameOdds> Odds { get; } = new List<GameOdds>();

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

        public static implicit operator Game(GameDto dto)
        {
            return dto.ToGame();
        }

        public Game ToGame()
        {
            var game =  new Game
            {
                Id = Id,
                League = League,
                HomeTeam = HomeTeam,
                AwayTeam = AwayTeam,

                Date = Date.Value,

                HomeTeamScore = HomeTeamScore.Value,
                AwayTeamScore = AwayTeamScore.Value,

                IsPlayoffs = IsPlayoffs,
                IsOvertime = IsOvertime
            };

            game.Odds.AddRange(Odds);

            return game;
        }
    }
}
