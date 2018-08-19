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
    

