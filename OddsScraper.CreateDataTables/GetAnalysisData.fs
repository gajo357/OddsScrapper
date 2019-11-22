module OddsScraper.CreateDateTables.GetAnalysisData

open FSharp.Data.Sql
open OddsScraper.CreateDataTables.Printing
open System.IO
open System.Data.SQLite
open OddsScraper.FSharp.Common
open OptionExtension
open BettingCalculations

let createConnectionString path =
    let file = new FileInfo(path)
    let builder = new SQLiteConnectionStringBuilder()
    builder.DataSource <- file.FullName
    builder.ConnectionString

[<Literal>]
let resolutionPath = 
    __SOURCE_DIRECTORY__ + @"\..\packages\System.Data.SQLite.Core.1.0.112.0\lib\net46\"

[<Literal>]
let private connectionString = 
    "Data Source=" + 
    __SOURCE_DIRECTORY__ + "\..\ArchiveData.db;" + 
    "Version=3"

type private sqlProvider = 
    SqlDataProvider<
        Common.DatabaseProviderTypes.SQLITE,
        connectionString,
        //ResolutionPath = resolutionPath, 
        CaseSensitivityChange = Common.CaseSensitivityChange.ORIGINAL
    >

let connect = createConnectionString >> sqlProvider.GetDataContext

let run consoleWriter fileWriter =
    let ctx = connect @"..\ArchiveData_fullData.db"

    let bookieNameQuery = query { for b in ["bet365"; "bwin"; "Pinnacle"; "William Hill"] do yield b }
    
    let bookkeeperQuery = query {
        for item in ctx.Main.Bookkeepers do
        select (item.Id, item.Name)
    }
    
    let oddsQuery = query {
        for odd in ctx.Main.GameOdds do
        where (odd.IsValid)
        select (odd.FkGameOddsGamesId, odd.FkGameOddsBookkeepersId, odd)
    }
    
    let getOddsForGame gameId = query {
        for (gId, bId, odds) in oddsQuery do
        join (bId, name) in bookkeeperQuery on (bId = bId)
        where (gameId = gId)
        where (bookieNameQuery |> Seq.contains name)
        select (odds, name)
    }

    query {
        for item in ctx.Main.Leagues do
        where (item.FkLeaguesSportsId = int64 1)
        select item
    }
    |> Seq.iter (fun league -> 
            let sport = query {
                for sport in ctx.Main.Sports do
                where (sport.Id = league.FkLeaguesSportsId)
                select sport
                exactlyOne
            }
            let country = query {
                for country in ctx.Main.Countries do
                where (country.Id = league.FkLeaguesCountriesId)
                select country
                exactlyOne
            }
        
            consoleWriter(sprintf "%s,%s,%s" sport.Name country.Name league.Name) |> Async.Start
            consoleWriter(System.DateTime.Now.ToString()) |> Async.Start
            
            let gameQuery = query {
                for game in ctx.Main.Games do
                where (game.FkGamesLeaguesId = league.Id)
                where (game.Date.Year > 2007)
                        
                select (game.Id, game.HomeTeamScore, game.AwayTeamScore)
            }

            let wl = 
                gameQuery
                |> Seq.map (fun (gameId, homeScore, awayScore) -> 
                    option {
                        let odds = getOddsForGame gameId |> Seq.toArray
                        let! myBookie = odds |> Array.tryFind (snd >> (=) "bet365")
                        let myBookie = fst myBookie
                        
                        // convert to odd, then normalize
                        let normalizedPct = odds |> Array.map (fun (o, _) -> normalizePct (invert o.HomeOdd) (invert o.DrawOdd) (invert o.AwayOdd))
                        // mean
                        let calcPsychOdd f = 
                            match normalizedPct |> meanFromFunc f with
                            | None -> 0.
                            | Some v -> v |> psychFunc
                            
                        let h, d, a = 
                            // psych
                            let h = calcPsychOdd (fun (h, _, _) -> h)
                            let d = calcPsychOdd (fun (_, d, _) -> d)
                            let a = calcPsychOdd (fun (_, _, a) -> a)

                            // normalize
                            let h, d, a = normalizePct h d a 

                            // convert to odd
                            invert h, invert d, invert a
                        
                        return!
                            if kelly h myBookie.HomeOdd > 0. then
                                Some (homeScore > awayScore, h)
                            else 
                                if kelly d myBookie.DrawOdd > 0. then
                                    let w = homeScore = awayScore
                                    Some (w, d)
                                else
                                    if kelly a myBookie.AwayOdd > 0. then
                                        Some (homeScore < awayScore, a)
                                    else None
                    }
                )
                |> Seq.choose id
                |> Seq.toArray

            consoleWriter("Read all games") |> Async.Start
            consoleWriter(System.DateTime.Now.ToString()) |> Async.Start
        
            let winners = wl |> Array.filter fst |> Array.map snd
            let losers = wl |> Array.filter (fst >> not) |> Array.map snd
        
            let binSize = 0.02
            let binFunc v = floor(v / binSize) * binSize
            let sortToBins = Array.groupBy binFunc >> Array.map (fun (bin, games) -> bin, games |> Seq.length) >> Array.sortBy fst
        
            let wBins = sortToBins winners
            let lBins = sortToBins losers
        
            //let wMu, wSigma = muStdDev winners
            //let lMu, lSigma = muStdDev losers
            match muStdDev winners, muStdDev losers with
            | (Some (wMu, wSigma), Some (lMu, lSigma)) ->              
                let wnMu = binSize * (wBins |> Array.sumBy snd |> float)
                let lnMu = binSize * (lBins |> Array.sumBy snd |> float)
                
                let line = JoinValues [ sport.Name; country.Name; league.Name; wMu; wSigma; wnMu; lMu; lSigma; lnMu ]
                line |> fileWriter |> Async.Start
                ()
            | _ -> ()
    )



