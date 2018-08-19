namespace OddsScraper.FSharp.Scraping

module FutureGamesDownload =
    open CanopyExtensions
    open HtmlNodeExtensions
    open GamePageReading
    open OddsScraper.FSharp.Common
    open OptionExtension
    open BettingCalculations
    open OddsScraper.FSharp.Scraping.ScrapingParts
    open FSharp.Core

    type Bet = Home | Draw | Away
    type LeagueInfo = { Country: string; League: string }
    type SportInfo = { Sport: string; Leagues: LeagueInfo list }
    type Game = 
        { 
            HomeTeam: string; AwayTeam: string; 
            Date: System.DateTime; GameLink: string 
            Sport: string; Country: string; League: string 
            HomeMeanOdd: float; DrawMeanOdd: float; AwayMeanOdd: float
            HomeOdd: float; DrawOdd: float; AwayOdd: float
        }

    let meanBookies = ["bwin"; "Pinnacle"; "888sport"; "Unibet"; "William Hill"]

    let getDateAsString (date: System.DateTime) = System.String.Format("{0:yyyyMMdd}", date)

    let getGameTimeAndLinkFromRow row =
        option {
            let! timeTd = row |> getTdsFromRow |> Seq.tryHead
            let! time = timeTd |> getText |> Common.TryParseDateTime

            let! link = row |> (getAllHrefFromElement >> Seq.tryFind (Common.Contains "javascript" >> not))

            return (time, prependBaseWebsite link)
        }
    
    let getGameLinksFromTable gamesTable = 
        gamesTable
        |> getTableRows
        |> Seq.map getGameTimeAndLinkFromRow
        |> Seq.choose id
    
    let readMeanOdds unfiltered =
        let odds =
            unfiltered 
            |> Array.filter (fun o -> meanBookies |> Seq.contains o.Name)
            |> Array.map convertOddsListTo1x2
        
        (odds |> meanFromFunc (fun (h,_,_) -> h),
            odds |> meanFromFunc (fun (_,d,_) -> d),
            odds |> meanFromFunc (fun (_,_,a) -> a))

    let getAmountToBet amount myOdd bookerOdd =
        let k = kelly myOdd bookerOdd 
        if (k > 0. && k < 0.03) then
            k*amount
        else 0.

    let makeBet myOdd bookerOdd bet =
        let k = kelly myOdd bookerOdd 
        if (k > 0. && k < 0.03) then
            Some (bet, k, bookerOdd)
        else
            None

    let betBind myOdd bookerOdd bet result =
        match result with
        | Some _ -> result
        | _ -> makeBet myOdd bookerOdd bet

    let getBet (homeOdd, drawOdd, awayOdd) (homeMeanOdd, drawMeanOdd, awayMeanOdd) =
        makeBet homeMeanOdd homeOdd Home
        |> betBind drawMeanOdd drawOdd Draw
        |> betBind awayMeanOdd awayOdd Away
    
    let readGame(gameLink, gameHtml) =
        option {
            let odds = gameHtml |> getOddsFromGamePage
            let! bet365Odd = odds |> Array.filter (fun o -> not o.Deactivated) |>  Seq.tryFind (fun o -> o.Name = "bet365")
            let (homeMeanOdd, drawMeanOdd, awayMeanOdd) = odds |> readMeanOdds
            let (homeOdd, drawOdd, awayOdd) = bet365Odd |> convertOddsListTo1x2

            let participantsAndDateElement = getParticipantsAndDateElement gameHtml
            let (homeTeam, awayTeam) = readParticipantsNames participantsAndDateElement
            let gameDate = participantsAndDateElement |> (readGameDate >> getDateOrDefault)
            let (sport, country, league) = ExtractSportCountryAndLeagueFromLink gameLink

            return {
                HomeTeam = homeTeam; AwayTeam = awayTeam
                Date = gameDate; GameLink = gameLink
                Sport = sport; Country = country; League = league
                HomeMeanOdd = homeMeanOdd; DrawMeanOdd = drawMeanOdd; AwayMeanOdd = awayMeanOdd
                HomeOdd = homeOdd; DrawOdd = drawOdd; AwayOdd = awayOdd
            }
        }

    let isGameLinkFromAnyLeague (sportInfo: SportInfo) gameLink =
        if ((gameLink |> (Common.Remove BaseWebsite) |> GetLinkParts |> Array.length) < 3) then
            false
        else
            let (sport, country, league) = ExtractSportCountryAndLeagueFromLink gameLink
            if sportInfo.Sport <> sport then
                false
            else if (sportInfo.Leagues |> Seq.exists (fun s -> s.League = league && s.Country = country)) then
                true
            else
                false
    
    let isGameWithinTimeFrame (dateNow:System.DateTime) timeSpan gameDate =
        gameDate > dateNow && gameDate < dateNow.AddMinutes(timeSpan)

    let getGamesTableHtml date sport =
        "/matches/" + sport + "/" + getDateAsString date + "/"
        |> prependBaseWebsite
        |> navigateAndReadGameHtml

    let downloadGamesForSport date timeSpan (sportInfo: SportInfo) =    
        match getGamesTableHtml date sportInfo.Sport with
        | Some gamesHtml -> 
            gamesHtml
            |> getElementById "#table-matches"
            |> getGameLinksFromTable
            |> Seq.filter (fst >> isGameWithinTimeFrame date timeSpan)
            |> Seq.filter (snd >> isGameLinkFromAnyLeague sportInfo)
            |> Seq.map snd
            |> Seq.map (fun l -> 
                match navigateAndReadGameHtml l with
                | Some p -> Some (l, p)
                | None -> None)
            |> Seq.choose id
            |> Seq.map readGame
            |> Seq.choose id
        | None -> Seq.empty
    
    let downloadFutureGames date sports timeSpan = sports |> Seq.collect (downloadGamesForSport date timeSpan)

    let getLeagues() =
        //[
        //    { Sport = "soccer"; 
        //        Leagues = [
        //            { Country = "england"; League = "premier-league" }
        //            { Country = "germany"; League = "bundesliga" }
        //            { Country = "serbia"; League = "super-liga"}
        //            { Country = "spain"; League = "laliga"}
        //            { Country = "greece"; League = "super-league"}
        //            { Country = "portugal"; League = "primeira-liga"}
        //            { Country = "turkey"; League = "super-lig"}
        //            { Country = "europe"; League = "champions-league"}
        //            ] }
        //]
        System.IO.File.ReadLines("goodLeagues.csv")
        |> Seq.map (Common.Split ",")
        |> Seq.map (fun parts -> (parts.[0], parts.[1], parts.[2]))
        |> Seq.groupBy (fun (s,_,_) -> s)
        |> Seq.map (fun (sport, leagues) ->
            { 
                Sport = sport;
                Leagues = leagues |> Seq.map (fun (_, c, l) -> { Country = c; League = l}) |> Seq.toList
            })
    
    let downloadGames daysFromToday timeSpan = 
        downloadFutureGames (System.DateTime.Now.AddDays(daysFromToday)) (getLeagues()) timeSpan
        |> Seq.sortBy (fun g -> g.Date)
        |> Seq.toList

    let downloadTomorrowsGames timeSpan = downloadGames 1. timeSpan
    let downloadTodaysGames timeSpan = downloadGames 0. timeSpan


    let getGamesToBet timeSpan =
        loginToOddsPortal()
        downloadTodaysGames timeSpan

    let gameToString g =
        Common.joinCsv [|g.Sport; g.Country; g.League; 
            g.HomeTeam; g.AwayTeam; 
            g.Date.ToString();
            g.HomeOdd.ToString(); g.DrawOdd.ToString(); g.AwayOdd.ToString(); |]
        
    let printGamesToBet() =
        initialize()

        getGamesToBet 30.
        |> Seq.map gameToString
        |> (fun l -> System.IO.File.WriteAllLines("../gamesToBet.txt", l))
        
