open OddsScraper.CreateDataTables
open OddsScraper.Repository.Repository
open System
open OddsScraper.FSharp.Common
open Common
open OptionExtension
open Printing
open OddsScraper.Repository.Models

type WriterAgent(write) =
    let agent = MailboxProcessor<string>.Start(fun inbox -> async {
        let rec loop () = async {
            let! message = inbox.Receive()

            write message

            return! loop()
        }

        return! loop()
    })
    
    member __.WriteAsync line = agent.PostAndAsyncReply(fun _ -> line)
    member __.Write line = agent.Post line

let appendLinesToFile fileName lines = System.IO.File.AppendAllLines(fileName, lines)
let appendLineToFile fileName line = appendLinesToFile fileName [line]

let getForLeague() = 
    option {
        let sportName = GetUserInput "Choose sport: "
        let repository = Project(@"..\ArchiveData_" + sportName + ".db")
        
        let! (sport, country, league) = chooseLeague repository sportName
        
        let fileName = String.Format(@"..\{0}_{1}_{2}.csv", sport.Name, country.Name, league.IdName.Name)
        
        System.IO.File.WriteAllLines(fileName, [Header])

        (repository.getAllLeagueGames league.IdName.Id) 
        |> Seq.iter (fun (game, odds) -> 
            odds 
            |> Seq.map (FormatGame game)
            |> appendLinesToFile fileName)

        return league
    } |> ignore
    
[<EntryPoint>]
let main argv = 
    let meanBookies = [ "bwin"; "Pinnacle"; "William Hill"; "bet365" ]
    let fileName = "Gaussians.csv"

    let consoleWriter = WriterAgent(System.Console.WriteLine)
    let fileWriter = WriterAgent(appendLineToFile fileName)
    
    consoleWriter.Write "Started"
    fileWriter.Write (JoinValues [ "Sport"; "Country"; "League"; "Winner Mu"; "Winner Sigma"; "Winner Factor"; "Losers Mu"; "Losers Sigma"; "Losers Factor";  ])

    OddsScraper.CreateDateTables.GetAnalysisData.run consoleWriter.WriteAsync fileWriter.WriteAsync |> ignore

    //let repository = Project(@"..\ArchiveData_fullData.db")
    //repository.getAllLeagues()
    //|> Seq.filter (fun league -> league.Sport = int64 1)  // only football for now
    //|> Seq.iter (fun league -> 
    //        let sport = repository.getSportById league.Sport
    //        let country = repository.getCountryById league.Country

    //        consoleWriter.WriteAsync(sprintf "%s,%s,%s" sport.Name country.Name league.IdName.Name) |> Async.Start

    //        let wl =
    //            repository.getAllLeagueGames league.IdName.Id
    //            |> Seq.filter (fun (g, _) -> g.Date.Year > 2007)
    //            |> Seq.map (fun (game, odds) -> 
    //                option {
    //                    let meanOdds = 
    //                        odds 
    //                        |> Seq.filter (fun o -> o.IsValid && meanBookies |> List.contains o.Bookkeeper.Name) 
    //                        |> Seq.toArray
    //                    let! myBookie = meanOdds |> Array.tryFind (fun o -> o.Bookkeeper.Name = "bet365")
    //                    let meanOdds = meanOdds |> Array.map (fun o -> normalizeOdds o.HomeOdd o.DrawOdd o.AwayOdd)
    //                    let psychOdds = {
    //                        Game = int64 1
    //                        Bookkeeper = { Name = ""; Id = int64 1 }
    //                        IsValid = true

    //                        HomeOdd = meanOdds |> meanFromFunc (fun (h, _, _) -> h) |> psychFunc
    //                        DrawOdd = meanOdds |> meanFromFunc (fun (_, d, _) -> d) |> psychFunc
    //                        AwayOdd = meanOdds |> meanFromFunc (fun (_, _, a) -> a) |> psychFunc
    //                    }

    //                    let k = { 
    //                        psychOdds with
    //                            HomeOdd = kelly psychOdds.HomeOdd myBookie.HomeOdd
    //                            DrawOdd = kelly psychOdds.DrawOdd myBookie.DrawOdd
    //                            AwayOdd = kelly psychOdds.AwayOdd myBookie.AwayOdd
    //                    }

    //                    return!
    //                        if k.HomeOdd > 0. then
    //                            Some (game.HomeScore > game.AwayScore, psychOdds.HomeOdd)
    //                        else if k.DrawOdd > 0. then
    //                            Some (game.HomeScore = game.AwayScore, psychOdds.DrawOdd)
    //                        else if k.AwayOdd > 0. then
    //                            Some (game.HomeScore < game.AwayScore, psychOdds.AwayOdd)
    //                        else None
    //                }
    //            )
    //            |> Seq.choose id
    //            |> Seq.toArray

    //        consoleWriter.WriteAsync("Read all games") |> Async.Start

    //        let winners = wl |> Array.filter fst |> Array.map snd
    //        let losers = wl |> Array.filter (fst >> not) |> Array.map snd

    //        let binSize = 0.02
    //        let binFunc v = floor(v / binSize) * binSize
    //        let sortToBins = Array.groupBy binFunc >> Array.map (fun (bin, games) -> bin, games |> Seq.length) >> Array.sortBy fst

    //        let wBins = sortToBins winners
    //        let lBins = sortToBins losers

    //        let wMu, wSigma = muStdDev winners
    //        let lMu, lSigma = muStdDev losers
    //        let wnMu = binSize * (wBins |> Array.sumBy snd |> float)
    //        let lnMu = binSize * (lBins |> Array.sumBy snd |> float)
        
    //        let line = JoinValues [ sport.Name; country.Name; league.IdName.Name; wMu; wSigma; wnMu; lMu; lSigma; lnMu ]
    //        fileWriter.Write line
    //)

    0 // return an integer exit code
