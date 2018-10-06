namespace OddsScraper.FSharp.Scraping

module RepositoryMethods =
    open OddsScraper.Repository.Repository
    open OddsScraper.Repository.Models
    open OddsScraper.FSharp.CommonScraping

    open GamePageReading
    open HtmlNodeExtensions
    open ScrapingParts

    let GetSportAndLeagueAsync (repository:Project) seasonLink leagueName =
        async {
            let (sportName, countryName, _) = seasonLink |> ExtractSportCountryAndLeagueFromLink
        
            let! sport = repository.getSportAsync sportName |> Async.AwaitTask
            let! country = repository.getCountryAsync countryName  |> Async.AwaitTask
            let! league = repository.getLeagueAsync sport.Value.Id country.Value.Id leagueName |> Async.AwaitTask

            return (sport, league)
        }

    let GetOrCreateTeamAsync (repository:Project) sport name = 
        async {
            let! team = (repository.getTeamAsync name sport) |> Async.AwaitTask
            
            match team with
            | Some b -> return b
            | None -> return repository.createTeam name sport
        }

    let GetParticipants (repository:Project) sport participantsAndDateElement =
        async {
            let (homeTeamName, awayTeamName) = readParticipantsNames participantsAndDateElement

            let getOrCreateTeam = GetOrCreateTeamAsync repository sport
            let! homeTeam = getOrCreateTeam homeTeamName
            let! awayTeam = getOrCreateTeam awayTeamName

            return (homeTeam, awayTeam)
        }

    let rec GetOrCreateBookkeeperAsync (repository:Project) name =
        async {
            let! bookerOpt = repository.getBookkeeperAsync name |> Async.AwaitTask
         
            match bookerOpt with
            | Some b -> return b
            | None -> return repository.createBookkeeper name
        }

    let ConvertToGameOddAsync (repository:Project) gameId (odd:Odd) =
        async {
            let! booker = GetOrCreateBookkeeperAsync repository odd.Name
        
            let (homeOdd, drawOdd, awayOdd) = convertOddsListTo1x2 odd
        
            let gameOdd = {
                Game = gameId
                HomeOdd = homeOdd
                DrawOdd = drawOdd
                AwayOdd = awayOdd
                IsValid = (not odd.Deactivated)
                Bookkeeper = booker
            }
            return gameOdd
        }

    let CreateOddsAsync (repository:Project) gameId (odds:Odd[]) =
        async {
            return 
                odds
                |> Seq.map (ConvertToGameOddAsync repository gameId)
                |> Seq.map Async.RunSynchronously
                |> Seq.toList
        }

    let GameExistsAsync (repository:Project) homeTeam awayTeam gameDate =
        async {
            return! repository.gameExistsAsync homeTeam awayTeam gameDate |> Async.AwaitTask
        }
    
    let GameLinkExistsAsync (repository:Project) gameLink =
        async {
            return! repository.gameLinkExistsAsync gameLink |> Async.AwaitTask
        }

    let ReadGameAsync (repository:Project) link season sport gameHtml =
        async {
            let participantsAndDateElement = getElementById "#col-content" gameHtml
            let! (homeTeam, awayTeam) = (GetParticipants repository sport participantsAndDateElement)
            let gameDate = participantsAndDateElement |> (readGameDate >> getDateOrDefault)
                    
            let (homeScore, awayScore, isOvertime) = readGameScore (getElementById "#event-status" gameHtml)
            
            return {
                    GameLink = link; Season = season
                    IsOvertime = isOvertime; IsPlayoffs = false //isPlayoffs
                    HomeScore = homeScore; AwayScore = awayScore
                    HomeTeam = homeTeam; AwayTeam = awayTeam
                    Date = gameDate
                }
        }

    let InsertGameAsync (repository:Project) game league =
        async {
            return! (repository.insertGameAsync game league) |> Async.AwaitTask
        }
    
    let InsertGameOddsAsync (repository:Project) (odds: GameOdd list) =
        async {
            do! (repository.insertGameOddsAsync odds) |> Async.AwaitTask
        }

