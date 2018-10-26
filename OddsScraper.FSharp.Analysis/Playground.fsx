System.Environment.CurrentDirectory <- @"D:\Projects\OddsScrapper\OddsScraper.Analysis\"
System.Environment.CurrentDirectory <- @"C:\Users\gm.DK\Documents\GitHub\OddsScrapper\OddsScraper.Analysis\"

#load "Playground.Data.fsx"
#load "Playground.MonteCarlo.fsx"
#r "../packages/FSharp.Charting.2.1.0/lib/net45/FSharp.Charting.dll"
#load "../packages/FSharp.Charting.2.1.0/FSharp.Charting.fsx"
open FSharp.Charting
open Playground.Data
open Playground.Engine
open Playground.MonteCarlo
open System

let betAmount = 500.<dkk>
let bestMargin = simpleMargin 0.02<pct>
let margins = 
        [0.02; 0.25; 0.03]
        |> List.map (toPct >> simpleMargin)
let bookies = ["bwin"; "bet365"; "Betfair"; "888sport"; "Unibet"]

[2005..2018]
|> Seq.map (fun s ->
    (s, 
        goodLeagues
        |> Seq.filter (getSeason s)
        |> Seq.sortBy (fun s -> s.Game.Date) 
        |> Seq.toList 
        |> betAll bestMargin betAmount 
        |> (fun b -> b/betAmount)))
|> Seq.toArray

let betByBookie margin g amount =
    bookies
    |> Seq.map (fun b -> (b, betBySeason margin g b amount))
    
let printAnalysis margin g = 
    betByBookie margin g betAmount
    |> Seq.iter (fun (bookie, seasons) -> 
        printfn "%A" bookie
        seasons 
        |> Seq.iter (fun (season, result) -> printfn "%A - %f" season result)
        )
let printMonthAnalysis margin g = 
    betByMonthMargin "bet365" g betAmount margin 
    |> snd
    |> Seq.iter (fun (month, seasons) -> printfn "%A - %f" month seasons)

let plotSeasonAnalysis margins (league, games) = 
    Chart.Combine(
        margins
        |> Seq.map ((betBySeasonMargin "bet365" games betAmount) >> (fun (m, seasons) -> Chart.Line(data = seasons, Name = m.ToString())))
        |> Seq.toList
    ).WithLegend(Alignment = Drawing.StringAlignment.Near, Docking = ChartTypes.Docking.Left).WithYAxis(Max = 10., MajorGrid = ChartTypes.Grid(Interval = 1.)).WithTitle(Text = league)
let plotMonthAnalysis margins (league, games) = 
    Chart.Combine(
        margins
        |> Seq.map ((betByMonthMargin "bet365" games betAmount) >> (fun (m, seasons) -> Chart.Line(data = seasons, Name = m.ToString())))
        |> Seq.toList)
        .WithLegend(Alignment = Drawing.StringAlignment.Near, Docking = ChartTypes.Docking.Left)
        .WithYAxis(Max = 3., MajorGrid = ChartTypes.Grid(Interval = 1.)).WithTitle(Text = league)
let plotMonthlyAgainst margin gs =
    Chart.Combine(
        gs 
        |> Seq.map (fun g -> betByMonthMargin "bet365" g betAmount margin) 
        |> Seq.map (fun (_, g) -> Chart.Line(g))
        |> Seq.toList
    ).WithLegend(Alignment = Drawing.StringAlignment.Near, Docking = ChartTypes.Docking.Left)

[romGames; czGames; welsGames]
|> Seq.map (plotMonthAnalysis margins) |> Seq.iter (fun p -> p.ShowChart() |> ignore)

[eng2Games]
|> Seq.map (plotSeasonAnalysis margins)
|> Seq.iter (fun p -> p.ShowChart() |> ignore)

printAnalysis bestMargin (engGames |> snd)
printMonthAnalysis bestMargin (goodLeagues)

plotMonthlyAgainst bestMargin [goodLeagues; goodLeagues |> (List.append (eng2Games|> snd))]

let execGroup groups = 
    groups
    |> Seq.map(fun (s, g) -> s, g |> List.ofSeq)
    |> List.ofSeq
    
let groupBySeason = groupByYear >> execGroup

let groupedBySeason = goodLeagues |> groupBySeason

let dailySim margin g = 
        daily margin "bet365" betAmount g |> Seq.ofList
        |> Seq.map (fun (r, m) -> (r, float m))
let plotBySeason margin noSamples (season, games) =
    Chart.Combine(
            games
            |> simpleMonteCarlo (dailySim margin) 10 noSamples
            |> Seq.map (fun g -> Chart.Line(g))
            |> Seq.toList)
        .WithLegend(Alignment = Drawing.StringAlignment.Near, Docking = ChartTypes.Docking.Left)
        .WithTitle(Text = sprintf "%A" season)
        .ShowChart() |> ignore

let plotSeason margin noSamples season =
    List.find(fun (s, _) -> s = season)
    >> plotBySeason margin noSamples

let simSingle margin noSamples games =
    games
    |> snd
    |> groupBySeason
    |> List.iter (plotBySeason margin noSamples)

let complexMargin k odd = 
    if (odd < 3.<euOdd>) then simpleMargin 0.0<pct> k odd
    else if (odd < 10.<euOdd>) then simpleMargin 0.04<pct> k odd
    else simpleMargin 0.05<pct> k odd

simSingle complexMargin 200 serGames

groupedBySeason |> List.iter (plotBySeason complexMargin (6*30*5))
groupedBySeason |> plotSeason bestMargin (6*30*5) 2017
groupedBySeason |> plotSeason complexMargin (6*30*5) 2017

System.Threading.Tasks.Parallel.ForEach(groupedBySeason, 
        plotBySeason complexMargin (6*30*5))