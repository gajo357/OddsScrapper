using System.IO;

namespace OddsWebsite.Helpers
{
    public static class FolderStructureHelpers
    {
        public const string ScrapperFolder = "OddsScrapper";

        public const string ArchiveFolder = "Archive";
        public const string TommorowsGamesFolder = "TommorowsGames";
        public const string AnalysedArchivedDataFolder = "AnalysedArchivedData";

        public static string GetArchiveFolderPath()
        {
            return Path.Combine(GetSolutionDirectory(), ScrapperFolder, ArchiveFolder);
        }

        public static string GetAnalysedArchivedDataFolderPath()
        {
            return Path.Combine(GetSolutionDirectory(), ScrapperFolder, AnalysedArchivedDataFolder);
        }

        public static string GetTommorowsGamesFolderPath()
        {
            return Path.Combine(GetSolutionDirectory(), ScrapperFolder, TommorowsGamesFolder);
        }

        private static string SolutionDirectory;
        public static string GetSolutionDirectory()
        {
            if(string.IsNullOrEmpty(SolutionDirectory))
                SolutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;

            return SolutionDirectory;
        }
    }
}
