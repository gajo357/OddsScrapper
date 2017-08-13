using System.IO;
using System.Text.RegularExpressions;

namespace OddsScrapper
{
    public static class HelperMethods
    {
        public const string ArchiveFolder = "FullArchive";
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
