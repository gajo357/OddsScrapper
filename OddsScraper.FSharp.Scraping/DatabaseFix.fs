namespace OddsScraper.FSharp.Scraping

module DatabaseFix =
    open OddsScraper.FSharp.Scraping
    open OddsScraper.Repository.Repository

    open Common
    open ScrapingParts
    
    let fixDatabase() =
        let repository = new Project(@"../ArchiveData.db")
        let leagues = 
            System.IO.File.ReadLines("..\seasons.txt")
            |> Seq.map (Split "\t")
            |> Seq.map (
                fun n -> 
                    n.[1] |> ExtractSportCountryAndLeagueFromLink |> (fun (s, c, _) -> (s, c)), n.[1], n.[2])
            |> Seq.groupBy (fun (p, _, _) -> p)
            |> dict

        let getLeagueName sport country gameLink =
            let (_, _, leagueName) = 
                leagues.[(sport, country)]
                |> Seq.find (fun (_, link, _) -> StartsWith link gameLink)
            leagueName

        repository.fixGames getLeagueName

