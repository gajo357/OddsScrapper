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

        private static string[] AllSports = new[] { Football, Basketball, Handball, Hockey, Baseball, AmericanFootball, RugbyLeague, RugbyUnion, WaterPolo, Volleyball };

        [STAThread]
        public static void Main(string[] args)
        {
            //var scrapper = new ArchiveOddsScrapper();
            //scrapper.Scrape(BaseWebsite, AllSports);

            //var analyser = new ArchiveDataAnalysis();
            //analyser.Analyse();

            //var scrapper = new CommingMatchesScrapper();
            //var date = scrapper.Scrape(BaseWebsite, AllSports);

            var date = "28Aug2017";
            var matcher = new OddsMatcher();
            matcher.MatchGamesWithArchivedData(date);
        }
    }
}
