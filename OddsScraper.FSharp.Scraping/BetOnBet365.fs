namespace OddsScraper.FSharp.Scraping

module BetOnBet365 =
    open canopy
    open canopy.classic
    open OddsScraper.FSharp.Common
    open OddsScraper.FSharp.CommonScraping
    open Common
    open Models
    open OptionExtension
    open CanopyExtensions
    open NodeExtensions
    open FutureGamesDownload

    let baseMobileWebsite = "https://mobile.bet365.dk"
    let baseWebsite = "https://www.bet365.dk"
    
    let prependBaseWebsite href = System.String.Format("{0}{1}", baseWebsite, href)


    let sportsMappings = [("soccer", "/#/AS/B1/")]
    let soccerLeagueMappings = [
        ({ Country = "england"; League = "premier-league"}, "England Premier League")
        ({ Country = "germany"; League = "bundesliga"}, "Germany Bundesliga I")
        ({ Country = "serbia"; League = "super-liga"}, "Serbia Super Liga")
        ({ Country = "spain"; League = "laliga"}, "Spain Primera Liga")
        ]
    let getSportLink sport = 
        sportsMappings |> Seq.find (fun s -> s |> fst = sport) |> snd |> prependBaseWebsite
    let getLeagueName leagueInfo = 
        soccerLeagueMappings |> Seq.find (fun s -> s |> fst = leagueInfo) |> snd

    let login() =
        navigateToPage baseMobileWebsite
        click "//div[contains(string(), 'Log ind']"

        let (username, password) = getUsernameAndPassword()
        "#PopUp_UserName" << username
        "#PopUp_Password" << password
        click "#LogInPopUpBttn"

    let getBalance() = 
        (element "hm-Balance ").Text |> TryParseDouble

    let getLeagueElementToClick leagueText= 
        elements "sm-CouponLink_Label "
        |> Seq.tryFind (fun e -> e.Text = leagueText)
    
    let getGamesElements =
        elements "sl-CouponParticipantWithBookCloses_Name "

    let dateToString (date: System.DateTime) = System.String.Format("{0:ddd dd MMM}", date)

    let getDateElement date =
        elements "gl-MarketColumnHeader sl-MarketHeaderLabel sl-MarketHeaderLabel_Date "
        |> Seq.tryFind (fun d -> d.Text = (dateToString date))
    
    let betToString bet =
        match bet with
        | Home -> "1"
        | Draw -> "X"
        | Away -> "2"
    let getHeaderElement bet =
        elements "gl-MarketColumnHeader"
        |> Seq.tryFind (fun d -> d.Text = (betToString bet))

    let getLeagueElementForGame game =
        { Country = game.Country; League = game.League } 
        |> getLeagueName 
        |> getLeagueElementToClick

    let bet game =
        option {
            navigateToPage (game.Sport |> getSportLink)
        
            let! balance = getBalance()
            
            let! leagueElement = getLeagueElementForGame game
            click (leagueElement)

            let! dateElement = getDateElement game.Date
            let participantsColumn = parent dateElement

            return ()
        } |> ignore

    let betOnGames games =
        login()
        navigateToPage baseWebsite

        games |> Seq.iter bet
    
    let testRun() =
        initialize()

        [
            {
                Sport = "soccer"; Country = "england"; League = "premier-league"
                HomeTeam = "Chelsea"; AwayTeam = "Arsenal";
                Date = System.DateTime.Today; GameLink = "http://www.oddsportal.com/soccer/england/premier-league/chelsea-arsenal-zZ6f59Ue/"
                HomeMeanOdd = 1.75; DrawMeanOdd = 3.93 ; AwayMeanOdd = 4.47
                HomeOdd = 1.8; DrawOdd = 3.96 ; AwayOdd = 4.5
            }]
        |> betOnGames