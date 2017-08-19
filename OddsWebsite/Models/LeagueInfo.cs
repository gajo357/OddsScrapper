namespace OddsWebsite.Models
{
    public class LeagueInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Sport Sport { get; set; }
        public Country Country { get; set; }
    }
}
