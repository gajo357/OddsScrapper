namespace OddsScraper.FSharp.CommonScraping
module Downloader =
    open OddsScraper.FSharp.CommonScraping
    open CanopyAgent
    open FutureGamesDownload
    open System
    open System.Threading.Tasks

    type IDownloader =
        abstract member DownloadGameInfos: DateTime -> float -> Task<Game seq>
        abstract member DownloadAllDayGameInfos: DateTime -> Task<Game seq>
        abstract member ReadGameFromLink: string -> Task<Game>
        abstract member LogIn: string -> string -> Task<bool>

    type Downloader() =
        let agent = CanopyAgent()
        let navigateAndGetHtml link = async {
            let! gameHtmlString = agent.GetPageHtml link
            return GamePageReading.parseGameHtml gameHtmlString 
        }
        interface IDownloader with 
            member __.DownloadGameInfos date timeSpan =
                async {
                    let! gamesHtmls = 
                        getLeagues()
                        |> Seq.map (fun s -> async {
                            let! document = navigateAndGetHtml(sportDateUrl date s.Sport)
                            return (s, document)
                        })
                        |> Async.Parallel
                    return 
                        gamesHtmls
                        |> Seq.map(fun (s, document) -> gamesFromGameTablePage date timeSpan s document)
                        |> Seq.collect(fun games -> games)
                } |> Async.StartAsTask

            member x.DownloadAllDayGameInfos date = (x :> IDownloader).DownloadGameInfos date (24.*60.)

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