module OddsScraper.FSharp.CommonScraping.WidgetScraping

open FSharp.Data
open OddsScraper.FSharp.CommonScraping
open HtmlNodeExtensions
open FutureGamesDownload
open Models
open OddsManipulation
open OddsScraper.FSharp.Common.BettingCalculations

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

let calculateMeans odds = 
    { Home = odds |> meanFromFunc (fun g-> g.Home);
      Draw = odds |> meanFromFunc (fun g-> g.Draw);
      Away = odds |> meanFromFunc (fun g-> g.Away) }

let groupGamesWithMean games =
    games
    |> Seq.groupBy (fun g -> g.GameLink)
    |> Seq.map (fun (_, gs) -> 
        let means = 
            gs 
            |> Seq.map (oddsFromGame >> normalizeGameOdds) 
            |> Seq.toArray |> calculateMeans
        { (gs |> Seq.head) with 
            MeanOdds = means })

let widgetMeanGamesAsync() = async {
    let! bet365Games = widgetBet365() |> Async.StartChild

    let! meanGames = [widgetBWin(); widgetPinnacle(); widgetWilliamHill()] |> Async.Parallel
    let meanGames = meanGames |> Seq.collect id |> groupGamesWithMean

    let! bet365Games = bet365Games
    let bet365Games = bet365Games |> Seq.toArray

    return 
        meanGames 
        |> Seq.map (fun meanGame -> 
            let bg = bet365Games |> Array.tryFind(fun g -> g.GameLink = meanGame.GameLink)
            match bg with
            | None -> { 
                meanGame with 
                    Odds = {
                        Home = 0.; Draw = 0.; Away = 0. }}    // can't find bet365 odds for this game
            | Some g -> g
        )
}
