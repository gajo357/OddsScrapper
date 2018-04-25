// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open canopy.classic
open OddsScraper.FSharp.Scraping
open OddsScrapper.Repository.Repository
open OddsScrapper.Repository.Models

open ScrapingParts

let BaseWebsite = "http://www.oddsportal.com"
//let Sports = ["soccer"]
let Sports = ["soccer"; "basketball"; "tennis"; "handball"; "hockey"; "baseball"; "american-football"; "rugby-league"; "rugby-union"; "water-polo"; "volleyball"]

let PrependBaseWebsite href = System.String.Format("{0}{1}", BaseWebsite, href)
let ResultsLinkForSport (sport:string) = PrependBaseWebsite (System.String.Format("/results/#{0}", sport))

let Login username password = 
    url (PrependBaseWebsite "/login/")

    "#login-username1" << username
    "#login-password1" << password
    
    click (last (text "Login"))

let GetPageHtml link =
    url link
    (element "html").GetAttribute("innerHTML")

let PrintLink link =
    printfn "%A" link

let GetSportCountryAndLeagueAsync (leagueLink:string) (repository:DbRepository) =
    async {
        let (sportName, countryName, leagueName) = ExtractSportCountryAndLeagueFromLink (leagueLink.Replace(BaseWebsite, System.String.Empty))
        
        let! sport = repository.GetOrCreateSportAsync(sportName) |> Async.AwaitTask
        let! country = repository.GetOrCreateCountryAsync(countryName) |> Async.AwaitTask
        let! league = repository.GetOrCreateLeagueAsync(leagueName, false, sport, country) |> Async.AwaitTask

        return (sport, league)
    }

let GetParticipants participantsAndDateElement (repository:DbRepository) sport =
    async {
        let (homeTeamName, awayTeamName) = ReadParticipantsNames participantsAndDateElement

        let! homeTeam = repository.GetOrCreateTeamAsync(homeTeamName, sport) |> Async.AwaitTask
        let! awayTeam = repository.GetOrCreateTeamAsync(awayTeamName, sport) |> Async.AwaitTask

        return (homeTeam, awayTeam)
    }

let ConvertToGameOddAsync (repository:DbRepository) (odd:Odd) =
    async {
        let gameOdd = GameOdds()
        let (homeOdd, drawOdd, awayOdd) =
            match odd.Odds with
            | [home; away] -> (home, 0.0, away)
            | [home; draw; away] -> (home, draw, away)
            | _ -> (0.0, 0.0, 0.0)
        let! booker = repository.GetOrCreateBookerAsync(odd.Name) |> Async.AwaitTask
        
        gameOdd.HomeOdd <- homeOdd
        gameOdd.DrawOdd <- drawOdd
        gameOdd.AwayOdd <- awayOdd
        gameOdd.IsValid <- (not odd.Deactivated)
        gameOdd.Bookkeeper <- booker

        return gameOdd
    }

let CreateOddsAsync (odds:Odd[]) (repository:DbRepository) =
    async {
        return 
            odds
            |> Seq.map (ConvertToGameOddAsync repository)
            |> Seq.map Async.RunSynchronously
            |> Seq.toList
    }

[<EntryPoint>]
let main argv = 
    //start an instance of chrome
    start chrome

    System.Console.Write("Enter username: ")
    let username = System.Console.ReadLine()
    System.Console.Write("Enter password: ")
    let password = System.Console.ReadLine()
    Login username password
        
    let sportLinks = Sports |> Seq.map (fun s -> PrependBaseWebsite ("/" + s + "/")) |> Seq.toArray
    let repository = DbRepository(@"../ArchiveData.db");
    
    url (ResultsLinkForSport (Sports |> Seq.head))
    let leaguesLinks = GetLeaguesLinks sportLinks (element "table")
    for leagueLink in leaguesLinks do
        url leagueLink
        let (sport, league) = (GetSportCountryAndLeagueAsync leagueLink repository) |> Async.RunSynchronously
        
        let seasons = GetSeasonsLinks (elements "div")
        for seasonLink in seasons do
            url seasonLink
            let pagination = GetResultsPagesLinks seasonLink (someElement "#pagination")
            for pageLink in pagination do
                url pageLink
                let gameLinks = GetGameLinksFromTable(element "#tournamentTable")
                for gameLink in gameLinks do
                    url gameLink
                    let odds = GetOddsFromGamePage (element "#odds-data-table")
                    let participantsAndDateElement = (element "#col-content")
                    let (homeTeam, awayTeam) = (GetParticipants participantsAndDateElement repository sport) |> Async.RunSynchronously
                    let gameDate = ReadGameDate participantsAndDateElement
                    let (homeScore, awayScore, isOvertime) = ReadGameScore (element "#event-status")

                    let game = Game()
                    game.IsOvertime <- isOvertime
                    //game.IsPlayoffs <- isPlayoffs
                    game.HomeTeamScore <- homeScore
                    game.AwayTeamScore <- awayScore
                    game.HomeTeam <- homeTeam
                    game.AwayTeam <- awayTeam
                    game.League <- league
                    game.GameLink <- gameLink
                    game.Odds.AddRange((CreateOddsAsync odds repository) |> Async.RunSynchronously)
                    game.Date <- (gameDate |> System.Nullable<System.DateTime>)

                    repository.UpdateOrInsertGameAsync(game) |> ignore

    System.Console.WriteLine("Press any key to exit...")
    System.Console.Read() |> ignore
    0 // return an integer exit code
