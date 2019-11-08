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
     turGames
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

let gamesHist year title games =
    let e =
        games
        |> Seq.filter (fun g -> g.Game.Date.Year >= year)
        |> Seq.map (betWithBet365 betAmount)
        |> Seq.choose id
        |> Seq.toList


    let winners = e |> List.filter (fun g -> float g.MoneyWon > 0.)
    let losers = e |> List.filter (fun g -> float g.MoneyWon < 0.)

    Chart.Combine([ Chart.Histogram(winners |> List.map (fun g -> g.MyOdd))
                    Chart.Histogram(losers |> List.map (fun g -> g.MyOdd)) ])
         .WithXAxis(Max = 4., Min = 2.5, MajorGrid = ChartTypes.Grid(Interval = 0.1)).WithTitle(title)

let leagueHist year league = gamesHist year (fst league) (snd league)

leagueHist 2010 azGames
leagues312
|> List.collect snd
|> (gamesHist 2010 "Title")


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


finalMedian (polGames |> snd) 15 bestMargin

let betGroup amount games =
    let amountLeft, winnings =
        games
        |> Seq.fold (fun (am, win) g ->
            let res = betWithBet365 am g
            match res with
            | None -> am, win
            | Some bet ->
                if bet.MyOdd < 3.1<euOdd> || bet.MyOdd > 3.2<euOdd> then am, win
                else am - bet.MoneyPlaced, win |> Array.append [| bet.MoneyWon |]) (amount, [||])

    let amountLeft = amountLeft + (winnings |> Array.sum)
    winnings.Length, amountLeft

let groupAndBet amount g =
    g
    |> Seq.groupBy (fun s -> s.Game.Date)
    |> Seq.map (fun (_, games) -> betGroup amount games)
    |> Seq.filter (fun (n, _) -> n > 0)

let groupAndBet' amount g =
    g
    |> Seq.groupBy (fun s -> s.Game.Date)
    |> Seq.map snd
    |> Seq.fold (fun total games ->
        let (_, amountLeft) = betGroup total games
        amountLeft) amount


let r =
    polGames
    |> snd
    |> groupAndBet betAmount
    |> Seq.map (fun (n, w) -> n, w / betAmount)
    |> Seq.toList

r
|> List.filter (fun (n, _) -> n > 1)
|> List.sortBy snd

(r |> List.filter (fun (n, w) -> n > 2 && w > 1.5)).Length

let betBestInGroup amount g =
    g
    |> Seq.groupBy (fun s -> s.Game.Date)
    |> Seq.sortBy fst
    |> Seq.map (fun (_, games) ->
        games
        |> Seq.map (betWithBet365 amount)
        |> Seq.choose id
        |> Seq.sortByDescending (fun b -> b.Kelly)
        |> Seq.tryHead)
    |> Seq.choose id
    |> Seq.fold (fun am bet ->
        if bet.MyOdd <= 3.1<euOdd> || bet.MyOdd > 3.2<euOdd> then am
        else am + bet.MoneyWon) amount

leagues312
|> Seq.collect snd
|> Seq.filter (fun g -> g.Game.Date.Year > 2017)
|> betBestInGroup betAmount
|> (*) (1. / betAmount)


leagues312
|> Seq.collect snd
|> Seq.filter (fun g -> g.Game.Date.Year > 2017)
|> groupAndBet' betAmount
|> (*) (1. / betAmount)
