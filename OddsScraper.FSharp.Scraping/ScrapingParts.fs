namespace OddsScraper.FSharp.Scraping

type Odd = 
    {
        Name: string;
        Odds: double list;
        Deactivated: bool
    }

module ScrapingParts = 
    open OddsScraper.FSharp.Scraping.Common
    open OddsScraper.FSharp.Scraping.NodeExtensions
    open System
    open OpenQA.Selenium

    let GetLinkParts (gameLink:string) =
        gameLink.Split([|'/'|], System.StringSplitOptions.RemoveEmptyEntries)

    let GetLeaguesLinks sportLinks mainTable = 
        mainTable
        |> GetAllHrefFromElements
        |> Seq.filter (fun a -> sportLinks |> Seq.exists (fun sl -> a.StartsWith(sl) && ((GetLinkParts(a.Replace(sl, ""))).Length >= 3)))
        //|> Seq.take 5
        |> Seq.toArray

    let GetSeasonsLinks allDivElements = 
        let filtered =
            allDivElements
            |> Seq.filter (ClassAttributeEquals "main-menu2 main-menu-gray")
            |> List.ofSeq

        match filtered with
        | [] -> [||]
        | head::_ -> 
            head
            |> GetAllHrefTextAndAttribute
            |> Seq.toArray

    let GetResultsPagesLinks link paginationElement = 
        (match paginationElement with
        | None -> [link]
        | Some el -> 
            let maxPage =
                GetAllHrefFromElements el
                |> Seq.map (fun n -> n.Replace(link, ""))
                |> Seq.map IntegerInString
                |> Seq.max
            match maxPage with
            | None -> [link]
            | Some p ->
                [1..p] |> List.map (fun n -> System.String.Format("{0}page/{1}/", link, n))
            )

    let GetGameLinksFromTable gamesTable = 
        gamesTable
        |> GetTableRows
        |> Seq.filter (ClassAttributeContains "deactivate")
        |> Seq.map (fun n -> GetAllHrefFromElements(n) |> Seq.head)
        |> Seq.distinct
        |> Seq.toArray

    let ConvertStringToOdd input =
        match TryParseDouble input with
        | Some value -> value
        | None -> 1.0

    let GetOddsFromRow node =
        let tds = GetTdsFromRow node |> Seq.toArray
        match tds with
        | [||] -> None
        | _ ->
            let name = (tds |> Seq.head |> (fun n -> n.Text)).Trim().Replace("\n", System.String.Empty)
            let oddTds = 
                tds 
                |> Seq.filter (ClassAttributeContains "right odds")
                |> Seq.toArray

            match oddTds with
            | [||] -> None
            | _ ->
                let odds = 
                    oddTds
                    |> Seq.map (fun n -> n.Text)
                    |> Seq.map ConvertStringToOdd
                    |> Seq.toList

                let deactivated = 
                    oddTds
                    |> Seq.exists (ClassAttributeContains "dark")

                Some { Name = name; Odds = odds; Deactivated = deactivated}

    let GetOddsFromGamePage tableNode = 
        tableNode
        |> (GetElements "tbody") |> Seq.head
        |> GetTableRows
        |> Seq.filter (ClassAttributeContains "lo")
        |> Seq.map GetOddsFromRow
        |> Seq.choose id
        |> Seq.toArray

    let ReadParticipantsNames participantElement = 
        participantElement
        |> (GetElements "h1") |> Seq.head
        |> fun n -> n.Text.Split([|" - "|], StringSplitOptions.RemoveEmptyEntries)
        |> fun parts -> (parts.[0], parts.[1])

    let ReadGameDate dateElement = 
        dateElement
        |> (GetElements "p") |> Seq.head
        |> fun n -> System.String.Join(", ", n.Text.Split(',').[1..2])
        |> fun n -> System.DateTime.Parse(n)
        
    let ReadGameScore (resultElement:IWebElement) = 
        let defaultScore = (-1, -1, false)
        match resultElement.Text with
        | n when n.ToUpper().Contains("CANCELED") -> defaultScore
        | n when n.Trim().StartsWith("Final result") ->
            let isOvertime = n.ToUpper().Contains("OT") || n.ToUpper().Contains("OVERTIME")
            let parts = 
                n.Split(':').[0..1] 
                |> Array.map IntegerInString 
                
            match parts with
            | [|Some home; Some away|] -> (home, away, isOvertime)
            | _ -> defaultScore
        | _ -> defaultScore

    let ExtractSportCountryAndLeagueFromLink gameLink =
        let parts = GetLinkParts gameLink
        (parts.[0], parts.[1], parts.[2])


