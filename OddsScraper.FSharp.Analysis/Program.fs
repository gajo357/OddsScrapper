open OddsScraper.Repository.Repository
open OddsScraper.FSharp.Analysis.ProcessLeagues
open OddsScraper.FSharp.Common.Common
open OddsScraper.FSharp.Common.OptionExtension

// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.



[<EntryPoint>]
let main argv = 

    let repository = Project(@"../ArchiveData.db")
    option {
        let! choice = GetUserInputAsInt "Choose 1 to process all leagues, 2 for single"

        if choice = 1 then
            processAll repository
        else 
            processSelected repository
    } |> ignore
    
    printfn "%A" argv
    0 // return an integer exit code
