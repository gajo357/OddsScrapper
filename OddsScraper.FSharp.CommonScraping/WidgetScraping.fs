module OddsScraper.FSharp.CommonScraping.WidgetScraping

open FSharp.Data
open OddsScraper.FSharp.CommonScraping.HtmlNodeExtensions
open OddsScraper.FSharp.CommonScraping.FutureGamesDownload
open OddsScraper.FSharp.Common.BettingCalculations

type GameProvider = HtmlProvider<"https://widgets.oddsportal.com/7812aab9e9b2e3d/s/">

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

let widgetBet365() = widgetGamesAsync "https://widgets.oddsportal.com/483cccf42fd1cbc/s/"
let widgetBWin() = widgetGamesAsync "https://widgets.oddsportal.com/26b59f816f920d1/s/"
let widgetPinnacle() = widgetGamesAsync "https://widgets.oddsportal.com/6f7a86682258d9f/s/"
let widgetWilliamHill() = widgetGamesAsync "https://widgets.oddsportal.com/07b7f648e1ca55d/s/"

let calculateMeans games = 
    (games |> meanFromFunc (fun g-> g.HomeOdd),
        games |> meanFromFunc (fun g-> g.DrawOdd),
        games |> meanFromFunc (fun g-> g.AwayOdd))

let groupGamesWithMean games =
    games
    |> Seq.groupBy (fun g -> g.GameLink)
    |> Seq.map (fun (_, gs) -> 
        let gs = gs |> Seq.toArray
        let (home, draw, away) = calculateMeans gs
        { (gs |> Seq.head) with HomeMeanOdd = home; DrawMeanOdd = draw; AwayMeanOdd = away}
        )

let widgetMeanGamesAsync() = async {
    let! bet365Games = widgetBet365() |> Async.StartChild

    let! meanGames = [widgetBWin(); widgetPinnacle(); widgetWilliamHill()] |> Async.Parallel
    let meanGames = meanGames |> Seq.collect id |> groupGamesWithMean

    let! bet365Games = bet365Games
    let bet365Games = bet365Games |> Seq.toArray

    return 
        meanGames 
        |> Seq.map (fun meanGame -> 
            let bg = bet365Games |> Seq.tryFind(fun g -> g.GameLink = meanGame.GameLink)
            match bg with
            | None -> meanGame
            | Some g -> { meanGame with HomeOdd = g.HomeOdd; DrawOdd = g.DrawOdd; AwayOdd = g.AwayOdd }
        )
}
