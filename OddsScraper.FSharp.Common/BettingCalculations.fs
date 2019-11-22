namespace OddsScraper.FSharp.Common

module BettingCalculations =
    open OptionExtension
    
    let round (n: float) = System.Math.Round(n, 2)

    let mean (values: float seq) = 
        if values |> Seq.isEmpty then None
        else values |> Seq.average |> Some 
    let meanFromFunc propFunc = (Seq.map propFunc) >> mean

    let muStdDev data = option {
        let! mu = mean data
        let variance = data |> Array.averageBy (fun x -> (x - mu)**2.)
        return mu, sqrt(variance)
    }
        

    let kelly myOdd bookerOdd = 
        if (myOdd = 0.) then 0.
        else if (bookerOdd = 1.) then 0.
        else (bookerOdd/myOdd - 1.) / (bookerOdd - 1.)

    let complexMargin k odd = 
        if (odd < 3.) then false
        else if (odd < 10.) then k <= 0.04
        else k <= 0.05

    let moneyToBet kelly amount =
        let m = kelly * amount
        if m < 2.0 then 2.0
        else m
    
    let invert v = 
        if v = 0. then 0.
        else 1. / v

    let normalizePct h d a =
        let whole = h + d + a
        (h/whole, d/whole, a/whole)
    let normalizeOdds h d a =
        let (h, d, a) = normalizePct (invert h) (invert d) (invert a)
        ((invert h), (invert d), (invert a))
    
    let psychFunc v = 1.3794 * v * v * v * v - 0.1194 * v * v * v - 1.959 * v * v + 1.6147 * v + 0.0344
    
    let getAmountToBet amount myOdd bookerOdd =
        let myOdd = myOdd |> invert |> psychFunc |> invert
        if myOdd < 3.1 || myOdd > 3.25 then 0.
        else
            let k = kelly myOdd bookerOdd
            if k > 0. then moneyToBet k amount
            else 0.
    

