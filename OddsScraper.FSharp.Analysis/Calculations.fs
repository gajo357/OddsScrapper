namespace OddsScraper.FSharp.Analysis

module Calculations =
    open OddsScraper.Repository.Models
    open OddsScraper.FSharp.Common.BettingCalculations
    open OddsScraper.FSharp.Common.OptionExtension

    let makeBet win myOdd bookerOdd (amount, alreadyRun) =
        let k = kelly myOdd bookerOdd
        if (not alreadyRun && k * amount > 2. && k < 0.1) then
            if (win) then
                (amount + (bookerOdd - 1.)*k*amount, true)
            else 
                (amount*(1. - k), true)
        else
            (amount, alreadyRun)

    let betGame (g: Game) (meanOdds: GameOdd) (gameOdds: GameOdd) (amount: float) =
        (amount, false)
        |> makeBet (g.HomeScore > g.AwayScore) meanOdds.HomeOdd gameOdds.HomeOdd
        |> makeBet (g.HomeScore = g.AwayScore) meanOdds.DrawOdd gameOdds.DrawOdd
        |> makeBet (g.HomeScore < g.AwayScore) meanOdds.AwayOdd gameOdds.AwayOdd
        |> fst

    let betGames g meanOdds odds amount =
        match odds with
        | Some o -> betGame g meanOdds o amount
        | _ -> amount
    
    let meanOdds odds = option {
        let! h = odds |> meanFromFunc (fun o -> o.HomeOdd)
        let! d = odds |> meanFromFunc (fun o -> o.DrawOdd)
        let! a = odds |> meanFromFunc (fun o -> o.AwayOdd)

        return {
            HomeOdd = h
            DrawOdd = d
            AwayOdd = a
            Game = int64(0); Bookkeeper = {Id = int64(0); Name = ""}; IsValid = true
        }
    }
        
    let betGroupedGame amount bookie (g:Game, odds: GameOdd list) = option {     
            let! mu = meanOdds odds
            return betGames g mu (odds |> List.tryFind (fun o -> o.Bookkeeper.Name = bookie)) amount
        }

    let rec betAll amount bookie games =
        match games with
        | head :: tail -> 
            match (betGroupedGame amount bookie head) with
            | None -> amount
            | Some v -> betAll v bookie tail
        | [] -> amount
