using OddsScrapper.Repository.Repository;
using OddsScrapper.WebsiteScraping.Helpers;
using OddsScrapper.WebsiteScraping.Scrappers;
using OddsScrapper.WebsiteScrapping;
using System;

namespace OddsScrapper.Console
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
            try
            {
                ScrapperInitializer.Initialize();

                System.Console.WriteLine("Press 1 to download all history, press 2 to download coming games");
                var input = System.Console.ReadLine();
                int choice;
                if (!int.TryParse(input, out choice))
                    return;

                var reader = new HtmlContentReader();
                var repository = new ArchiveDataRepository(new ArchiveContext(new Microsoft.EntityFrameworkCore.DbContextOptions<ArchiveContext>()));
                if (choice == 1)
                {
                    var scrapper = new FinishedGamesScrapper(repository, reader);
                    scrapper.ScrapeAsync(BaseWebsite, AllSports, DateTime.Today).Wait();
                }
                else if (choice == 2)
                {
                    System.Console.WriteLine("Enter 1 for today, and 2 for tomorrow");
                    var dateInput = System.Console.ReadLine();
                    int dateChoice;
                    if (!int.TryParse(dateInput, out dateChoice))
                        return;

                    if (dateChoice != 1 && dateChoice != 2)
                        return;

                    var date = dateChoice == 1 ? DateTime.Today : DateTime.Today.AddDays(1);

                    var scrapper = new UnfinishedGamesScrapper(repository, reader);
                    scrapper.ScrapeAsync(BaseWebsite, AllSports, date).Wait();
                }
            }
            finally
            {
                ScrapperInitializer.CleanUp();
            }
        }
    }
}
