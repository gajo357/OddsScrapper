// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open canopy.classic
open OddsScraper.FSharp.Scraping
open OddsScrapper.Repository.Repository
open OddsScrapper.Repository.Models

open ScrapingParts
open NodeExtensions
open OpenQA.Selenium
open OddsScraper.FSharp.Scraping

let Football = ["soccer"]
let Basketball = ["basketball"]
let Volleyball = ["volleyball"]
let Others = ["handball"; "hockey"; "baseball"; "american-football"; "rugby-league"; "rugby-union"; "water-polo"]
let Sports = ["soccer"; "basketball"; "handball"; "hockey"; "baseball"; "american-football"; "rugby-league"; "rugby-union"; "water-polo"; "volleyball"]

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

let GetSportCountryAndLeagueAsync (repository:DbRepository) (leagueLink:string) =
    async {
        let (sportName, countryName, leagueName) = ExtractSportCountryAndLeagueFromLink (leagueLink.Replace(BaseWebsite, System.String.Empty))
        
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

let ReadGameAsync (repository:DbRepository) sport league season gameLink =
    let participantsAndDateElement = (element "#col-content")
    let (homeTeam, awayTeam) = (GetParticipants repository sport participantsAndDateElement) |> Async.RunSynchronously
    let gameDate = ReadGameDate participantsAndDateElement
                    
    if not ((GameExistsAsync repository homeTeam awayTeam gameDate) |> Async.RunSynchronously) then
        let (homeScore, awayScore, isOvertime) = ReadGameScore (element "#event-status")
        let odds = GetOddsFromGamePage (element "#odds-data-table")

        let game = Game()
        game.IsOvertime <- isOvertime
        //game.IsPlayoffs <- isPlayoffs
        game.HomeTeamScore <- homeScore
        game.AwayTeamScore <- awayScore
        game.HomeTeam <- homeTeam
        game.AwayTeam <- awayTeam
        game.League <- league
        game.Season <- season
        game.GameLink <- gameLink
        game.Odds.AddRange((CreateOddsAsync repository odds) |> Async.RunSynchronously)
        game.Date <- (gameDate |> System.Nullable<System.DateTime>)

        Some game
    else
        None

let InsertGameAsync (repository:DbRepository) game =
    async {
        (repository.InsertGameAsync game) |> Async.AwaitTask |> ignore
    }

let rec NavigateAndReadGame currentReadGameAsync currentInsertGameAsync gameLink timesTried =
    if timesTried < 5 then
        try
            url gameLink

            match currentReadGameAsync gameLink with
            | Some game -> 
                currentInsertGameAsync(game) |> Async.RunSynchronously |> ignore
            | None _ -> ()
        with
        | _ -> NavigateAndReadGame currentReadGameAsync currentInsertGameAsync gameLink (timesTried + 1)

[<EntryPoint>]
let main argv = 
    //start an instance of chrome
    start chrome

    System.Console.Write("Enter username: ")
    let username = System.Console.ReadLine()
    System.Console.Write("Enter password: ")
    let password = System.Console.ReadLine()
    Login username password

    //System.Console.Write("Choose sport (1-football, 2-basketall, 3-voleyball, 4-others) :")
    //let sport = System.Convert.ToInt32(System.Console.ReadLine())
    //let sports =
    //    match sport with
    //    | 1 -> Football
    //    | 2 -> Basketball
    //    | 3 -> Volleyball
    //    | _ -> Others
        
    let sportLinks = Sports |> Seq.map (fun s -> PrependBaseWebsite ("/" + s + "/")) |> Seq.toArray
    let repository = DbRepository(@"../ArchiveData.db")

    let currentReadGameAsync = ReadGameAsync repository
    let currentGetSportCountryAndLeagueAsync = GetSportCountryAndLeagueAsync repository
    let currentInsertGameAsync = InsertGameAsync repository
    
    url (ResultsLinkForSport (Sports |> Seq.head))
    for leagueLink in GetLeaguesLinks sportLinks (element "table") do
        let (sport, league) = leagueLink |> currentGetSportCountryAndLeagueAsync |> Async.RunSynchronously
        url leagueLink
        
        for (seasonLink, season) in GetSeasonsLinks(element "#col-content") do
            url seasonLink

            let currentNavigateAndReadGame = NavigateAndReadGame (currentReadGameAsync sport league season) currentInsertGameAsync
            for pageLink in GetResultsPagesLinks seasonLink (someElement "#pagination") do
                url pageLink
                for gameLink in GetGameLinksFromTable(element "#tournamentTable") do
                    currentNavigateAndReadGame gameLink 1

    System.Console.WriteLine("Press any key to exit...")
    System.Console.Read() |> ignore
    0 // return an integer exit code
    
//[<EntryPoint>]
//let main argv = 
//    //start an instance of chrome
//    start chrome

//    let pages = 
//        System.IO.File.ReadLines("..\pages.txt")
//        |> Seq.map (Common.Split "\t")
//        |> Seq.map (fun n -> (n.[0], n.[1]))
    
//    let join first second = System.String.Format("{0}\t{1}", first, second)
//    let appendToFile lines = System.IO.File.AppendAllLines("..\games.txt", lines)

//    for (season, pageLink) in pages do
//        url pageLink

//        let joinSeason = join season

//        GetGameLinksFromTable(element "#tournamentTable")
//        |> Seq.map joinSeason
//        |> appendToFile

//    0 // return an integer exit code
