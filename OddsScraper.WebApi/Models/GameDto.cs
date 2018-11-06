using System;

namespace OddsScraper.WebApi.Models
{
    public class GameDto
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

        internal static GameDto Create(FSharp.CommonScraping.Models.Game model)
        {
            return new GameDto
            {
                Sport = model.Sport,
                Country = model.Country,
                League = model.League,

                HomeTeam = model.HomeTeam,
                AwayTeam = model.AwayTeam,

                Date = model.Date,

                HomeMeanOdd = model.HomeMeanOdd,
                DrawMeanOdd = model.DrawMeanOdd,
                AwayMeanOdd = model.AwayMeanOdd,

                HomeOdd = model.HomeOdd,
                DrawOdd = model.DrawOdd,
                AwayOdd = model.AwayOdd,

                GameLink = model.GameLink
            };
        }
    }
}
