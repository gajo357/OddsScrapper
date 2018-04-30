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
    open OpenQA.Selenium

    let BaseWebsite = "http://www.oddsportal.com"

    let GetLinkParts (gameLink:string) =
        gameLink |> (Split "/")

    let GetLeaguesLinks sportLinks mainTable = 
        mainTable
        |> GetAllHrefFromElements
        |> Seq.filter (fun a -> sportLinks |> Seq.exists (fun sl -> a.StartsWith(sl) && ((GetLinkParts(a.Replace(sl, ""))).Length >= 3)))
        //|> Seq.take 5
        |> Seq.distinct
        |> Seq.toArray

    let GetSeasonsLinks colContent = 
        let div = 
            GetElements "div" colContent
            |> Seq.filter (ClassAttributeEquals "main-menu2 main-menu-gray")
            |> Seq.tryHead

        match div with
        | None -> [||]
        | Some value -> 
            value
            |> GetAllHrefAttributeAndText
            |> Seq.distinct
            |> Seq.filter (fst >> (Contains BaseWebsite))
            |> Seq.toArray

    let GetResultsPagesLinks link paginationElement = 
        [link]
        |> List.append (
            match paginationElement with
            | None -> []
            | Some el -> 
                let maxPage =
                    GetAllHrefFromElements el
                    |> Seq.filter (Contains link)
                    |> Seq.map (Remove link)
                    |> Seq.map IntegerInString
                    |> Seq.max
                match maxPage with
                | None -> []
                | Some p ->
                    [2..p] 
                    |> List.map (fun n -> System.String.Format("{0}#/page/{1}/", link, n)))
            
    let GetGameLinksFromTable gamesTable = 
        gamesTable
        |> GetElementsByClassName "deactivate"
        |> Seq.map (GetAllHrefFromElements >> Seq.tryHead)
        |> Seq.choose id
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
            let name = tds |> Seq.head |> GetText |> Remove "\n"
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
        match tableNode |> (GetElements "tbody") |> Seq.tryHead with
        | None -> [||]
        | Some head -> 
            head
            |> GetTableRows
            |> Seq.filter (ClassAttributeContains "lo")
            |> Seq.map GetOddsFromRow
            |> Seq.choose id
            |> Seq.toArray

    let ReadParticipantsNames participantElement = 
        participantElement
        |> (GetElements "h1") |> Seq.head
        |> (GetText >> (Split " - "))
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
                (n |> (Split ":")).[0..1] 
                |> Array.map IntegerInString 
                
            match parts with
            | [|Some home; Some away|] -> (home, away, isOvertime)
            | _ -> defaultScore
        | _ -> defaultScore

    let ExtractSportCountryAndLeagueFromLink gameLink =
        let parts = GetLinkParts gameLink
        (parts.[0], parts.[1], parts.[2])


