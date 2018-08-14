namespace OddsScraper.FSharp.Analysis

module Calculations =
    open OddsScraper.Repository.Models
    
    let mean (values: float seq) = 
        if values |> Seq.isEmpty then
            1.
        else
            values |> Seq.average
    let meanFromFunc propFunc = (Seq.map propFunc) >> mean

    let kelly myOdd bookerOdd = 
        if (myOdd = 0.) then
            0.
        else if (bookerOdd = 1.) then
            0.
        else    
            (bookerOdd/myOdd - 1.) / (bookerOdd - 1.)

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
    
    let meanOdds odds =
        {
            HomeOdd = odds |> meanFromFunc (fun o -> o.HomeOdd)
            DrawOdd = odds |> meanFromFunc (fun o -> o.DrawOdd)
            AwayOdd = odds |> meanFromFunc (fun o -> o.AwayOdd)
            Game = int64(0); Bookkeeper = {Id = int64(0); Name = ""}; IsValid = true
        }
    let betGroupedGame amount bookie (g:Game, odds: GameOdd list) =     
        betGames g (meanOdds odds) (odds |> List.tryFind (fun o -> o.Bookkeeper.Name = bookie)) amount

    let rec betAll amount bookie games =
        match games with
        | head :: tail -> betAll (betGroupedGame amount bookie head) bookie tail
        | [] -> amount
