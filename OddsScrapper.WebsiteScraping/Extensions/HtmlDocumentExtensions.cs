using HtmlAgilityPack;
using OddsScrapper.Repository.Models;
using OddsScrapper.WebsiteScraping.Helpers;
using OddsScrapper.WebsiteScraping.Scrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OddsScrapper.WebsiteScrapping.Extensions
{
    public static class HtmlDocumentExtensions
    {
        public static HtmlNode GetOddsTableFromGameDocument(this HtmlDocument gameDocument)
        {
            var div = gameDocument.GetElementbyId("odds-data-table");
            HtmlNode table = null;
            foreach (var child in div.ChildNodes)
            {
                var t = child.Element(HtmlTagNames.Table);
                if (t != null && t.AttributeContains(HtmlAttributes.Class, "table-main detail-odds sortable"))
                {
                    table = t;
                    break;
                }
            }

            return table;
        }

        public static (string home, string away) ReadParticipantsFromGameDocument(this HtmlDocument gameDocument)
        {
            var contentDiv = gameDocument.GetElementbyId("col-content");
            var header = contentDiv.Element(HtmlTagNames.H1);

            var participants = header.InnerText
                .Replace("&nbsp;", string.Empty)
                .Replace("&amp;", "and")
                .Replace("'", " ")
                .Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
            if (participants.Length < 2)
                return (null, null);

            return (participants[0], participants[1]);
        }

        public static DateTime? ReadDateAndTimeFromGameDocument(this HtmlDocument gameDocument)
        {
            HtmlNode dateNode = null;

            var contentDiv = gameDocument.GetElementbyId("col-content");
            foreach (var p in contentDiv.Elements("p"))
            {
                if (!p.AttributeContains(HtmlAttributes.Class, "date"))
                {
                    continue;
                }

                dateNode = p;
                break;
            }

            if (dateNode == null)
                return null;

            var dateStrings = dateNode.InnerText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var dateString = dateStrings[1];
            var timeString = dateStrings[2];

            if (DateTime.TryParseExact($"{dateString} {timeString}", $"{BaseScrapper.DateFormat} {BaseScrapper.TimeFormat}", null, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime date))
                return date;

            return null;
        }

        public static IList<(string booker, GameOdds)> ReadGameOddsFromGameDocument(this HtmlDocument gameDocument)
        {
            var result = new List<(string booker, GameOdds)>();

            HtmlNode table = gameDocument.GetOddsTableFromGameDocument();
            if (table == null)
                return result;

            foreach (var tableRow in table.Element(HtmlTagNames.Tbody).ChildNodes)
            {
                // date, matchup and odds tds in a row
                var tds = tableRow.ChildNodes.WithName(HtmlTagNames.Td).ToArray();
                if (tds.Length < 4)
                    continue;

                var bookersName = tds[0].InnerText.Replace("&nbsp;", string.Empty).Replace(Environment.NewLine, string.Empty);
                if (string.IsNullOrEmpty(bookersName))
                    continue;

                var oddsTds = tds.WithAttributeContains(HtmlAttributes.Class, "right odds").ToArray();
                if (oddsTds.Length < 2)
                    continue;

                var deactivated = oddsTds.WithAttributeContains(HtmlAttributes.Class, "deactivate").Any();

                var currentOdds = oddsTds.Select(n => n.GetOddFromTdNode()).ToArray();

                var homeOdd = currentOdds[0];
                var drawOdd = 0.0;
                var awayOdd = 0.0;
                if (currentOdds.Length == 2)
                {
                    drawOdd = 0;
                    awayOdd = currentOdds[1];
                }
                else
                {
                    drawOdd = currentOdds[1];
                    awayOdd = currentOdds[2];
                }

                result.Add((bookersName,
                    new GameOdds()
                    {
                        HomeOdd = homeOdd,
                        DrawOdd = drawOdd,
                        AwayOdd = awayOdd,
                        IsValid = !deactivated
                    }));
            }

            return result;
        }

        public static (int homeScore, int awayScore, bool isOvertime) ReadGameScoresFromGameDocument(this HtmlDocument gameDocument)
        {
            (int h, int a, bool i) defaultResult = (-1, -1, false);

            var contentDiv = gameDocument.GetElementbyId("event-status");
            if (contentDiv == null)
                return defaultResult;

            var statusText = contentDiv.InnerText;

            const string regexPattern = @"Final result (\d+):(\d+)*";
            var match = Regex.Match(statusText, regexPattern);
            if (match == null)
                return defaultResult;

            statusText = statusText.ToUpper();
            var isOvertime = statusText.Contains("OT") || statusText.Contains("OVERTIME");

            var home = Convert.ToInt32(match.Groups[1].Value);
            var away = Convert.ToInt32(match.Groups[2].Value);
            return (home, away, isOvertime);
        }
    }
}
