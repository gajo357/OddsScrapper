using HtmlAgilityPack;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace OddsScrapper
{
    public enum ResultType
    {
        All,
        Seasonal
    }

    public enum AnalysisType
    {
        Negative,
        Positive
    }

    public static class HelperMethods
    {
        public const string ArchiveFolder = "Archive";
        public const string TommorowsGamesFolder = "TommorowsGames";
        public const string AnalysedArchivedDataFolder = "AnalysedArchivedData";
        public const string BetsFolderName = "GamesToBet";

        public static string GetArchiveFolderPath()
        {
            return Path.Combine(GetProjectDirectory(), ArchiveFolder);
        }

        public static string GetAnalysedArchivedDataFolderPath()
        {
            return Path.Combine(GetProjectDirectory(), AnalysedArchivedDataFolder);
        }

        public static string GetTommorowsGamesFolderPath()
        {
            return Path.Combine(GetProjectDirectory(), TommorowsGamesFolder);
        }

        public static string GetGamesToBetFolderPath()
        {
            return Path.Combine(GetProjectDirectory(), BetsFolderName);
        }

        public static string GetAnalysedResultsFile(int bet, ResultType resultType)
        {
            var type = GetResultTypeText(resultType);
            return Path.Combine(GetAnalysedArchivedDataFolderPath(), $"results_{type}_{bet}.csv");
        }

        public static string GetAnalysedResultsFile(int bet, ResultType resultType, AnalysisType analysisType)
        {
            var type = GetResultTypeText(resultType);
            var analysisText = GetAnalysisTypeText(analysisType);

            return Path.Combine(GetAnalysedArchivedDataFolderPath(), $"results_{type}_{analysisText}_{bet}.csv");
        }

        public static string[] GetAnalysedResultsFiles()
        {
            return new[]
            {
                GetAnalysedResultsFile(1, ResultType.All, AnalysisType.Positive),
                GetAnalysedResultsFile(2, ResultType.All, AnalysisType.Positive),

                GetAnalysedResultsFile(1, ResultType.Seasonal, AnalysisType.Positive),
                GetAnalysedResultsFile(2, ResultType.Seasonal, AnalysisType.Positive),
            };
        }

        private static string GetBetText(int bet)
        {
            if (bet == 0)
                return "draw";
            if (bet == 1)
                return "home";

            return "away";
        }

        public static string GetResultTypeText(ResultType resultType)
        {
            switch (resultType)
            {
                case ResultType.All:
                    return "all";
                case ResultType.Seasonal:
                    return "byseason";
                default:
                    return string.Empty;
            }
        }

        private static string GetAnalysisTypeText(AnalysisType analysisType)
        {
            switch(analysisType)
            {
                case AnalysisType.Negative:
                    return "negative";
                case AnalysisType.Positive:
                    return "positive";
                default:
                    return string.Empty;
            }
        }

        public static string GetSolutionDirectory()
        {
            return Directory.GetParent(GetProjectDirectory()).FullName;
        }

        public static string GetProjectDirectory()
        {
            return Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        }

        public static string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(name, invalidRegStr, "_");
        }

        /// <summary>
        /// Returns 1 for home bet, 0 for draw and 2 for away
        /// </summary>
        /// <param name="length"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static int GetBetComboFromIndex(int length, int index)
        {
            if (index == 0)
            {
                // first index is always a home win
                return 1;
            }
            if (index == length - 1)
            {
                // last index is always a visitor win
                return 2;
            }

            // else it's a draw
            return 0;
        }

        public static Tuple<string, string> GetParticipants(string participantsString)
        {
            var participants = participantsString.Replace("&nbsp;", string.Empty).Replace("&amp;", "and").Replace("'", " ").Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
            if (participants.Length < 2)
                return null;

            return new Tuple<string, string>(participants[0], participants[1]);
        }

        public static void PopulateOdds(Game game, HtmlNode[] oddTags)
        {
            int winIndex = -1;
            int lowestOddIndex = -1;
            var lowestOdd = double.MaxValue;
            double homeOdd = 0;
            double drawOdd = 0;
            double awayOdd = 0;
            for (var i = 0; i < oddTags.Length; i++)
            {
                var nodeOdd = GetOddFromTdNode(oddTags[i]);
                if (i == 0)
                {
                    homeOdd = nodeOdd;
                }
                else if (i == 1 && oddTags.Length == 3)
                {
                    drawOdd = nodeOdd;
                }
                else
                {
                    awayOdd = nodeOdd;
                }

                if (nodeOdd < lowestOdd)
                {
                    lowestOdd = nodeOdd;
                    lowestOddIndex = i;
                }

                if (oddTags[i].Attributes[HtmlAttributes.Class].Value.Contains("result-ok"))
                {
                    winIndex = i;
                }
            }

            // 1 is home win, 2 is away win, 0 is draw
            int winCombo = GetBetComboFromIndex(oddTags.Length, winIndex);
            int betCombo = GetBetComboFromIndex(oddTags.Length, lowestOddIndex);
            
            game.HomeOdd = homeOdd;
            game.DrawOdd = drawOdd;
            game.AwayOdd = awayOdd;
            game.Bet = betCombo;
            game.Winner = winCombo;
        }

        public static double CalculateKellyCriterionPercentage(double odd, double successRate)
        {
            return (successRate * odd - 1) / (odd - 1);
        }

        public static double GetOddFromTdNode(HtmlNode tdNode)
        {
            double odd;
            if (double.TryParse(tdNode.FirstChild.InnerText, out odd))
                return odd;

            return double.NaN;
        }
    }
}
