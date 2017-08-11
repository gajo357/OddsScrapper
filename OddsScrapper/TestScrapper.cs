using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OddsScrapper
{
    public class TestScrapper
    {
        public const string BaseWebsite = "http://www.oddsportal.com";
        private const string Football = "soccer";

        public static void Read()
        {
            var country = "england";
            var league = "premier-league";
            var page = $"{BaseWebsite}/{Football}/{country}/{league}/results/";

            var resultsFile = "results.csv";

            var html = GetHtmlFromWebpage(page);
            var mainDiv = html.DocumentNode.Descendants("div").First(s => s.GetAttributeValue("class", null) == "main-menu2 main-menu-gray");
            var ul = mainDiv.Element("ul");
            foreach (var a in ul.Descendants("a").Skip(1))
            {
                var seasonResultsLink = a.Attributes["href"].Value;

                var seasonPage = $"{BaseWebsite}{seasonResultsLink}";
                var seasonHtml = GetHtmlFromWebpage(seasonPage, TournamentTableDivLoaded);

                var divTable = FindResultsDiv(seasonHtml);
                if (divTable == null)
                    continue;

                var lines = FindAndReadResultsTable(divTable);
                if (lines != null)
                    System.IO.File.AppendAllLines(resultsFile, lines);
                var pagination = divTable.Element("div");

                foreach (var resultPage in pagination.ChildNodes)
                {
                    if (resultPage.Name == "a" &&
                        resultPage.FirstChild.GetAttributeValue("class", null) != "arrow")
                    {
                        var pagePage = resultPage.Attributes["href"].Value;
                        var pageResult = GetHtmlFromWebpage(pagePage, TournamentTableDivLoaded);
                        var pageLines = FindAndReadResultsTable(FindResultsDiv(pageResult));
                        if (lines != null)
                            System.IO.File.AppendAllLines(resultsFile, pageLines);
                    }
                }
            }
        }


        private static bool TournamentTableDivLoaded(object o)
        {
            var webBrowser = (System.Windows.Forms.WebBrowser)o;

            // WAIT until the dynamic text is set
            return !string.IsNullOrEmpty(webBrowser.Document.GetElementById("tournamentTable").InnerText);
        }

        private static HtmlNode FindResultsDiv(HtmlDocument document)
        {
            return document.DocumentNode.Descendants("div").FirstOrDefault(s => s.GetAttributeValue("id", null) == "tournamentTable");
        }

        private static IEnumerable<string> FindAndReadResultsTable(HtmlNode divNode)
        {
            var resultsTable = divNode.Element("table");
            if (resultsTable == null)
                yield break;

            foreach (var tr in resultsTable.Element("tbody").ChildNodes)
            {
                var attribute = tr.GetAttributeValue("class", null);
                if (string.IsNullOrEmpty(attribute) ||
                    !attribute.Contains("deactivate"))
                    continue;

                var odds = tr.Elements("td").Where(s => s.Attributes["class"].Value.Contains("odds-nowrp"));
                var goodOdd = odds.FirstOrDefault(s => GetOddFromTdNode(s) <= 1.5);
                if (goodOdd == null)
                    continue;

                var odd = GetOddFromTdNode(goodOdd);
                var isWinning = goodOdd.Attributes["class"].Value.Contains("result-ok") ? 1 : 0;

                yield return $"{odd},{isWinning}";
            }
        }

        public static double GetOddFromTdNode(HtmlNode tdNode)
        {
            double odd;
            if (double.TryParse(tdNode.FirstChild.InnerText, out odd))
                return odd;

            return double.NaN;
        }

        public static HtmlDocument GetHtmlFromWebpage(string webpage, Func<object, bool> isBrowserScriptCompleted = null)
        {
            var web = new HtmlWeb();

            var htmlDoc = isBrowserScriptCompleted != null ?
                web.LoadFromBrowser(webpage, isBrowserScriptCompleted) :
                web.Load(webpage);

            return htmlDoc;
        }
    }
}
