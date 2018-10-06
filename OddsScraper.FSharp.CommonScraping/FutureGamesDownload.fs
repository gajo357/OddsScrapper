namespace OddsScraper.FSharp.CommonScraping

module FutureGamesDownload =
    open CanopyExtensions
    open HtmlNodeExtensions
    open GamePageReading
    open ScrapingParts
    open OddsScraper.FSharp.Common
    open Common
    open OptionExtension
    open BettingCalculations
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
    let emptyGame = {
        HomeTeam = ""; AwayTeam = ""; Date = System.DateTime.MinValue;
        GameLink = ""; Sport = ""; Country = ""; League = "";
        HomeMeanOdd = 1.0; DrawMeanOdd = 1.; AwayMeanOdd = 1.;
        HomeOdd = 1.; DrawOdd = 1.; AwayOdd = 1.
    }

    let meanBookies = ["bwin"; "Pinnacle"; "888sport"; "Unibet"; "William Hill"]

    let getDateAsString (date: System.DateTime) = System.String.Format("{0:yyyyMMdd}", date)

    let rowOdds tds =
        tds 
        |> Array.map getText 
        |> Array.choose TryParseDouble
        |> List.ofArray
        |> convertOddsListTo1x2

    let getGameInfoFromRow row =
        option {
            let tds = row |> getTdsFromRow |> Seq.toArray

            let! timeTd = tds |> Seq.tryHead
            let! time = timeTd |> getText |> TryParseDateTime

            let! hrefElem = 
                tds 
                |> Seq.collect getAllHrefElements
                |> Seq.tryFind (fun a -> a |> getText |> (Contains "-"))
            let link = hrefElem |> getHref
            let (homeTeam, awayTeam) = hrefElem |> getText |> ((Split "-") >> (fun p -> (p.[0], p.[1])))
            let (sport, country, league) = ExtractSportCountryAndLeagueFromLink link
            let (home, draw, away) = rowOdds tds

            return { emptyGame with 
                        Date = time; HomeTeam = homeTeam; AwayTeam = awayTeam;
                        GameLink =  prependBaseWebsite link;
                        Sport = sport; Country = country; League = league;
                        HomeOdd = home; DrawOdd = draw; AwayOdd = away
                        }
        }
    
    let getGameInfosFromTable = getTableRows >> Seq.choose getGameInfoFromRow
    
    let readMeanOdds unfiltered =
        let odds =
            unfiltered 
            |> Array.filter (fun o -> meanBookies |> Seq.contains o.Name)
            |> Array.map convertOddsTo1x2
        
        (odds |> meanFromFunc (fun (h,_,_) -> h),
            odds |> meanFromFunc (fun (_,d,_) -> d),
            odds |> meanFromFunc (fun (_,_,a) -> a))

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
            let bet365Odd = odds |>  Seq.tryFind (fun o -> o.Name = "bet365")
            let (homeMeanOdd, drawMeanOdd, awayMeanOdd) = odds |> readMeanOdds
            let (homeOdd, drawOdd, awayOdd) = 
                match bet365Odd with
                | Some b -> b |> convertOddsTo1x2
                | None -> (1.0, 1.0, 1.0)

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

    let sportDateUrl date sport = 
        "/matches/" + sport + "/" + getDateAsString date + "/"
        |> prependBaseWebsite

    let getGamesTableHtml date sport =
        sportDateUrl date sport
        |> navigateAndReadGameHtml
    
    let gamesFromGameTable sportInfo gamesTable = 
        gamesTable
        |> getGameInfosFromTable
        |> Seq.filter (fun g -> isGameLinkFromAnyLeague sportInfo g.GameLink)
    
    let gamesFromGameTablePage tableId sportInfo gamesHtml = 
        gamesHtml
        |> getElementById tableId
        |> gamesFromGameTable sportInfo
    
    let gamesFromRegularPage sportInfo gamesHtml = 
        gamesFromGameTablePage "#table-matches" sportInfo gamesHtml

    let downloadGameInfosForSport date (sportInfo: SportInfo) =
        match getGamesTableHtml date sportInfo.Sport with
        | Some gamesHtml -> gamesFromRegularPage sportInfo gamesHtml
        | None -> Seq.empty

    let downloadGamesForSport date (sportInfo: SportInfo) =
        downloadGameInfosForSport date sportInfo
        |> Seq.choose (fun g -> 
            match navigateAndReadGameHtml g.GameLink with
            | Some p -> Some (g.GameLink, p)
            | None -> None)
        |> Seq.choose readGame
    
    let downloadFutureGamesWithTrans getFunc date sports = sports |> Seq.collect (getFunc date)
    let downloadFutureGames = downloadFutureGamesWithTrans downloadGamesForSport
    let downloadFutureGameInfos = downloadFutureGamesWithTrans downloadGameInfosForSport

    let getLeagues() =
        System.IO.File.ReadLines("goodLeagues.csv")
        |> Seq.map (Common.Split ",")
        |> Seq.map (fun parts -> (parts.[0], parts.[1], parts.[2]))
        |> Seq.groupBy (fun (s,_,_) -> s)
        |> Seq.map (fun (sport, leagues) ->
            { 
                Sport = sport;
                Leagues = leagues |> Seq.map (fun (_, c, l) -> { Country = c; League = l}) |> Seq.toList
            })
    
    let downloadGames date = 
        downloadFutureGames date (getLeagues())
        |> Seq.sortBy (fun g -> g.Date)

    let downloadGameInfos date = downloadFutureGameInfos date (getLeagues())

    let readGameFromLink gameLink = 
        option {
            let! gameHtml = navigateAndReadGameHtml gameLink
            return! readGame(gameLink, gameHtml)
        }
        |> (fun go -> 
            match go with
            | Some g -> g
            | None -> emptyGame)


    let dateFromToday daysFromToday = System.DateTime.Now.AddDays(daysFromToday)
    let downloadTomorrowsGames()= downloadGames (dateFromToday 1.)
    let downloadTodaysGames()= downloadGames (dateFromToday 0.)
    
    let getGamesToBet() =
        loginToOddsPortal()
        downloadTodaysGames()

    let gameToString g =
        Common.joinCsv [|g.Sport; g.Country; g.League; 
            g.HomeTeam; g.AwayTeam; 
            g.Date.ToString();
            g.HomeOdd.ToString(); g.DrawOdd.ToString(); g.AwayOdd.ToString(); |]
        
    let printGamesToBet() =
        initialize()

        getGamesToBet()
        |> Seq.map gameToString
        |> (fun l -> System.IO.File.WriteAllLines("../gamesToBet.txt", l))
        
