// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open OddsScraper.FSharp.Scraping

open DownloadGames
open DatabaseFix
open FutureGamesDownload

[<EntryPoint>]
let main argv = 
    //download()
    
    FutureGamesDownload.printGamesToBet()
    //BetOnBet365.testRun()

    //System.Console.WriteLine(fixDatabase())
    0 // return an integer exit code
