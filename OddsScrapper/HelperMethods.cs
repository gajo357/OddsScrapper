using System.IO;
using System.Text.RegularExpressions;

namespace OddsScrapper
{
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

        public static string GetAllAnalysedResultsFile()
        {
            return Path.Combine(GetAnalysedArchivedDataFolderPath(), "results_all.csv");
        }

        public static string[] GetAnalysedResultsFiles()
        {
            return new[]
            {
                Path.Combine(GetAnalysedArchivedDataFolderPath(), "results_all_10percent.csv"),
                Path.Combine(GetAnalysedArchivedDataFolderPath(), "results_byseasons_10percent.csv"),
                Path.Combine(GetAnalysedArchivedDataFolderPath(), "results_all_negative.csv"),
                Path.Combine(GetAnalysedArchivedDataFolderPath(), "results_byseasons_allnegative.csv"),
                Path.Combine(GetAnalysedArchivedDataFolderPath(), "results_byseasons_allpositive.csv")
            };
        }

        public static string GetBySeasonsAnalysedResultsFile()
        {
            return Path.Combine(GetAnalysedArchivedDataFolderPath(), "results_byseasons.csv");
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
    }
}
