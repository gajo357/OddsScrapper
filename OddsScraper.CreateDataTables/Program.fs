open OddsScraper.CreateDataTables
open OddsScraper.Repository.Repository
open System

open OddsScraper.FSharp.Common.OptionExtension
open Printing

[<EntryPoint>]
let main argv = 
    let repository = Project(@"../ArchiveData.db")
    
    option {
        
        let! (sport, country, league) = chooseLeague repository
        
        let fileName = String.Format("..\{0}_{1}_{2}.csv", sport.Name, country.Name, league.IdName.Name)
        
        System.IO.File.WriteAllLines(fileName, [Header])
        let writeToFile lines = System.IO.File.AppendAllLines(fileName, lines)

        (repository.getAllLeagueGames league.IdName.Id) 
        |> Seq.iter (fun (game, odds) -> 
            odds 
            |> Seq.map (FormatGame game)
            |> writeToFile)

        return league
    } |> ignore
    
    0 // return an integer exit code
