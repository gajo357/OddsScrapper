module OddsManipulationTests

open FsCheck.Xunit
open FsCheck

open OddsScraper.FSharp.CommonScraping.Models
open OddsScraper.FSharp.CommonScraping.OddsManipulation

type OddsGen = 
    static member Odd() = Gen.elements [1.01 .. 0.01 .. 30.] |> Arb.fromGen

type OddsProperty() = 
    inherit PropertyAttribute(
        Arbitrary = [| typeof<OddsGen> |],
        QuietOnSuccess = true)

[<OddsProperty>]
let ``normalizeGameOdds keeps size order`` (odd: Odds) =
    (odd.Home < odd.Draw && odd.Draw < odd.Away) ==> lazy
        let normalized = normalizeGameOdds odd
        
        normalized.Home < normalized.Draw && normalized.Draw < normalized.Away

[<OddsProperty>]
let ``normalizeGameOdds probability is 1`` (odd: Odds) =
    (odd.Home < odd.Draw && odd.Draw < odd.Away) ==> lazy
        let normalized = normalizeGameOdds odd
        let actual = 1./normalized.Home + 1./normalized.Draw + 1./normalized.Away

        Xunit.Assert.Equal(1., actual, 8)

[<Property>]
let ``moneyToBet is always zero on negative kelly`` (myOdd: float, bOdd: float, amount: float, k : float) =
    (k < 0.) ==> lazy
        let kelly _ _ = k
        let margin _ _ = failwith "should never be invoked"
        let actual = moneyToBet kelly margin myOdd bOdd amount

        0. = actual