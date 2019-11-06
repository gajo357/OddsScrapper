System.Environment.CurrentDirectory <- @"C:\Users\gajo3\source\repos\OddsScrapper\OddsScraper.Analysis\"

#load "Playground.Data.fsx"
#load "Playground.MonteCarlo.fsx"

#r "../packages/FSharp.Charting.2.1.0/lib/net45/FSharp.Charting.dll"
#load "../packages/FSharp.Charting.2.1.0/FSharp.Charting.fsx"

open FSharp.Charting

open Playground.Data
open Playground.Engine
open Playground.MonteCarlo

let betAmount = 1000.<dkk>

let bestMargin = simpleMargin 1.0<pct>

[ 2005 .. 2018 ]
|> Seq.map (fun s ->
    (s,
     indGames
     |> snd
     |> Seq.filter (isFromSeason s)
     |> Seq.sortBy (fun s -> s.Game.Date)
     |> Seq.toList
     |> betAll bestMargin betAmount
     |> (fun b -> b / betAmount)))
|> Seq.toArray

let betWithBookie bookie amount gg =
    match gg.Odds |> Seq.tryFind (fun f -> bookie = f.Name) with
    | None -> None
    | Some odds -> placeBetOnGame gg.Game gg.Mean odds amount

let betWithBet365 = betWithBookie "bet365"

let e = 
    engGames
    |> snd
    // |> Seq.filter (isFromSeason 2017)
    // |> Seq.sortBy (fun s -> s.Game.Date)
    |> Seq.map (betWithBet365 betAmount)
    |> Seq.choose id
    |> Seq.toList

|> List.filter (fun g -> float g.MoneyWon > 0.) 

let winners = e |> List.filter (fun g -> float g.MoneyWon > 0.)
let losers = e |> List.filter (fun g -> float g.MoneyWon < 0.)


Chart.Histogram(losers |> List.map (fun g -> g.Kelly)).WithXAxis(Max = 1., Min = 0., MajorGrid = ChartTypes.Grid(Interval = 0.1))
Chart.Point(winners |> List.map (fun g -> g.Kelly, g.BookerOdd)).WithYAxis(Max = 20., Min = 0., MajorGrid = ChartTypes.Grid(Interval = 5.))

let bet margin g =
    g
    |> Seq.sortBy (fun s -> s.Game.Date)
    |> Seq.toList
    |> betAll margin betAmount
    |> (fun b -> b / betAmount)

let finalMedian games noSamples margin =
    games
    |> simpleMonteCarlo (bet margin) 10 noSamples
    |> median


finalMedian (engGames |> snd) 5 bestMargin
