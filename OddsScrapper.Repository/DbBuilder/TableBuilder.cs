using OddsScrapper.Repository.Repository;

namespace OddsScrapper.Repository.DbBuilder
{
    internal class TableBuilder
    {
        public static Table CreateSportTable()
        {
            return new Table(DbRepository.SportsTable)
                .AddIdColumn()
                .AddNameColumn();
        }

        public static Table CreateCountriesTable()
        {
            return new Table(DbRepository.CountriesTable)
                .AddIdColumn()
                .AddNameColumn();
        }

        public static Table CreateBookerTable()
        {
            return new Table(DbRepository.BookersTable)
                .AddIdColumn()
                .AddNameColumn();
        }

        public static Table CreateLeagueTable()
        {
            return new Table(DbRepository.LeaguesTable)
                .AddIdColumn()
                .AddNameColumn()
                .AddBoolColumn("IsFirst")
                .AddForeignKeyColumn(CreateSportTable())
                .AddForeignKeyColumn(CreateCountriesTable());
        }

        public static Table CreateTeamTable()
        {
            return new Table(DbRepository.TeamsTable)
                .AddIdColumn()
                .AddNameColumn()
                .AddForeignKeyColumn(CreateSportTable());
        }

        public static Table CreateGameTable()
        {
            var teamTable = CreateTeamTable();
            return new Table(DbRepository.GamesTable)
                .AddIdColumn()
                .AddDatetimeColumn("Date")
                .AddIntegerColumn("HomeTeamScore")
                .AddIntegerColumn("AwayTeamScore")
                .AddBoolColumn("IsPlayoffs")
                .AddBoolColumn("IsOvertime")
                .AddTextColumn("Season")
                .AddTextColumn("GameLink")
                .AddForeignKeyColumn(CreateLeagueTable())
                .AddForeignKeyColumn(teamTable, "HomeTeamId")
                .AddForeignKeyColumn(teamTable, "AwayTeamId");
        }

        public static Table CreateGameOddsTable()
        {
            return new Table(DbRepository.GameOddsTable)
                .AddForeignKeyColumn(CreateGameTable(), onDeleteCascade: true)
                .AddForeignKeyColumn(CreateBookerTable())
                .AddDoubleColumn("HomeOdd")
                .AddDoubleColumn("DrawOdd")
                .AddDoubleColumn("AwayOdd")
                .AddBoolColumn("IsValid");
        }
    }
}
