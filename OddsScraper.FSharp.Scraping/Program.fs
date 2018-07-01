// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open canopy.classic
open OddsScraper.FSharp.Scraping
open OddsScrapper.Repository.Repository

open Common
open ScrapingParts
open RepositoryMethods
open OddsScrapper.Repository.Models
open System

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
    js "return document.documentElement.outerHTML" |> string
    //(element "html").GetAttribute("innerHTML")

let NavigateAndReadGameHtml(season, link) =
    match InvokeRepeatedIfFailed (fun () -> GetPageHtml link) with
    | Some gameHtml ->
        Some (season, link, gameHtml |> GamePageReading.ParseGameHtml)
    | None -> None

let FindMatchingLeagueLink leagues link =
    leagues
    |> Seq.find (fun (_:string, l, _:string) -> StartsWith l link)

let download() =
    //start an instance of chrome
    start chrome
    
    use repository = new DbRepository(@"../ArchiveData.db")
    let currentGameExists = (GameLinkExistsAsync repository >> Async.RunSynchronously)

    System.Console.Write("Enter username: ")
    let username = System.Console.ReadLine()
    System.Console.Write("Enter password: ")
    let password = System.Console.ReadLine()
    Login username password

    
    let sports = SelectSports()       
    let sportLinks = sports |> Seq.map (fun s -> PrependBaseWebsite ("/" + s + "/")) |> Seq.toArray

    let gameStartsWithSportLink gameLink =
        sportLinks |> Seq.exists (fun sl -> StartsWith sl gameLink)
    
    let leagueExistsInSports(_, league, _) = 
        sports |> Seq.exists (fun sport -> Contains ("/" + sport + "/") league)
    let leagues = 
        System.IO.File.ReadLines("..\seasons.txt")
        |> Seq.map (Split "\t")
        |> Seq.map (fun n -> (n.[0], n.[1], n.[2]))
        |> Seq.filter leagueExistsInSports
        |> Seq.toArray            

    let parseAndInsertGameAsync(season, link, gameHtml) =
        async {
            try
                let (_, seasonLink, leagueName) = 
                    leagues
                    |> Seq.find (fun (_:string, l, _:string) -> StartsWith l link)
                let! (sport, league) = GetSportAndLeagueAsync repository seasonLink leagueName
            
                let game = new Game()
                game.League <- league
                game.GameLink <- link
                game.Season <- season

                do! ReadGameAsync repository game sport gameHtml
                return! InsertGameAsync repository game
            with _ -> ()
        }

    System.IO.File.ReadLines("..\games.txt")
    |> Seq.map (Split "\t")
    |> Seq.map (fun n -> (n.[0], n.[1])) 
    |> Seq.filter (snd >> gameStartsWithSportLink)
    |> Seq.filter (snd >> currentGameExists >> not)
    |> Seq.map NavigateAndReadGameHtml
    |> Seq.choose id
    |> Seq.iter (parseAndInsertGameAsync >> Async.Start)
    |> ignore

let fixDatabase() =
    use repository = new DbRepository(@"../ArchiveData.db")
    let leagues = 
        System.IO.File.ReadLines("..\seasons.txt")
        |> Seq.map (Split "\t")
        |> Seq.map (
            fun n -> 
                n.[1] |> ExtractSportCountryAndLeagueFromLink |> (fun (s, c, _) -> (s, c)), n.[1], n.[2])
        |> Seq.groupBy (fun (p, _, _) -> p)
        |> dict

    let updateGameLeagueAsync game =
        async {
            (repository.UpdateGameLeagueAsync game) |> Async.AwaitTask |> ignore
        }

    let updateGameLeague (game:Game) =
        async {
            let (_, _, leagueName) = 
                leagues.[(game.League.Sport.Name, game.League.Country.Name)]
                |> Seq.find (fun (_, link, _) -> StartsWith link game.GameLink)
            
            Console.WriteLine(game.League.Name + " __ " + leagueName)
            let! league = repository.GetOrCreateLeagueAsync(leagueName, false, game.League.Sport, game.League.Country) |> Async.AwaitTask
            game.League <- league

            do! updateGameLeagueAsync game
        }

    repository.GetCustomGames()
    |> Seq.map updateGameLeague
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Seq.iter (fun _ -> ())

[<EntryPoint>]
let main argv = 
    //download()
    fixDatabase()
    0 // return an integer exit code
