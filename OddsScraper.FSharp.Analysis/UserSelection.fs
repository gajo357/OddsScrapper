namespace OddsScraper.FSharp.Analysis

module UserSelection =
    open OddsScraper.Repository.Repository
    open OddsScraper.Repository.Models
    open OddsScraper.FSharp.Common
    open Common
    open OptionExtension
    open System

    let printLeagues leagues =
        WriteToConsole "Possible leagues"
        leagues
        |> Seq.mapi (fun i l -> String.Format("{0}: {1}", i, l.IdName.Name))
        |> Seq.iter WriteToConsole

    let selectLeague leagues =
        printLeagues leagues
        let userChoice = GetUserInputAsInt "\nType desired league index"
        match userChoice with
        | Some i -> 
            if i < 0 || i >= List.length leagues then
                None
            else 
                Some leagues.[i]
        | _ -> None

    let chooseLeague (repository: Project) =
        option {
        
            let! sport = GetUserInput "Choose sport: " |> repository.getSport
            let! country = GetUserInput "Choose country: " |> repository.getCountry
        
            let! league = selectLeague ((repository.getLeagues sport.Id country.Id) |> Seq.toList)
            
            return (sport, country, league)
        }

