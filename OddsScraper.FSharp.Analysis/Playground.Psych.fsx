System.Environment.CurrentDirectory <- @"C:\Users\gajo3\source\repos\OddsScrapper\OddsScraper.Analysis\"

#load "Playground.Data.fsx"
#load "Playground.MonteCarlo.fsx"

#r "../packages/FSharp.Charting.2.1.0/lib/net45/FSharp.Charting.dll"
#load "../packages/FSharp.Charting.2.1.0/FSharp.Charting.fsx"

open FSharp.Charting
open FSharp.Data

open Playground.Data
open Playground.Engine
open Playground.MonteCarlo


type OutputCsv = CsvProvider<"output.csv">

type OutputRow = OutputCsv.Row

let outputCsv = OutputCsv.Load("output.csv")

let betAmount = 100.<dkk>

let isInRange (league: OutputRow) myOdd =
    let wg = (float league.WinCountMu) * gaus (float league.WinMu) (float league.WinSigma) myOdd
    let lg = (float league.LoseCountMu) * gaus (float league.LoseMu) (float league.LoseSigma) myOdd

    (wg > lg * 2.) && (float (league.WinMu - league.WinSigma) <= myOdd)
    && (float (league.WinMu + league.WinSigma) >= myOdd)

let betWithBookie bookie amount gg =
    match gg.Odds |> Seq.tryFind (fun f -> bookie = f.Name) with
    | None -> None
    | Some odds -> placeBetOnGame gg.Game gg.Mean odds amount

let betWithBet365 = betWithBookie "bet365"

let betGroup league amount games =
    let amountLeft, winnings =
        games
        |> Seq.fold (fun (am, win) g ->
            if am < 2.<dkk> then
                am, win
            else
                let res = betWithBet365 am g
                match res with
                | None -> am, win
                | Some bet ->
                    if isInRange league (float bet.MyOdd) |> not then am, win
                    else am - bet.MoneyPlaced, win |> Array.append [| bet.MoneyWon |]) (amount, [||])

    let amountLeft = amountLeft + (winnings |> Array.sum)
    winnings.Length, amountLeft

let groupAndBet league amount g =
    g
    |> Seq.groupBy (fun s -> s.Game.Date)
    |> Seq.map (fun (_, games) -> betGroup league amount games)
    |> Seq.filter (fun (n, _) -> n > 0)

let groupAndBet' year league amount g =
    g
    |> Seq.filter (fun g -> g.Game.Date.Year = year)
    |> Seq.groupBy (fun s -> s.Game.Date)
    |> Seq.map snd
    |> Seq.fold (fun total games ->
        let (_, amountLeft) = betGroup league total games
        amountLeft) amount

let split (input: string) = input.Split([| '_' |], System.StringSplitOptions.RemoveEmptyEntries)

let leagueGaus (games: string * GroupedGame list) year =
    let [| sport; country; name |] =
        games
        |> fst
        |> split

    let leagueRow =
        outputCsv.Rows |> Seq.find (fun row -> row.Sport = sport && row.Country = country && row.League = name)
    groupAndBet' year leagueRow betAmount (snd games)

[ 2005 .. 2018 ] |> List.map (leagueGaus kazGames)
[ 2005 .. 2018 ]
|> List.collect (fun year -> goodLeagues |> List.map (fun league -> (fst league, leagueGaus league year)))
|> List.filter (snd >> (>) betAmount)

let writeToCsv line = System.IO.File.AppendAllLines("output.csv", [ line ])

writeToCsv "league,wMu,wSigma,wnMu,lMu,lSigma,lnMu"

let gamesGausPlot bookie minYear title games =
    let results =
        games
        |> Seq.filter (fun g -> g.Game.Date.Year >= minYear)
        |> Seq.map (betWithBookie bookie betAmount)
        |> Seq.choose id
        |> Seq.filter (fun g -> g.MyOdd < 3.3<euOdd>)
        |> Seq.toList

    let winners =
        results
        |> List.filter (fun g -> float g.MoneyWon > 0.)
        |> List.map (fun g -> float g.MyOdd)
    let losers =
        results
        |> List.filter (fun g -> float g.MoneyWon < 0.)
        |> List.map (fun g -> float g.MyOdd)

    let binSize = 0.02
    let binFunc v = System.Math.Floor(v / binSize) * binSize

    let sortToBins =
        List.groupBy binFunc
        >> List.map (fun (bin, games) -> bin, games |> Seq.length)
        >> List.sortBy fst

    let wBins = sortToBins winners
    let lBins = sortToBins losers

    let wMu, wSigma = muStdDev winners
    let lMu, lSigma = muStdDev losers
    let wnMu = binSize * (wBins |> List.sumBy (snd >> float))
    let lnMu = binSize * (lBins |> List.sumBy (snd >> float))
    writeToCsv (sprintf "%s,%f,%f,%f,%f,%f,%f" title wMu wSigma wnMu lMu lSigma lnMu)

    Chart.Combine([ Chart.Column(wBins)
                    Chart.Column(lBins)
                    Chart.Line
                        (List.zip (wBins |> List.map fst) (wBins |> List.map (fun (b, _) -> wnMu * gaus wMu wSigma b)))
                    Chart.Line
                        (List.zip (lBins |> List.map fst) (lBins |> List.map (fun (b, _) -> lnMu * gaus lMu lSigma b))) ])
         .WithXAxis(Min = 2.85, Max = 3.4, MajorGrid = ChartTypes.Grid(Interval = 0.1)).WithTitle(title)

let leagueGaus bookie year league = gamesGausPlot bookie year (fst league) (snd league)

leagueGaus "William Hill" 2010 gerGames
leagueGaus "bwin" 2010 engGames
leagueGaus "bet365" 2010 eng2Games
leagueGaus "Pinnacle" 2010 engGames
// System.IO.Directory.GetFiles(@"..\OddsScraper.Analysis\", "*.csv")
// |> Array.map System.IO.FileInfo
// |> Array.filter (fun f -> f.Name.StartsWith("soccer"))
// |> Array.map (fun f -> getGames(f.Name.Replace(".csv", "")))
// |> Array.map (leagueGaus "bet365" 2010)
