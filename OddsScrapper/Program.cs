using System;

namespace OddsScrapper
{
    public class Program
    {
        public const string BaseWebsite = "http://www.oddsportal.com";
        private const string Football = "soccer";

        [STAThread]
        public static void Main(string[] args)
        {
            var scrapper = new CommingMatchesScrapper(); // ArchiveOddsScrapper();
            scrapper.Scrape(BaseWebsite, Football);
        }
    }
}
