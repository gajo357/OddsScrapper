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
        
        public static string GetArchiveFolderPath()
        {
            return Path.Combine(GetSolutionDirectory(), ArchiveFolder);
        }

        public static string GetAnalysedArchivedDataFolderPath()
        {
            return Path.Combine(GetSolutionDirectory(), AnalysedArchivedDataFolder);
        }

        public static string GetTommorowsGamesFolderPath()
        {
            return Path.Combine(GetSolutionDirectory(), TommorowsGamesFolder);
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
            return Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
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

        public static double CalculateKellyCriterionPercentage(double odd, double successRate)
        {
            return (successRate * odd - 1) / (odd - 1);
        }
    }
}
