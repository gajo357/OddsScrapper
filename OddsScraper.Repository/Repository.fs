namespace OddsScraper.Repository

module Repository =
    open FSharp.Data.Sql
    open Models
    open System.Linq
    
    [<Literal>]
    let resolutionPath = 
        __SOURCE_DIRECTORY__ + @"\..\packages\System.Data.SQLite.Core.1.0.112.0\lib\net46\"
    
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

        let createGame (game: Game) (league: Id) = 
            let item = games.Create()
            item.FkGamesTeamsHomeTeamId <- game.HomeTeam.Id
            item.FkGamesTeamsAwayTeamId <- game.AwayTeam.Id
            item.FkGamesLeaguesId <- league
            item.HomeTeamScore <- game.HomeScore
            item.AwayTeamScore <- game.AwayScore
            item.Date <- game.Date 
            item.Season <- game.Season
            item.IsOvertime <- game.IsOvertime
            item.IsPlayoffs <- game.IsPlayoffs
            item.GameLink <- game.GameLink
            item
        
        let createGameOdd (gameOdd: GameOdd) =
            let item = gameOdds.Create()
            item.FkGameOddsGamesId <- gameOdd.Game
            item.FkGameOddsBookkeepersId <- gameOdd.Bookkeeper.Id
            item.HomeOdd <- gameOdd.HomeOdd
            item.DrawOdd <- gameOdd.DrawOdd
            item.AwayOdd <- gameOdd.AwayOdd
            item.IsValid <- gameOdd.IsValid
            item
        
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
                select ({IdName = {Id = item.Id; Name = item.Name}; Sport = sport; Country = country })
                exactlyOne
            }
        member this.getLeagueAsync sport country name =
            async {
                return this.getLeague sport country name
            } |> Async.StartAsTask

        member __.getTeam name sport =
            query {
                for item in teams do
                where (item.Name = name && item.FkTeamsSportsId = sport)
                select (Some {Id = item.Id; Name = item.Name})
                exactlyOneOrDefault
            }
        member this.getTeamAsync name sport =
            async {
                return this.getTeam name sport
            } |> Async.StartAsTask
        
        member __.getSportById id =
            query {
                for item in sports do
                where (item.Id = id)
                select ({ Id = id; Name = item.Name })
                exactlyOne
            }
        member __.getCountryById id =
            query {
                for item in countries do
                where (item.Id = id)
                select ({ Id = id; Name = item.Name })
                exactlyOne
            }  
        member __.getLeagueById id =
            query {
                for item in leagues do
                where (item.Id = id)
                select ( 
                    { IdName = { Id = id; Name = item.Name };
                    Sport = item.FkLeaguesSportsId;
                    Country = item.FkLeaguesCountriesId})
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
                sortBy game.Date
                let homeTeam = this.getTeamById game.FkGamesTeamsHomeTeamId
                let awayTeam = this.getTeamById game.FkGamesTeamsAwayTeamId
                select (
                    { 
                        HomeTeam = homeTeam; AwayTeam = awayTeam
                        HomeScore = game.HomeTeamScore; AwayScore = game.AwayTeamScore
                        Date = game.Date; Season = game.Season
                        IsOvertime = game.IsOvertime; IsPlayoffs = game.IsPlayoffs
                        GameLink = game.GameLink
                    }, 
                    this.getGameOddsById game.Id)
            }
        member this.getAllLeagueGamesAsync league =
            async {
                return! this.getAllLeagueGames league |> Seq.executeQueryAsync
            } |> Async.StartAsTask
        member this.getAllLeagueGamesGroupedBySeason league =
            query {
                for g in this.getAllLeagueGames league do
                groupBy ((g |> fst).Season)
            }
        member this.getLeaguesSportAndCountry leagueId =
            let league = this.getLeagueById leagueId
            let sport = this.getSportById league.Sport
            let country = this.getCountryById league.Country
            (sport, country)
        member this.getLeaguesSportAndCountryAsync leagueId =
            async {
                return (this.getLeaguesSportAndCountry leagueId)
            } |> Async.StartAsTask
        
        member __.getAllLeagues() =
            query {
                for item in leagues do
                select ( 
                    { IdName = { Id = item.Id; Name = item.Name };
                    Sport = item.FkLeaguesSportsId;
                    Country = item.FkLeaguesCountriesId})
            }

        member __.createSport name = sports.Create([("Name", name)]) |> ignore
        member __.createCountry name = countries.Create([("Name", name)]) |> ignore

        member this.createBookkeeper name = 
            let item = bookkeepers.Create([("Name", name)]) 
            this.submit()
            item |> (fun item -> { Id = item.Id; Name = item.Name })
        
        member __.createLeague (league: League) = leagues.Create() |> ignore

        member this.insertGameAsync game league = 
            async {
                let item = createGame game league
                do! this.submitAsync()
                return (item.GetColumn("Id") : Id)
            } |> Async.StartAsTask

        member this.insertGameOddsAsync (gameOdds: GameOdd list) = 
            async {
                gameOdds |> List.map createGameOdd |> ignore
                do! this.submitAsync()
            } |> Async.StartAsTask
        
        member this.createTeam name sport = 
            let item = teams.Create([("Name", name); ("FkTeamsSportsId", sport)]) 
            this.submit()
            item |> (fun item -> { Id = item.Id; Name = item.Name })
        
        member __.updateGameLeague game league =
            let dbGame = 
                query {
                    for item in games do
                    where (item.Id = game)
                    exactlyOne
                }
            dbGame.FkGamesLeaguesId <- league

        member __.submit = ctx.SubmitUpdates
        member __.submitAsync = ctx.SubmitUpdatesAsync

        member this.fixGames getLeagueName =
            let leaguesToDelete =
                query {
                    for item in leagues do
                    where (item.Id <> int64 44 && item.Name = "aff-championship-u18")
                }
            let leaguesIds = leaguesToDelete |> Seq.map (fun l -> l.Id) |> Seq.toArray
            let gamesToChange =
                query {
                    for item in games do
                    where (item.FkGamesLeaguesId |=| leaguesIds)
                }
            gamesToChange
            |> Seq.iter (fun item ->
                let (sport, country) = this.getLeaguesSportAndCountry item.FkGamesLeaguesId
                let leagueName = getLeagueName sport.Name country.Name item.GameLink
                let league = this.getLeague sport.Id country.Id  leagueName
                item.FkGamesLeaguesId <- league.IdName.Id)

            leaguesToDelete
            |> Seq.``delete all items from single table``
            |> Async.RunSynchronously
            |> ignore
            
            ctx.SubmitUpdates()
        

        