namespace OddsScraper.FSharp.CommonScraping
module Downloader =
    open OddsScraper.FSharp.CommonScraping
    open Models
    open CanopyAgent
    open FutureGamesDownload
    open WidgetScraping
    open System
    open System.Threading.Tasks

    let DownloadFromWidget path =
        async {
            let! games = widgetMeanGamesAsync()
            let games = games |> Seq.toArray
            return
                getLeaguesFromPath path
                |> Seq.collect (fun s -> 
                    games |> Seq.filter (fun g -> isGameLinkFromAnyLeague s g.GameLink))
        } |> Async.StartAsTask

    type IDownloader =
        abstract member DownloadFromWidget: unit -> Task<Game seq>
        abstract member DownloadGameInfos: DateTime -> Task<Game seq>
        abstract member ReadGameFromLink: string -> Task<Game>
        abstract member LogIn: string -> string -> Task<bool>

    type Downloader() =
        let agent = CanopyAgent()
        let navigateAndGetHtml link = async {
            let! gameHtmlString = agent.GetPageHtml link
            return GamePageReading.parseGameHtml gameHtmlString 
        }

        interface IDownloader with 
            member __.DownloadFromWidget() = DownloadFromWidget ""

            member __.DownloadGameInfos date =
                async {
                    let! gamesHtmls = 
                        getLeagues()
                        |> Seq.map (fun s -> async {
                            let! document = navigateAndGetHtml(sportDateUrl date s.Sport)
                            return (s, document)})
                        |> Async.Parallel
                    return 
                        gamesHtmls
                        |> Seq.collect(fun (s, document) -> gamesFromRegularPage s document)
                } |> Async.StartAsTask

            member __.ReadGameFromLink gameLink = 
                async {
                    let! html = navigateAndGetHtml gameLink
                    return 
                        match readGame(gameLink, html) with
                        | Some g -> g
                        | None -> emptyGame
                } |> Async.StartAsTask

            member __.LogIn username password = 
                async {
                    let! resultLink = agent.Login username password
                    printfn "%A" resultLink
                    return "https://www.oddsportal.com/settings/" = resultLink
                } |> Async.StartAsTask