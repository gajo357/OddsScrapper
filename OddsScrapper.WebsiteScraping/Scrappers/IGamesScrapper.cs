using OddsScrapper.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OddsScrapper.WebsiteScraping.Scrappers
{
    public interface IGamesScrapper
    {
        Task<IEnumerable<Game>> ScrapeAsync(string baseWebsite, string[] sports, DateTime date);
    }
}
