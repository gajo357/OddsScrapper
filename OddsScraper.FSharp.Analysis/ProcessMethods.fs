namespace OddsScraper.FSharp.Analysis

module ProcessLeagues =
    open OddsScraper.Repository.Repository
    open OddsScraper.Repository.Models
    open OddsScraper.FSharp.Analysis.LeagueProcessing
    open OddsScraper.FSharp.Common.OptionExtension
    open OddsScraper.FSharp.Analysis.UserSelection

    let private getGames (repository: Project) l = 
        l.IdName.Id 
        |> repository.getAllLeagueGamesGroupedBySeason
        |> Seq.map (fun g -> (g.Key, g |> Seq.map (fun s -> (fst s, s |> snd |> Seq.filter (fun b -> Seq.contains b.Bookkeeper.Name meanBookies) |> Seq.toList)) |> Seq.toList))    // execute Queries
        |> Seq.toList

    let processAll (repository: Project) =
        let getSport l = repository.getSportById l.Sport
        let getCountry l = repository.getCountryById l.Country
        let getLeagueGames = getGames repository

        repository.getAllLeagues()
        |> Seq.iter (fun l -> processLeague (getSport l) (getCountry l) l (getLeagueGames l))

    
    let processSelected repository =
        option {
            let! (sport, country, league) = chooseLeague repository
            processLeague sport country league (league |> getGames repository)
        } |> ignore

