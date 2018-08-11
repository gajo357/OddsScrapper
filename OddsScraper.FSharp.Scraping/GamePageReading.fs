namespace OddsScraper.FSharp.Scraping

module GamePageReading =
    open FSharp.Data
    open HtmlNodeExtensions
    open OddsScraper.FSharp.Common.Common

    type GameProvider = HtmlProvider<"http://www.oddsportal.com/soccer/england/premier-league-2016-2017/arsenal-everton-SGPa5fvr/">

    let ParseGameHtml gameHtml =
        (GameProvider.Parse gameHtml).Html

    let ReadParticipantsNames participantElement = 
        participantElement
        |> (GetFirstElement "h1")
        |> GetText 
        |> (Split " - ")
        |> fun parts -> (parts.[0], parts.[1])

    let ReadGameDate dateElement = 
        dateElement
        |> (GetFirstElement "p")
        |> GetText 
        |> (Split ",")
        |> (fun n -> n.[1..2])
        |> (Join ", ")
        |> TryParseDateTime

    let GetDateOrDefault gameDate =
        match gameDate with
        | Some d -> d
        | None -> System.DateTime.MinValue
        
    let ReadGameScore resultElement = 
        let defaultScore = (int64 -1, int64 -1, false)
        match resultElement |> GetText with
        | n when n.ToUpper().Contains("CANCELED") -> defaultScore
        | n when n.StartsWith("Final result") ->
            let isOvertime = n.ToUpper().Contains("OT") || n.ToUpper().Contains("OVERTIME")
            let parts = 
                (n |> (Split ":")).[0..1] 
                |> Array.map IntegerInString 
                
            match parts with
            | [|Some home; Some away|] -> (int64 home, int64 away, isOvertime)
            | _ -> defaultScore
        | _ -> defaultScore

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
                    |> Seq.map GetText
                    |> Seq.map ConvertStringToOdd
                    |> Seq.toList

                let deactivated = 
                    oddTds
                    |> Seq.exists (ClassAttributeContains "dark")

                Some { Name = name; Odds = odds; Deactivated = deactivated}

    let GetOddsFromGamePage gameHtml = 
        match gameHtml |> (GetElementById "#odds-data-table") |> (GetElements "tbody") |> Seq.tryHead with
        | None -> [||]
        | Some head -> 
            head
            |> GetTableRows
            |> Seq.filter (ClassAttributeContains "lo")
            |> Seq.map GetOddsFromRow
            |> Seq.choose id
            |> Seq.toArray

    
        

