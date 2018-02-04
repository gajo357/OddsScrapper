namespace OddsScrapper.Shared.Models
{
    public static class ModelExtensions
    {
        private static string[] CupNames = new[] { "cup", "copa", "cupen", "coupe", "coppa" };
        private const string Women = "women";

        public static bool IsWomen(this League league)
        {
            return league.Name.Contains(Women);
        }

        public static bool IsCup(this League league)
        {
            foreach (var cup in CupNames)
            {
                if (league.Name.Contains(cup))
                    return true;
            }

            return false;
        }

        public static int GetResult(this Game game)
        {
            if (game.HomeTeamScore > game.AwayTeamScore)
                return 1;

            if (game.HomeTeamScore < game.AwayTeamScore)
                return 2;

            return 0;
        }
    }
}
