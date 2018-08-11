namespace OddsScraper.FSharp.Scraping

type Odd = 
    {
        Name: string;
        Odds: double list;
        Deactivated: bool
    }

module ScrapingParts = 
    open System.Text.RegularExpressions
    open OddsScraper.FSharp.Common.Common
    open OddsScraper.FSharp.Scraping.NodeExtensions

    let BaseWebsite = "http://www.oddsportal.com"
    
    let YearPattern = "-\\d{4}"
    let FindYearInLink input = 
        seq { 
            for m in Regex.Matches(input, YearPattern) -> m.Value 
        }

    let HasYearInLink input =
        Regex.IsMatch(input, YearPattern)

    let RemoveYearsFromLink input =
        FindYearInLink input |> RemoveSeq input

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

    let ExtractSportCountryAndLeagueFromLink gameLink =
        let parts = GetLinkParts (gameLink |> Remove BaseWebsite)
        (parts.[0], parts.[1], parts.[2])


