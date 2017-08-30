using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;

namespace OddsScrapper
{
    public class CommingMatchesScrapper
    {
        private HtmlReader WebReader { get; } = new HtmlReader();

        public string Scrape(string baseWebsite, string[] sports)
        {
            string date = null;
            
            var fileName = Path.Combine(HelperMethods.GetTommorowsGamesFolderPath(), $"{Path.GetRandomFileName()}.csv");
            using (var fileStream = File.AppendText(fileName))
            {
                fileStream.WriteLine("Sport,Country,League,Season,Participants,WinningOdd,Bet");
                foreach (var sport in sports)
                {
                    var tommorowsGames = GetTommorowGames($"{baseWebsite}/matches/{sport}/");
                    if (tommorowsGames == null)
                        break;

                    var s = WriteResultsToFile(tommorowsGames, sport, fileStream);
                    if (string.IsNullOrEmpty(date))
                        date = s;
                }
            }
            File.Copy(fileName, Path.Combine(HelperMethods.GetTommorowsGamesFolderPath(), $"games_{date}.csv"), true);
            File.Delete(fileName);

            return date;
        }

        private string WriteResultsToFile(HtmlDocument tommorowsGames, string sport, StreamWriter fileStream)
        {
            var date = GetDate(tommorowsGames);

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

                    int oddIndex = -1;
                    double odd = 0;
                    for (var i = 0; i < odds.Length; i++)
                    {
                        var nodeOdd = ArchiveOddsScrapper.GetOddFromTdNode(odds[i]);
                        if (nodeOdd <= 1.5)
                        {
                            odd = nodeOdd;
                            oddIndex = i;
                            break;
                        }
                    }
                    if (oddIndex < 0)
                        continue;

                    int betCombo = HelperMethods.GetBetComboFromIndex(odds.Length, oddIndex);

                    var nameTd = tds.First(s => s.GetAttributeValue(HtmlAttributes.Class, string.Empty).Contains("table-participant"));
                    var nameElement = nameTd.Elements(HtmlTagNames.A).First(s => !string.IsNullOrEmpty(s.GetAttributeValue(HtmlAttributes.Href, null)) &&
                                                                                !s.Attributes[HtmlAttributes.Href].Value.Contains("javascript"));

                    var participants = nameElement.InnerText;
                    var gameLink = nameElement.Attributes[HtmlAttributes.Href].Value;
                    if (!gameLink.Contains(sport))
                        continue;

                    gameLink = gameLink.Substring(gameLink.IndexOf($"/{sport}/"));
                    var gameLinkParts = gameLink.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    //var gameSport = gameLinkParts[0];
                    var country = gameLinkParts[1];
                    var league = gameLinkParts[2];

                    fileStream.WriteLine($"{sport},{country},{league},2018,{participants},{odd},{betCombo}");
                }
            }

            return date;
        }

        private string GetDate(HtmlDocument tommorowsGames)
        {
            var div = tommorowsGames.GetElementbyId("col-content");
            if (div == null)
                return null;

            var header = div.Element(HtmlTagNames.H1);
            var date = header.InnerText.Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries).Last();
            return date.Replace(" ", string.Empty);
        }

        private HtmlDocument GetTommorowGames(string link)
        {
            //return WebReader.GetHtmlFromWebpage(link, GamesTableLoaded);

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

        private static bool FirstPageLoaded(System.Windows.Forms.WebBrowser webBrowser)
        {
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

        private static bool GamesTableLoaded(System.Windows.Forms.WebBrowser webBrowser)
        {
            // WAIT until the dynamic text is set
            return !string.IsNullOrEmpty(webBrowser.Document.GetElementById("table-matches").InnerText);
        }
    }
}
