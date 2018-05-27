namespace OddsScraper.FSharp.Scraping

module RepositoryMethods =
    open OddsScrapper.Repository.Repository
    open OddsScrapper.Repository.Models
    open OddsScraper.FSharp.Scraping

    open Common
    open GamePageReading
    open HtmlNodeExtensions
    open ScrapingParts

    let GetSportCountryAndLeagueAsync (repository:DbRepository) (leagueLink:string) =
        async {
            let (sportName, countryName, leagueName) = leagueLink |> Remove BaseWebsite |> ExtractSportCountryAndLeagueFromLink
        
            let! sport = repository.GetOrCreateSportAsync(sportName) |> Async.AwaitTask
            let! country = repository.GetOrCreateCountryAsync(countryName) |> Async.AwaitTask
            let! league = repository.GetOrCreateLeagueAsync(leagueName, false, sport, country) |> Async.AwaitTask

            return (sport, league)
        }

    let GetParticipants (repository:DbRepository) sport participantsAndDateElement =
        async {
            let (homeTeamName, awayTeamName) = ReadParticipantsNames participantsAndDateElement

            let! homeTeam = repository.GetOrCreateTeamAsync(homeTeamName, sport) |> Async.AwaitTask
            let! awayTeam = repository.GetOrCreateTeamAsync(awayTeamName, sport) |> Async.AwaitTask

            return (homeTeam, awayTeam)
        }

    let ConvertToGameOddAsync (repository:DbRepository) (odd:Odd) =
        async {
            let! booker = repository.GetOrCreateBookerAsync(odd.Name) |> Async.AwaitTask
        
            let (homeOdd, drawOdd, awayOdd) =
                match odd.Odds with
                | [home; away] -> (home, 0.0, away)
                | [home; draw; away] -> (home, draw, away)
                | _ -> (0.0, 0.0, 0.0)
        
            let gameOdd = GameOdds()
            gameOdd.HomeOdd <- homeOdd
            gameOdd.DrawOdd <- drawOdd
            gameOdd.AwayOdd <- awayOdd
            gameOdd.IsValid <- (not odd.Deactivated)
            gameOdd.Bookkeeper <- booker

            return gameOdd
        }

    let CreateOddsAsync (repository:DbRepository) (odds:Odd[]) =
        async {
            return 
                odds
                |> Seq.map (ConvertToGameOddAsync repository)
                |> Seq.map Async.RunSynchronously
                |> Seq.toList
        }

    let GameExistsAsync (repository:DbRepository) homeTeam awayTeam gameDate =
        async {
            return! repository.GameExistsAsync(homeTeam, awayTeam, gameDate) |> Async.AwaitTask
        }
    
    let GameLinkExistsAsync (repository:DbRepository) gameLink =
        async {
            return! repository.GameExistsAsync(gameLink) |> Async.AwaitTask
        }

    let ReadGame (repository:DbRepository) (game: Game) sport gameHtml =
        let participantsAndDateElement = GetElementById "#col-content" gameHtml
        let (homeTeam, awayTeam) = (GetParticipants repository sport participantsAndDateElement) |> Async.RunSynchronously
        let gameDate = ReadGameDate participantsAndDateElement
                    
        let (homeScore, awayScore, isOvertime) = ReadGameScore (GetElementById "#event-status" gameHtml)
        let odds = GetOddsFromGamePage gameHtml
            
        game.IsOvertime <- isOvertime
        //game.IsPlayoffs <- isPlayoffs
        game.HomeTeamScore <- homeScore
        game.AwayTeamScore <- awayScore
        game.HomeTeam <- homeTeam
        game.AwayTeam <- awayTeam
        game.Odds.AddRange((CreateOddsAsync repository odds) |> Async.RunSynchronously)
        game.Date <- ConvertOptionToNullable gameDate

    let InsertGameAsync (repository:DbRepository) game =
        async {
            (repository.InsertGameAsync game) |> Async.AwaitTask |> ignore
        }

