namespace OddsScraper.FSharp.Analysis

module LeagueProcessing =
    open OddsScraper.FSharp.Common.Common
    open OddsScraper.Repository.Models
    open OddsScraper.FSharp.Analysis.Calculations

    let myBookies = [|"bet365"; "888sport"; "bwin"; "Unibet"|]
    let meanBookies = ["bet365"; "bwin"; "Pinnacle"; "888sport"; "Unibet"; "William Hill"]

    let Separator = ","
    let JoinValues = Join Separator


    let isImportant odd = meanBookies |> Seq.contains odd.Bookkeeper.Name
    let takeImportantOdds odds = odds |> Seq.filter isImportant |> Seq.toList
    let filterOdds games = games |> Seq.map (fun (g, odds) ->  (g, takeImportantOdds odds))
    let getSortedGames games = 
        games 
        |> filterOdds
        |> Seq.groupBy (fun (g, _) -> g.Season)
        |> Seq.toList
    let startingAmount = 500.
    let devideByStartingAmount v = v/startingAmount
    let getValueForSeasons seasons b = 
        (b, 
            seasons 
            |> Seq.map snd
            |> Seq.map (betAll startingAmount b) 
            |> Seq.map devideByStartingAmount 
            |> Seq.toList)

    let formatSeasonResultsForBookie (bookie, results) =
        let values = results |> Seq.map (sprintf "%f") |> Seq.toArray
        [|bookie|] |> (Array.append values) |> JoinValues

    let processLeague sport country league seasons =
        let fileName = System.String.Format("..\{0}_{1}_{2}_bookies.csv", sport.Name, country.Name, league.IdName.Name)
        let write lines = System.IO.File.AppendAllLines(fileName, lines)
    
        let header seasons = [|"Bookkeeper/Season";|] |> (Array.append seasons) |> JoinValues
        [header (seasons |> Seq.map fst |> Seq.toArray)] |> write
    
        myBookies 
        |> Seq.map (getValueForSeasons seasons)
        |> Seq.map formatSeasonResultsForBookie
        |> write

