namespace OddsScraper.FSharp.Scraping

module DatabaseFix =
    open OddsScraper.FSharp.Scraping
    open OddsScrapper.Repository.Repository

    open Common
    open ScrapingParts
    open OddsScrapper.Repository.Models
    open System
    
    let fixDatabase() =
        use repository = new DbRepository(@"../ArchiveData.db")
        let leagues = 
            System.IO.File.ReadLines("..\seasons.txt")
            |> Seq.map (Split "\t")
            |> Seq.map (
                fun n -> 
                    n.[1] |> ExtractSportCountryAndLeagueFromLink |> (fun (s, c, _) -> (s, c)), n.[1], n.[2])
            |> Seq.groupBy (fun (p, _, _) -> p)
            |> dict

        let updateGameLeagueInDbAsync game =
            async {
                return! (repository.UpdateGameLeagueAsync game) |> Async.AwaitTask
            }

        let fixGameLeagueAsync (game:Game) =
            async {
                let (_, _, leagueName) = 
                    leagues.[(game.League.Sport.Name, game.League.Country.Name)]
                    |> Seq.find (fun (_, link, _) -> StartsWith link game.GameLink)
            
                return! repository.GetOrCreateLeagueAsync(leagueName, false, game.League.Sport, game.League.Country) |> Async.AwaitTask
            }

        let updateGameLeagueAsync (game:Game) =
            async {
                let! league = fixGameLeagueAsync game
                game.League <- league
                do! updateGameLeagueInDbAsync game
                do! Console.Out.WriteLineAsync(game.League.Name) |> Async.AwaitTask
                return 0
            }

        repository.GetCustomGames()
        |> Seq.map updateGameLeagueAsync
        |> Seq.map Async.RunSynchronously
        |> Seq.fold (fun s c -> s + 1) 0

