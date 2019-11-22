module OddsScraper.FSharp.CommonScraping.WidgetScraping

open FSharp.Data
open OddsScraper.FSharp.CommonScraping
open HtmlNodeExtensions
open FutureGamesDownload
open Models
open OddsManipulation
open OddsScraper.FSharp.Common.BettingCalculations
open OddsScraper.FSharp.Common.OptionExtension

type GameProvider = HtmlProvider<"https://widgets.oddsportal.com/db03999b5cfdac1/s/">

let widgetHtmlAsync link = async {
        let! page = GameProvider.AsyncLoad(link)
        return page.Html
    }

let widgetTableAsync link = async {
        let! html = widgetHtmlAsync link
        return getFirstElement "table" (html.Body())
    }

let widgetGamesAsync link = async {
        let! table = widgetTableAsync link
        return getGameInfosFromTable table
    }

let widgetBet365() = widgetGamesAsync "https://widgets.oddsportal.com/e2eb0fe27b471bb/s/"
let widgetBWin() = widgetGamesAsync "https://widgets.oddsportal.com/5f5c1365462bfec/s/"
let widgetPinnacle() = widgetGamesAsync "https://widgets.oddsportal.com/098c02809003dcd/s/"
let widgetWilliamHill() = widgetGamesAsync "https://widgets.oddsportal.com/db03999b5cfdac1/s/"

let calculateMeans odds = option {
        let! h = odds |> meanFromFunc (fun g-> g.Home)
        let! d = odds |> meanFromFunc (fun g-> g.Draw)
        let! a = odds |> meanFromFunc (fun g-> g.Away)

        return { Home = h; Draw = d; Away = a }
    }

let groupGamesWithMean games =
    games
    |> Seq.groupBy (fun g -> g.GameLink)
    |> Seq.map (fun (_, gs) -> 
        option {
            let gs = gs |> Seq.toArray
            let! means = 
                gs 
                |> Array.map (oddsFromGame >> normalizeGameOdds) 
                |> calculateMeans
        
            return { (gs |> Seq.head) with 
                        MeanOdds = means
                        NoMean = gs.Length}
         })
    |> Seq.choose id

let widgetMeanGamesAsync() = async {
    let! meanGames = [widgetBet365(); widgetBWin(); widgetPinnacle(); widgetWilliamHill()] |> Async.Parallel

    let bet365Games = meanGames |> Array.head |> Seq.toArray
    let meanGames = meanGames |> Seq.collect id |> groupGamesWithMean

    return 
        meanGames 
        |> Seq.map (fun meanGame -> 
            let bg = bet365Games |> Array.tryFind(fun g -> g.GameLink = meanGame.GameLink)
            match bg with
            | None -> { 
                meanGame with 
                    Odds = {
                        Home = 0.; Draw = 0.; Away = 0. }}    // can't find bet365 odds for this game
            | Some g -> { g with 
                            MeanOdds = meanGame.MeanOdds
                            NoMean = meanGame.NoMean
                        }
        )
}
