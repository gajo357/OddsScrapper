using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;

namespace OddsScrapper
{
    public class CommingMatchesScrapper
    {
        private HtmlReader WebReader { get; } = new HtmlReader();

        public void Scrape(string baseWebsite, string sport)
        {
            var tommorowsGames = GetTommorowGames($"{baseWebsite}/matches/{sport}/");
            if (tommorowsGames == null)
                return;

            WriteResultsToFile(tommorowsGames);
        }

        private void WriteResultsToFile(HtmlDocument tommorowsGames)
        {
            var fileName = $"tommorows_games.csv";
            using (var fileStream = File.AppendText(fileName))
            {
                var div = tommorowsGames.GetElementbyId("table-matches");
                var table = div.Element(HtmlTagNames.Table);
                foreach (var tr in table.Element(HtmlTagNames.Tbody).ChildNodes)
                {
                    // date, matchup and odds tds in a row
                    var tds = tr.ChildNodes.Where(s => s.Name == HtmlTagNames.Td).ToArray();
                    if (tds.Length < 4)
                        continue;

                    var odds = tds.Where(s => s.Attributes[HtmlAttributes.Class].Value.Contains("odds-nowrp")).ToArray();
                    // has a winner?
                    if (odds.Any(s => s.Attributes[HtmlAttributes.Class].Value.Contains("result-ok")))
                        continue;

                    var goodOdd = odds.FirstOrDefault(s => ArchiveOddsScrapper.GetOddFromTdNode(s) <= 1.5);
                    if (goodOdd == null)
                        continue;

                    var nameTd = tds.First(s => s.GetAttributeValue(HtmlAttributes.Class, string.Empty).Contains("table-participant"));
                    var name = nameTd.Elements(HtmlTagNames.A)
                        .First(s => !string.IsNullOrEmpty(s.GetAttributeValue(HtmlAttributes.Href, null)) && 
                                    !s.Attributes[HtmlAttributes.Href].Value.Contains("javascript")).InnerText;

                    fileStream.WriteLine($"{name},{String.Join(",", odds.Select(s => s.InnerText).ToArray())}");
                }
            }
        }

        private HtmlDocument GetTommorowGames(string link)
        {
            var page = WebReader.GetHtmlFromWebpage(link, FirstPageLoaded);
            if (page == null)
                return null;
                        
            var datesSpan = page.DocumentNode.Descendants(HtmlTagNames.Span).FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Class, null) == "next-games-date");
            if (datesSpan == null)
                return null;

            var a = datesSpan.Elements(HtmlTagNames.A).FirstOrDefault(s => s.InnerText.ToUpperInvariant().Contains("TOMORROW"));
            if (a == null)
                return null;

            return WebReader.GetHtmlFromWebpage(a.Attributes[HtmlAttributes.Href].Value, GamesTableLoaded);
        }

        private static bool FirstPageLoaded(object o)
        {
            var webBrowser = (System.Windows.Forms.WebBrowser)o;

            // WAIT until the dynamic text is set
            foreach(System.Windows.Forms.HtmlElement span in webBrowser.Document.GetElementsByTagName(HtmlTagNames.Span))
            {
                var attribute = span.GetAttribute(HtmlAttributes.ClassName);
                if (attribute == "next-games-date")
                {
                    return !string.IsNullOrEmpty(span.InnerHtml);
                }
            }

            return false;
        }

        private static bool GamesTableLoaded(object o)
        {
            var webBrowser = (System.Windows.Forms.WebBrowser)o;

            // WAIT until the dynamic text is set
            return !string.IsNullOrEmpty(webBrowser.Document.GetElementById("table-matches").InnerText);
        }
    }
}
