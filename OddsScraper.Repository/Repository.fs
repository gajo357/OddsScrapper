namespace OddsScraper.Repository

module Repository =
    open FSharp.Data.Sql
    open Models

    [<Literal>]
    let resolutionPath = 
        __SOURCE_DIRECTORY__ + @"\..\packages\System.Data.SQLite.Core.1.0.108.0\lib\net46\"
    
    [<Literal>]
    let private connectionString = 
        "Data Source=" + 
        __SOURCE_DIRECTORY__ + "\..\ArchiveData.db;" + 
        "Version=3"

    type private sqlProvider = 
        SqlDataProvider<
            Common.DatabaseProviderTypes.SQLITE,
            connectionString,
            //ResolutionPath = resolutionPath, 
            CaseSensitivityChange = Common.CaseSensitivityChange.ORIGINAL
        >
    
    let private connect = ConnectionString.createConnectionString >> sqlProvider.GetDataContext

    type Project(path) =
        let ctx = connect path

        let sports = ctx.Main.Sports
        let countries = ctx.Main.Countries
        let bookkeepers = ctx.Main.Bookkeepers
        let leagues = ctx.Main.Leagues
        let teams = ctx.Main.Teams
        let games = ctx.Main.Games
        let gameOdds = ctx.Main.GameOdds
        
        member __.getSport name =
            query {
                for item in sports do
                where (item.Name = name)
                select (Some { Id = item.Id; Name = item.Name} )
                exactlyOneOrDefault
            }
        member this.getSportAsync name =
            async {
                return this.getSport name
            } |> Async.StartAsTask
        
        member __.getCountry name =
            query {
                for item in countries do
                where (item.Name = name)
                select (Some {Id = item.Id; Name = item.Name })
                exactlyOneOrDefault
            }
        member this.getCountryAsync name =
            async {
                return this.getCountry name
            } |> Async.StartAsTask
        
        member __.getBookkeeper name =
            query {
                for item in bookkeepers do
                where (item.Name = name)
                select (Some {Id = item.Id; Name = item.Name })
                exactlyOneOrDefault
            }
        member this.getBookkeeperAsync name =
            async {
                return this.getBookkeeper name
            } |> Async.StartAsTask

        member __.getLeagues sport country =
            query {
                for item in leagues do
                where (item.FkLeaguesSportsId = sport && item.FkLeaguesCountriesId = country)
                select {IdName = {Id = item.Id; Name = item.Name}; Sport = item.FkLeaguesSportsId; Country = item.FkLeaguesCountriesId}
            }
        member this.getLeaguesAsync sport country =
            async {
                return! this.getLeagues sport country |> Seq.executeQueryAsync
            } |> Async.StartAsTask

        member __.getLeague sport country name =
            query {
                for item in leagues do
                where (item.FkLeaguesSportsId = sport && item.FkLeaguesCountriesId = country && item.Name = name)
                select (Some {IdName = {Id = item.Id; Name = item.Name}; Sport = sport; Country = country })
                exactlyOneOrDefault
            }
        member this.getLeagueAsync sport country name =
            async {
                return this.getLeague sport country name
            } |> Async.StartAsTask
            
        member __.getLeagueById id =
            query {
                for item in leagues do
                where (item.Id = id)
                select ({ Id = id; Name = item.Name })
                exactlyOne
            }
        member __.getBookkeeperById id =
            query {
                for item in bookkeepers do
                where (item.Id = id)
                select ({ Id = id; Name = item.Name })
                exactlyOne
            }
        member __.getTeamById id =
            query {
                for item in teams do
                where (item.Id = id)
                select ({ Id = id; Name = item.Name })
                exactlyOne
            }
        member this.getGameOddsById id =
            query {
                for odd in gameOdds do
                where (odd.FkGameOddsGamesId = id)
                select (
                    { 
                        Game = id; Bookkeeper = this.getBookkeeperById odd.FkGameOddsBookkeepersId
                        HomeOdd = odd.HomeOdd; DrawOdd = odd.DrawOdd; AwayOdd = odd.AwayOdd
                        IsValid = odd.IsValid
                    })
            }

        member __.gameLinkExists gameLink =
            query {
                for game in games do
                exists (game.GameLink = gameLink)
            }
        member __.gameLinkExistsAsync gameLink =
            async {
                let! res = 
                    query {
                        for game in ctx.Main.Games do
                        where (game.GameLink = gameLink)
                    } |> Seq.executeQueryAsync
                    
                return res |> (Seq.isEmpty >> not)
            } |> Async.StartAsTask
            
        member __.gameExistsAsync homeTeam awayTeam date =
            async {
                let! res = 
                    query {
                        for game in games do
                        where (game.FkGamesTeamsHomeTeamId = homeTeam && 
                               game.FkGamesTeamsAwayTeamId = awayTeam && 
                               game.Date = date)
                    } |> Seq.executeQueryAsync
                    
                return res |> (Seq.isEmpty >> not)
            } |> Async.StartAsTask

        member this.getAllLeagueGames league =
            query {
                for game in games do
                where (game.FkGamesLeaguesId = league)
                let homeTeam = this.getTeamById game.FkGamesTeamsHomeTeamId
                let awayTeam = this.getTeamById game.FkGamesTeamsAwayTeamId
                select (
                    { 
                        HomeTeam = homeTeam; AwayTeam = awayTeam
                        HomeScore = game.HomeTeamScore; AwayScore = game.AwayTeamScore
                        Date = game.Date; Season = game.Season
                        IsOvertime = game.IsOvertime; IsPlayoffs = game.IsPlayoffs
                    }, 
                    this.getGameOddsById game.Id)
            }
        member this.getAllLeagueGamesAsync league =
            async {
                return! this.getAllLeagueGames league |> Seq.executeQueryAsync
            } |> Async.StartAsTask

        member __.createSport name = sports.Create([("Name", name)]) |> ignore
        member __.createCountry name = countries.Create([("Name", name)]) |> ignore
        member __.createBookkeeper name = bookkeepers.Create([("Name", name)]) |> ignore
        member __.createLeague (league: League) = leagues.Create() |> ignore
        member __.createGame (game: Game) = games.Create() |> ignore
        member __.createGameOdd (gameOdd: GameOdd) = gameOdds.Create() |> ignore
        
        member __.submit = ctx.SubmitUpdates
        member __.submitAsync = ctx.SubmitUpdatesAsync
        

        