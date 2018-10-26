namespace OddsScraper.FSharp.Common

module BettingCalculations =
    
    let round (n: float) = System.Math.Round(n, 2)

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

    let complexMargin k odd = 
        if (odd < 3.) then false
        else if (odd < 10.) then k <= 0.04
        else k <= 0.05

    let moneyToBet kelly amount =
        let m = kelly * amount
        if m < 2.0 then 2.0
        else m
    
    let invert = (/) 1.

    let normalizePct h d a =
        let whole = h + d + a
        (h/whole, d/whole, a/whole)
    let normalizeOdds h d a =
        let (h, d, a) = normalizePct (invert h) (invert d) (invert a)
        ((invert h), (invert d), (invert a))
    
    let getAmountToBet maxPercent amount myOdd bookerOdd =
        let k = kelly myOdd bookerOdd 
        if (k > 0. && k <= maxPercent) then
            moneyToBet k amount
        else 0.
    

