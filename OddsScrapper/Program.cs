using System;

namespace OddsScrapper
{
    public class Program
    {
        public const string BaseWebsite = "http://www.oddsportal.com";
        private const string Football = "soccer";
        private const string Basketball = "basketball";
        private const string Tennis = "tennis";
        private const string Handball = "handball";
        private const string Hockey = "hockey";
        private const string Baseball = "baseball";
        private const string AmericanFootball = "american-football";
        private const string RugbyLeague = "rugby-league";
        private const string RugbyUnion = "rugby-union";
        private const string WaterPolo = "water-polo";
        private const string Volleyball = "volleyball";


        [STAThread]
        public static void Main(string[] args)
        {
            var scrapper = new ArchiveOddsScrapper();
            scrapper.Scrape(BaseWebsite, new[] { Basketball, Handball, Hockey, Baseball, AmericanFootball, RugbyLeague, RugbyUnion, WaterPolo, Volleyball });

            //var scrapper = new CommingMatchesScrapper();
            //scrapper.Scrape(BaseWebsite, new[] { Football, Basketball, Handball, Hockey, Baseball, AmericanFootball });

            //var analyser = new ArchiveDataAnalysis();
            //analyser.Analyse();

            //var matcher = new OddsMatcher();
            //matcher.MatchGamesWithArchivedData();
        }
    }
}
