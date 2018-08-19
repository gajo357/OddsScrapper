namespace OddsScraper.FSharp.Scraping

module CanopyExtensions =
    open canopy
    open canopy.classic
    open OddsScraper.FSharp.Common.Common
    open OddsScraper.FSharp.Common.OptionExtension
    
    let initialize() = start classic.chrome

    let navigateToPage link = 
        url link

    let loginToOddsPortal() =
        let (username, password) = getUsernameAndPassword()
        url (ScrapingParts.prependBaseWebsite "/login/")
        
        "#login-username1" << username
        "#login-password1" << password
        click (last (text "Login"))

    let getPageHtml link =
        url link
        js "return document.documentElement.outerHTML" |> string

    let navigateAndReadGameHtml link =
        option {
            let! gameHtml = InvokeRepeatedIfFailed (fun () -> getPageHtml link)
            return gameHtml |> GamePageReading.parseGameHtml
        }
