namespace OddsScraper.CreateDataTables

module Printing =
    open System
    open Common
    open OddsScraper.Repository.Models

    let private Separator = ","
    let private JoinValues = Join Separator

    let Header = 
        JoinValues ["HomeTeam"; "AwayTeam"; "HomeTeamScore"; "AwayTeamScore";
                    "Date"; "Season"; "IsPlayoffs"; "IsOvertime";
                    "Bookmaker"; "HomeOdd"; "DrawOdd"; "AwayOdd"; "IsValid"]

    let FormatGame (game:Game) (gameOdd:GameOdd) =
        JoinValues [game.HomeTeam.Name; game.AwayTeam.Name; game.HomeScore; game.AwayScore;
                    game.Date; game.Season; game.IsPlayoffs; game.IsOvertime;
                    gameOdd.Bookkeeper.Name; gameOdd.HomeOdd; gameOdd.DrawOdd; gameOdd.AwayOdd; gameOdd.IsValid]

    let GetUserInput (message:String) =
        WriteToConsole message
        Console.ReadLine()

    let PrintLeagues (leagues: League list) =
        WriteToConsole "Possible leagues"
        leagues
        |> Seq.mapi (fun i l -> String.Format("{0}: {1}", i, l.IdName.Name))
        |> Seq.iter WriteToConsole

    let SelectLeague (leagues: League list) =
        PrintLeagues leagues
        let userChoice = (GetUserInput "\nType desired league index") |> IntegerInString
        match userChoice with
        | Some i -> 
            if i < 0 || i >= List.length leagues then
                None
            else 
                Some leagues.[i]
        | _ -> None