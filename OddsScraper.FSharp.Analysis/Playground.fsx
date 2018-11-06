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


let complexMargin lower upper l m u k odd = 
    if (odd < lower) then simpleMargin l k odd
    else if (odd < upper) then simpleMargin m k odd
    else simpleMargin u k odd

let complexMargin' = complexMargin 3.<euOdd> 10.<euOdd> 0.<pct> 0.04<pct> 0.05<pct>

let finalMedian games noSamples margin = 
    games
    |> simpleMonteCarlo (dailySim margin) 10 noSamples
    |> Seq.map ((Seq.sortBy fst) >> Seq.last >> snd)
    |> median

type marginSetup = {
        LowerOdd: float; UpperOdd: float; 
        LowerMargin: float; MiddleMargin: float; UpperMargin: float;
}
let optimize noSamples games = seq {
    for lower in [1. .. 1. .. 11.] do
        for upper in [lower + 1. .. 1. .. 11.] do
             for l in [0. .. 0.02 .. 0.06] do
                for m in [0.0 .. 0.02 .. 0.1] do
                    for u in [0. .. 0.02 .. 0.06] do
                        let margin = complexMargin (toEuOdd lower) (toEuOdd upper) (toPct l) (toPct m) (toPct u)
                        let fm = finalMedian games noSamples margin
                        yield fm, { LowerOdd = lower; UpperOdd = upper;
                                LowerMargin = l; MiddleMargin = m; UpperMargin = u}
        }

let optimizeSeason season g =
        g
        |> List.find(fun (s, _) -> s = season)
        |> snd
        |> optimize (6*30*5)
        |> Seq.maxBy fst
        |> snd

groupedBySeason |> optimizeSeason 2017

groupedBySeason |> List.iter (plotBySeason complexMargin' (6*30*5))
groupedBySeason |> plotSeason bestMargin (6*30*5) 2017
groupedBySeason |> plotSeason complexMargin' (6*30*5) 2017
groupedBySeason |> plotSeason (complexMargin 1.5<euOdd> 2.<euOdd> 0.02<pct> 0.03<pct> 0.06<pct>) (6*30*5) 2017

simSingle complexMargin' 200 serGames

System.Threading.Tasks.Parallel.ForEach(groupedBySeason, 
        plotBySeason complexMargin' (6*30*5))