namespace OddsScraper.FSharp.CommonScraping

module GamePageReading =
    open FSharp.Data
    open HtmlNodeExtensions
    open OddsScraper.FSharp.Common.Common

    type GameProvider = HtmlProvider<"http://www.oddsportal.com/soccer/england/premier-league-2016-2017/arsenal-everton-SGPa5fvr/">

    let parseGameHtml gameHtml =
        (GameProvider.Parse gameHtml).Html

    let readParticipantsNames participantElement = 
        participantElement
        |> (getFirstElement "h1")
        |> getText 
        |> (Split " - ")
        |> fun parts -> (parts.[0], parts.[1])

    let getParticipantsAndDateElement = getElementById "#col-content" 
    let getGameScoreElement = getElementById "#event-status"

    let readGameDate dateElement = 
        dateElement
        |> (getFirstElement "p")
        |> getText 
        |> (Split ",")
        |> (fun n -> n.[1..2])
        |> (Join ", ")
        |> TryParseDateTime

    let getDateOrDefault gameDate =
        match gameDate with
        | Some d -> d
        | None -> System.DateTime.MinValue
        
    let readGameScore resultElement = 
        let defaultScore = (int64 -1, int64 -1, false)
        match resultElement |> getText with
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

    let convertStringToOdd input =
        match TryParseDouble input with
        | Some value -> value
        | None -> 1.0

    let getOddsFromRow node =
        let tds = getTdsFromRow node |> Seq.toArray
        match tds with
        | [||] -> None
        | _ ->
            let name = tds |> Seq.head |> getText |> Remove "\n"
            let oddTds = 
                tds 
                |> Seq.filter (classAttributeContains "right odds")
                |> Seq.toArray

            match oddTds with
            | [||] -> None
            | _ ->
                let odds = 
                    oddTds
                    |> Seq.map getText
                    |> Seq.map convertStringToOdd
                    |> Seq.toList

                let deactivated = 
                    oddTds
                    |> Seq.exists (classAttributeContains "dark")

                Some { Name = name; Odds = odds; Deactivated = deactivated}

    let getOddsFromGamePage gameHtml = 
        match gameHtml |> (getElementById "#odds-data-table") |> (getElements "tbody") |> Seq.tryHead with
        | None -> [||]
        | Some head -> 
            head
            |> getTableRows
            |> Seq.filter (classAttributeContains "lo")
            |> Seq.map getOddsFromRow
            |> Seq.choose id
            |> Seq.toArray

    let convertOddsListTo1x2 odd = 
        match odd.Odds with
        | [home; away] -> (home, double 0.0, away)
        | [home; draw; away] -> (home, draw, away)
        | _ -> (0.0, 0.0, 0.0)

    
        

