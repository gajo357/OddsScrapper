System.Environment.CurrentDirectory <- @"D:\Projects\OddsScrapper\OddsScraper.Analysis\"
System.Environment.CurrentDirectory <- @"C:\Users\gm.DK\Documents\GitHub\OddsScrapper\OddsScraper.Analysis\"

#load "Playground.Data.fsx"
#r "../packages/FSharp.Charting.2.1.0/lib/net45/FSharp.Charting.dll"
#load "../packages/FSharp.Charting.2.1.0/FSharp.Charting.fsx"
open FSharp.Charting
open Playground.Data
open Playground.Engine
open System

let betAmount = 500.
let bookies = ["bwin"; "bet365"; "Betfair"; "888sport"; "Unibet"]

[2005..2018]
|> Seq.map (fun s ->
    (s, 
        goodLeagues
        |> Seq.filter (getSeason s)
        |> Seq.sortBy (fun s -> s.Game.Date) 
        |> Seq.toList 
        |> betAll 0.02 betAmount 
        |> (fun b -> b/betAmount)))
|> Seq.toArray

let betByBookie margin g amount =
    bookies
    |> Seq.map (fun b -> (b, betBySeason margin g b amount))
    
let printAnalysis margin g = 
    betByBookie margin g 
    |> Seq.iter (fun (bookie, seasons) -> 
        printfn "%A" bookie
        seasons 
        |> Seq.iter (fun (season, result) -> printfn "%A - %f" season result)
        )
let printMonthAnalysis margin g = 
    betByMonthMargin "bet365" g margin
    |> snd
    |> Seq.iter (fun (month, seasons) -> printfn "%A - %f" month seasons)

let plotSeasonAnalysis margins (league, games) = 
    Chart.Combine(
        margins
        |> Seq.map ((betBySeasonMargin "bet365" games) >> (fun (m, seasons) -> Chart.Line(data = seasons, Name = m.ToString())))
        |> Seq.toList
    ).WithLegend(Alignment = Drawing.StringAlignment.Near, Docking = ChartTypes.Docking.Left).WithYAxis(Max = 10., MajorGrid = ChartTypes.Grid(Interval = 1.)).WithTitle(Text = league)
let plotMonthAnalysis margins (league, games) = 
    Chart.Combine(
        margins
        |> Seq.map ((betByMonthMargin "bet365" games) >> (fun (m, seasons) -> Chart.Line(data = seasons, Name = m.ToString())))
        |> Seq.toList
    ).WithLegend(Alignment = Drawing.StringAlignment.Near, Docking = ChartTypes.Docking.Left).WithYAxis(Max = 3., MajorGrid = ChartTypes.Grid(Interval = 1.)).WithTitle(Text = league)
let plotMonthlyAgainst margin gs =
    Chart.Combine(
        gs 
        |> Seq.map (fun g -> betByMonthMargin "bet365" g margin) 
        |> Seq.map snd
        |> Seq.map (fun g -> Chart.Line(g))
        |> Seq.toList
    ).WithLegend(Alignment = Drawing.StringAlignment.Near, Docking = ChartTypes.Docking.Left)

[engGames; gerGames; greGames;
    polGames; porGames; serGames; espGames; turGames; usaGames;
    clGames; elGames; rusGames; rusCupGames; scoGames; sokGames;
    chiGames]
[azGames; belGames; irGames; indGames; isrGames; kazGames]
[eng2Games; fraGames]
[romGames; czGames; welsGames; swiGames; sloGames]
|> Seq.map (plotMonthAnalysis [0.02;0.025;0.03]) |> Seq.iter (fun p -> p.ShowChart() |> ignore)

[azGames; belGames; irGames; indGames; isrGames; kazGames]
[eng2Games; fraGames]
|> Seq.map (plotSeasonAnalysis [0.02;0.025;0.03])
|> Seq.iter (fun p -> p.ShowChart() |> ignore)

printAnalysis 0.02 (engGames |> snd)
printMonthAnalysis 0.02 (goodLeagues)

plotMonthlyAgainst 0.02 [goodLeagues; goodLeagues |> (Seq.append (eng2Games|> snd))]
