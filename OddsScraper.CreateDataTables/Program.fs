open OddsScraper.CreateDataTables
open OddsScraper.Repository.Repository
open System

open OptionExtension
open Printing

[<EntryPoint>]
let main argv = 
    let repository = Project(@"../ArchiveData.db")
    
    option {
        
        let! sport = GetUserInput "Choose sport: " |> repository.getSport
        let! country = GetUserInput "Choose country: " |> repository.getCountry
        
        let! league = SelectLeague ((repository.getLeagues sport.Id country.Id) |> Seq.toList)
        //let! league = GetUserInput "Choose league: " |> GetFromRepository repository (GetLeagueAsync sport country)

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
