﻿using HtmlAgilityPack;
using OddsScrapper.DbModels;
using System;
using System.IO;
using System.Linq;

namespace OddsScrapper
{
    public class CommingMatchesScrapper
    {
        private HtmlReader WebReader { get; } = new HtmlReader();
        private DbRepository DbRepository { get; } = new DbRepository(Path.Combine(HelperMethods.GetSolutionDirectory(), "ArchiveData_NoGames.db"));

        public string Scrape(string baseWebsite, string[] sports)
        {
            string date = null;
            
            var fileName = Path.Combine(HelperMethods.GetTommorowsGamesFolderPath(), $"{Path.GetRandomFileName()}.csv");
            var sureWinName = Path.Combine(HelperMethods.GetGamesToBetFolderPath(), $"{Path.GetRandomFileName()}.csv");
            var sureWinHeaderWritten = false;
            using (var fileStream = File.AppendText(fileName))
            {
                using (var sureWinFileStream = File.AppendText(sureWinName))
                {
                    fileStream.WriteLine("Sport,Country,League,Participants,SportId,CountryId,LeagueId,Bet,HomeTeamId,AwayTeamId,HomeOdd,DrawOdd,AwayOdd,IsPlayoffs,IsCup,IsWomen");
                    foreach (var sport in sports)
                    {
                        var tommorowsGames = GetTommorowGames(baseWebsite, sport);
                        if (tommorowsGames == null)
                            break;

                        var s = WriteResultsToFile(tommorowsGames, baseWebsite, sport, fileStream, sureWinFileStream, ref sureWinHeaderWritten);
                        if (string.IsNullOrEmpty(date))
                            date = s;
                    }
                }
            }
            File.Copy(fileName, Path.Combine(HelperMethods.GetTommorowsGamesFolderPath(), $"games_{date}.csv"), true);
            File.Delete(fileName);

            if(sureWinHeaderWritten)
                File.Copy(sureWinName, Path.Combine(HelperMethods.GetTommorowsGamesFolderPath(), $"sureWinGames_{date}.csv"), true);
            File.Delete(sureWinName);

            return date;
        }

        private string WriteResultsToFile(HtmlDocument tommorowsGames, string baseWebsite, string sport, StreamWriter fileStream, StreamWriter sureWinFileStream, ref bool sureWinHeaderWritten)
        {
            var date = GetDate(tommorowsGames);

            {
                var div = tommorowsGames.GetElementbyId("table-matches");
                var table = div.Element(HtmlTagNames.Table);
                var isPlayoffs = false;
                foreach (var tr in table.Element(HtmlTagNames.Tbody).ChildNodes)
                {
                    var attribute = tr.GetAttributeValue(HtmlAttributes.Class, null);
                    if (string.IsNullOrEmpty(attribute))
                        continue;

                    if (attribute.Contains("center nob-border"))
                    {
                        // date
                        var dateElement = tr.Element("th").Element(HtmlTagNames.Span);
                        if (dateElement == null)
                            continue;

                        var dateAttribute = dateElement.GetAttributeValue(HtmlAttributes.Class, null);
                        if (string.IsNullOrEmpty(dateAttribute) || !dateAttribute.Contains("datet"))
                            continue;

                        isPlayoffs = tr.InnerText.Contains("Play Offs");
                    }

                    // date, matchup and odds tds in a row
                    var tds = tr.ChildNodes.Where(s => s.Name == HtmlTagNames.Td).ToArray();
                    if (tds.Length < 4)
                        continue;

                    var odds = tds.Where(s => s.Attributes[HtmlAttributes.Class].Value.Contains("odds-nowrp")).ToArray();
                    // has a winner?
                    if (odds.Any(s => s.Attributes[HtmlAttributes.Class].Value.Contains("result-ok")))
                        continue;

                    var nameTd = tds.First(s => s.GetAttributeValue(HtmlAttributes.Class, string.Empty).Contains("table-participant"));
                    var nameElement = nameTd.Elements(HtmlTagNames.A).First(s => !string.IsNullOrEmpty(s.GetAttributeValue(HtmlAttributes.Href, null)) &&
                                                                                !s.Attributes[HtmlAttributes.Href].Value.Contains("javascript"));

                    var participantsString = nameElement.InnerText;
                    var gameLink = nameElement.Attributes[HtmlAttributes.Href].Value;
                    if (!gameLink.Contains(sport))
                        continue;

                    var game = new Game();
                    game.IsPlayoffs = isPlayoffs;
                    HelperMethods.PopulateOdds(game, odds);
                    var participants = HelperMethods.GetParticipants(participantsString);
                    game.HomeTeam = participants.Item1;
                    game.AwayTeam = participants.Item2;

                    gameLink = gameLink.Substring(gameLink.IndexOf($"/{sport}/"));
                    var gameLinkParts = gameLink.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    //var gameSport = gameLinkParts[0];
                    var country = gameLinkParts[1];
                    var leagueName = gameLinkParts[2];

                    CheckIsSureWin(sureWinFileStream, baseWebsite, gameLink, sport, country, leagueName, participantsString, ref sureWinHeaderWritten);

                    var sportId = DbRepository.GetSportId(sport);
                    var countryId = DbRepository.GetCountryId(country);
                    var league = DbRepository.GetLeague(sportId, countryId, leagueName);
                    var homeTeamId = 0;
                    var awayTeamId = 0;
                    if(league != null)
                    {
                        homeTeamId = DbRepository.GetTeamId(league.Id, game.HomeTeam);
                        awayTeamId = DbRepository.GetTeamId(league.Id, game.AwayTeam);
                    }

                    fileStream.WriteLine($"{sport},{country},{leagueName},{participantsString},{sportId},{countryId},{league.Id},{game.Bet},{homeTeamId},{awayTeamId},{game.HomeOdd},{game.DrawOdd},{game.AwayOdd},{(game.IsPlayoffs ? 1 : 0)},{(league.IsCup ? 1 : 0)},{(league.IsWomen ? 1 : 0)}");
                }
            }

            return date;
        }

