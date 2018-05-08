// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open canopy.classic
open OddsScraper.FSharp.Scraping
open OddsScrapper.Repository.Repository

open Common
open ScrapingParts
open RepositoryMethods

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

let SelectSports() =
    System.Console.Write("Choose sport (1-football, 2-basketall, 3-voleyball, 4-others) :")
    let sport = System.Convert.ToInt32(System.Console.ReadLine())
    match sport with
    | 1 -> Football
    | 2 -> Basketball
    | 3 -> Volleyball
    | _ -> Others

let GetPageHtml link =
    url link
    (element "html").GetAttribute("innerHTML")

let PrintLink link =
    printfn "%A" link

let NavigateAndReadGame readGame insertGame gameLink =
    let action() =
        url gameLink

        match readGame gameLink with
        | Some game -> 
            insertGame(game) |> ignore
        | None _ -> ()

    InvokeRepeatedIfFailed action

let FindMatchingLeagueLink leagues link =
    leagues
    |> Seq.filter (fun l -> Contains l link)
    |> Seq.maxBy String.length
        
//[<EntryPoint>]
//let main argv = 
//    //start an instance of chrome
//    start chrome

//    System.Console.Write("Enter username: ")
//    let username = System.Console.ReadLine()
//    System.Console.Write("Enter password: ")
//    let password = System.Console.ReadLine()
//    Login username password

//    //System.Console.Write("Choose sport (1-football, 2-basketall, 3-voleyball, 4-others) :")
//    //let sport = System.Convert.ToInt32(System.Console.ReadLine())
//    //let sports =
//    //    match sport with
//    //    | 1 -> Football
//    //    | 2 -> Basketball
//    //    | 3 -> Volleyball
//    //    | _ -> Others
        
//    let sportLinks = Sports |> Seq.map (fun s -> PrependBaseWebsite ("/" + s + "/")) |> Seq.toArray
//    let repository = DbRepository(@"../ArchiveData.db")

//    let currentReadGameAsync = ReadGameAsync repository
//    let currentGetSportCountryAndLeagueAsync = GetSportCountryAndLeagueAsync repository
//    let currentInsertGameAsync = InsertGameAsync repository
    
//    url (ResultsLinkForSport (Sports |> Seq.head))
//    for leagueLink in GetLeaguesLinks sportLinks (element "table") do
//        let (sport, league) = leagueLink |> currentGetSportCountryAndLeagueAsync |> Async.RunSynchronously
//        url leagueLink
        
//        for (seasonLink, season) in GetSeasonsLinks(element "#col-content") do
//            url seasonLink

//            let currentNavigateAndReadGame = NavigateAndReadGame (currentReadGameAsync sport league season) currentInsertGameAsync
//            for pageLink in GetResultsPagesLinks seasonLink (someElement "#pagination") do
//                url pageLink
//                for gameLink in GetGameLinksFromTable(element "#tournamentTable") do
//                    currentNavigateAndReadGame gameLink

//    System.Console.WriteLine("Press any key to exit...")
//    System.Console.Read() |> ignore
//    0 // return an integer exit code

    
[<EntryPoint>]
let main argv = 
    //start an instance of chrome
    start chrome
    
    use repository = new DbRepository(@"../ArchiveData.db")
    let currentReadGameAsync = ReadGame repository
    let currentGetSportCountryAndLeagueAsync = (GetSportCountryAndLeagueAsync repository >> Async.RunSynchronously)
    let currentInsertGameAsync = (InsertGameAsync repository >> Async.RunSynchronously)
    let currentGameExists = (GameLinkExistsAsync repository >> Async.RunSynchronously)

    let sports = SelectSports()       
    let sportLinks = sports |> Seq.map (fun s -> PrependBaseWebsite ("/" + s + "/")) |> Seq.toArray

    let gameStartsWithSportLink gameLink =
        sportLinks |> Seq.exists (fun sl -> StartsWith sl gameLink)
    let games = 
        System.IO.File.ReadLines("..\games.txt")
        |> Seq.filter gameStartsWithSportLink
        |> Seq.filter currentGameExists
        |> Seq.map (Split "\t")
        |> Seq.map (fun n -> (n.[0], n.[1]))
    
    let leagueExistsInSports league = 
        sports |> Seq.exists (fun sport -> Contains ("/" + sport + "/") league)
    let leagues = 
        System.IO.File.ReadLines("..\leagues.txt")
        |> Seq.filter leagueExistsInSports
        |> Seq.map (Remove "/results/")
        |> Seq.toArray

    let readGameForSeason(season, link) =
        url link
        
        let leagueLink = FindMatchingLeagueLink leagues link

        let (sport, league) = leagueLink |> currentGetSportCountryAndLeagueAsync

        NavigateAndReadGame (currentReadGameAsync sport league season) currentInsertGameAsync link
    
    games 
    |> Seq.map (fun g -> fun() -> readGameForSeason g)
    |> Seq.iter InvokeRepeatedIfFailed

    0 // return an integer exit code
