open OddsScraper.FSharp.Scraping
open OddsScrapper.Repository.Repository
open System

open RepositoryMethods
open OptionExtension
open Common
open OddsScrapper.Repository.Models

// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

let Separator = ","
let JoinValues = Join Separator

let Header = 
    JoinValues ["HomeTeam"; "AwayTeam"; "HomeTeamScore"; "AwayTeamScore";
                "Date"; "Season"; "IsPlayoffs"; "IsOvertime";
                "Bookmaker"; "HomeOdd"; "DrawOdd"; "AwayOdd"; "IsValid"]

let FormatGame (game:Game) (gameOdd:GameOdds) =
    JoinValues [game.HomeTeam.Name; game.AwayTeam.Name; game.HomeTeamScore; game.AwayTeamScore;
                game.Date; game.Season; game.IsPlayoffs; game.IsOvertime;
                gameOdd.Bookkeeper.Name; gameOdd.HomeOdd; gameOdd.DrawOdd; gameOdd.AwayOdd; gameOdd.IsValid]

let GetUserInput (message:String) =
    WriteToConsole message
    Console.ReadLine()

let PrintLeagues (leagues: League list) =
    WriteToConsole "Possible leagues"
    leagues
    |> Seq.mapi (fun i l -> String.Format("{0}: {1}", i, l.Name))
    |> Seq.iter WriteToConsole

let SelectLeague (leagues: League list) =
    PrintLeagues leagues
    let userChoice = (GetUserInput "Type desired league index") |> IntegerInString
    match userChoice with
    | Some i -> 
        if i < 0 || i >= List.length leagues then
            None
        else 
            Some leagues.[i]
    | _ -> None

[<EntryPoint>]
let main argv = 
    use repository = new DbRepository(@"../ArchiveData.db")
    
    let opt = OptionBuilder()
    opt {
        let! sport = GetUserInput "Choose sport: " |> GetFromRepository repository GetSportAsync
        let! country = GetUserInput "Choose country: " |> GetFromRepository repository GetCountryAsync

        let! league = SelectLeague ((GetLeaguesAsync sport country repository) |> Async.RunSynchronously |> Seq.toList)
        //let! league = GetUserInput "Choose league: " |> GetFromRepository repository (GetLeagueAsync sport country)

        let fileName = String.Format("..\{0}_{1}_{2}.csv", sport.Name, country.Name, league.Name)
        
        System.IO.File.WriteAllLines(fileName, [Header])
        let writeToFile lines = System.IO.File.AppendAllLines(fileName, lines)

        (GetAllGames repository league) 
        |> Seq.collect (fun game -> game.Odds |> Seq.map (fun go -> FormatGame game go))
        |> writeToFile

        return league
    } |> ignore
    
    0 // return an integer exit code