        private void CheckIsSureWin(StreamWriter sureWinFileStream, string baseWebsite, string gameLink, string sport, string country, string league, string participants, ref bool sureWinHeaderWritten)
        {
            var page = WebReader.GetHtmlFromWebpage($"{baseWebsite}{gameLink}", OddsTableLoaded);
            if (page == null)
                return;
            var div = page.GetElementbyId("odds-data-table");
            HtmlNode table = null;
            foreach (var child in div.ChildNodes)
            {
                var t = child.Element(HtmlTagNames.Table);
                if (t != null && 
                    t.GetAttributeValue(HtmlAttributes.Class, string.Empty).Contains("table-main detail-odds sortable"))
                {
                    table = t;
                    break;
                }
            }
            if (table == null)
                return;
            
            double[] odds = null;
            foreach (var tr in table.Element(HtmlTagNames.Tbody).ChildNodes)
            {
                // date, matchup and odds tds in a row
                var tds = tr.ChildNodes.Where(s => s.Name == HtmlTagNames.Td).ToArray();
                if (tds.Length < 4)
                    continue;
                
                //var nameElement = tds[0].Elements(HtmlTagNames.A).First(s => s.GetAttributeValue(HtmlAttributes.Href, null) == "name");
                var bookersName = tds[0].InnerText.Replace("&nbsp;", string.Empty).Replace(Environment.NewLine, string.Empty);
                if (bookersName != "bwin" && bookersName != "bet365")
                    continue;

                var oddsTds = tds.Where(t => t.GetAttributeValue(HtmlAttributes.Class, string.Empty).Contains("right odds")).ToArray();
                if (oddsTds.Length < 2)
                    continue;

                var currentOdds = oddsTds.Select(HelperMethods.GetOddFromTdNode).ToArray();
                if (odds == null)
                {
                    odds = currentOdds;
                }
                else
                {
                    for (var i = 0; i < odds.Length; i++)
                    {
                        if(currentOdds[i] > odds[i])
                        {
                            odds[i] = currentOdds[i];
                        }
                    }
                }
            }

            if (odds != null && IsMustWinBet(odds))
            {
                if (!sureWinHeaderWritten)
                {
                    sureWinHeaderWritten = true;
                    sureWinFileStream.WriteLine("Sport,Country,League,Participants,Odds");
                }

                sureWinFileStream.WriteLine($"{sport},{country},{league},{participants},{odds}");
            }
        }

        private bool IsMustWinBet(double[] odds)
        {
            if (odds.Length == 2)
            {
                return ((odds[0] - 1.0) * (odds[1] - 1.0)) > 1.0;
            }
                
            if (odds.Length == 3)
            {
                return ((odds[0] * (odds[1] + odds[2])) / (odds[1] * odds[2] * (odds[0] - 1.0))) < 1.0;
            }
                
            return false;
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

        private HtmlDocument GetTommorowGames(string baseWebsite, string sport)
        {
            //return WebReader.GetHtmlFromWebpage(link, GamesTableLoaded);
            var link = $"{baseWebsite}/matches/{sport}/";
            var page = WebReader.GetHtmlFromWebpage(link, FirstPageLoaded);
            if (page == null)
                return null;
                        
            var datesSpan = page.DocumentNode.Descendants(HtmlTagNames.Span).FirstOrDefault(s => s.GetAttributeValue(HtmlAttributes.Class, null) == "next-games-date");
            if (datesSpan == null)
                return null;

            var a = datesSpan.Elements(HtmlTagNames.A).FirstOrDefault(s => s.InnerText.ToUpperInvariant().Contains("TOMORROW"));
            if (a == null)
                return null;

            var gamesLink = a.Attributes[HtmlAttributes.Href].Value;
            if (!gamesLink.StartsWith(baseWebsite))
                gamesLink = $"{baseWebsite}{gamesLink}";

            return WebReader.GetHtmlFromWebpage(gamesLink, GamesTableLoaded);
        }

        private static bool FirstPageLoaded(HtmlDocument document)
        {
            // WAIT until the dynamic text is set
            foreach(var span in document.DocumentNode.Descendants(HtmlTagNames.Span))
            {
                var attribute = span.GetAttributeValue(HtmlAttributes.Class, null);
                if (attribute == "next-games-date")
                {
                    return !string.IsNullOrEmpty(span.InnerHtml);
                }
            }

            return false;
        }

        private static bool GamesTableLoaded(HtmlDocument document)
        {
            // WAIT until the dynamic text is set
            return !string.IsNullOrEmpty(document.GetElementbyId("table-matches")?.InnerText);
        }

        private static bool OddsTableLoaded(HtmlDocument document)
        {
            // WAIT until the dynamic text is set
            return !string.IsNullOrEmpty(document.GetElementbyId("odds-data-table").InnerText);
        }
    }
}
