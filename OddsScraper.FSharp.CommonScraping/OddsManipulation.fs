module OddsScraper.FSharp.CommonScraping.OddsManipulation

open OddsScraper.FSharp.Common.BettingCalculations
open OddsScraper.FSharp.CommonScraping.Models

let complexMargin k odd = 
    if (odd < 3.) then false
    else if (odd < 10.) then k <= 0.04
    else k <= 0.05

let normalizeGameOdds odds =
    let (h, d, o) = normalizeOdds odds.Home odds.Draw odds.Away
    { 
        Home = h
        Draw = d
        Away = o
    }

let moneyToBet margin myOdd bookerOdd amount =
    let k = kelly myOdd bookerOdd
    if (k > 0.0 && margin k myOdd) then
        let m = k * amount * 1.
        if m < 2.0 then 2.
        else System.Math.Round(m, 2)
    else 0.0

let moneyWithComplex = moneyToBet complexMargin

let amountsToBet myOdds bookerOdds amount = 
    let myOdds = normalizeGameOdds myOdds
    let bookerOdds = normalizeGameOdds bookerOdds
    {
        Home = moneyWithComplex myOdds.Home bookerOdds.Home amount
        Draw = moneyWithComplex myOdds.Draw bookerOdds.Draw amount
        Away = moneyWithComplex myOdds.Away bookerOdds.Away amount
    }
