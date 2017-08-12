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
        

         [STAThread]
        public static void Main(string[] args)
        {
            //var scrapper = new ArchiveOddsScrapper();
            //scrapper.ScrapeLeagueInfo(BaseWebsite, Football);
            //scrapper.Scrape(BaseWebsite, new []{ Football, Basketball, Handball, Hockey});

            var scrapper = new CommingMatchesScrapper();
            scrapper.Scrape(BaseWebsite, new[] { Football, Basketball, Handball, Hockey });

            //var analyser = new ArchiveDataAnalysis();
            //analyser.Analyse();
        }
    }
}
