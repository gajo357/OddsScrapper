namespace OddsScraper.FSharp.Scraping

module RepositoryMethods =
    open OddsScrapper.Repository.Repository
    open OddsScraper.FSharp.Scraping.OptionExtension
    
    
    let RunRepositoryMethod repository getMethod =
        getMethod repository >> Async.RunSynchronously

    let GetFromRepository repository getMethod =
        (getMethod repository >> Async.RunSynchronously) >> ReturnFrom_Nullable

    let GetSportAsync (repository:IDbRepository) sportName =
        async {
            return! 
                repository.GetSportAsync(sportName) |> Async.AwaitTask
        }

    let GetCountryAsync (repository:IDbRepository) countryName =
        async {
            return! repository.GetCountryAsync(countryName) |> Async.AwaitTask
        }

    let GetLeaguesAsync sport country (repository:IDbRepository) =
        async {
            return! repository.GetLeaguesAsync(sport, country) |> Async.AwaitTask
        }

    let GetLeagueAsync sport country (repository:IDbRepository) leagueName =
        async {
            return! repository.GetLeagueAsync(leagueName, sport, country) |> Async.AwaitTask
        }

    let GetAllGamesAsync (repository:IDbRepository) league =
        async {
            return! repository.GetAllLeagueGamesAsync(league) |> Async.AwaitTask
        }

