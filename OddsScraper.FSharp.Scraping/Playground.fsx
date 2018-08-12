System.Environment.CurrentDirectory <- @"C:\Users\Gajo\Documents\Visual Studio 2017\Projects\OddsScrapper\OddsScraper.Analysis\"

#r "../packages/FSharp.Data.2.4.6/lib/net45/FSharp.Data.dll"
open FSharp.Data

type GamesCsv = CsvProvider<"..\OddsScraper.Analysis\soccer_england_premier-league.csv">
type game = { HomeTeam: string; AwayTeam: string; Date: System.DateTime; HomeScore: int; AwayScore: int }
type gameOdd = { HomeOdd: float; DrawOdd: float; AwayOdd: float; Name: string }
type groupedGame = { Game: game; Odds: gameOdd list; Mean: gameOdd } 

let meanBookies = ["bet365"; "bwin"; "Pinnacle"; "888sport"; "Unibet"; "William Hill"]

let mean (values: float seq) = values |> Seq.average
let meanFromFunc propFunc = (Seq.map propFunc) >> mean
let meanFromSecond propFunc = snd >> meanFromFunc propFunc
let getMeanOdds odds =
    {
        HomeOdd = odds |> meanFromFunc (fun a -> a.HomeOdd)
        DrawOdd = odds |> meanFromFunc (fun a -> a.DrawOdd) 
        AwayOdd = odds |> meanFromFunc (fun a -> a.AwayOdd)
        Name = ""
    }
let gameFromData (home, away, date, hScore, aScore) = { HomeTeam = home; AwayTeam = away; Date = date; HomeScore = hScore; AwayScore = aScore }

type games(path:string, year:int) =
    let gamesCsv = GamesCsv.Load(path)

    member __.getGames() =
        gamesCsv.Rows
        |> Seq.filter (fun s -> s.Date.Year > year) 
        |> Seq.groupBy (fun r -> (r.HomeTeam, r.AwayTeam, r.Date, r.HomeTeamScore, r.HomeTeamScore))
        |> Seq.map (fun r -> 
            {
                Game = r |> fst |> gameFromData
                Odds = r |> snd |> Seq.map (fun g -> { HomeOdd = (float)g.HomeOdd; DrawOdd = (float)g.DrawOdd; AwayOdd = (float)g.AwayOdd; Name = g.Bookmaker }) |> Seq.toList
                Mean = 
                    let b = r |> snd |> Seq.filter (fun o -> meanBookies |> Seq.contains o.Bookmaker)
                    { HomeOdd = b |> meanFromFunc (fun g -> (float)g.HomeOdd); DrawOdd = b |> meanFromFunc (fun g -> (float)g.DrawOdd); AwayOdd = b |> meanFromFunc (fun g -> (float)g.AwayOdd); Name = "" }
            })

let engGames = games("..\OddsScraper.Analysis\soccer_england_premier-league.csv", 2010).getGames() |> Seq.toList
let gerGames = games("..\OddsScraper.Analysis\soccer_germany_bundesliga.csv", 2010).getGames() |> Seq.toList
let espGames = games("..\OddsScraper.Analysis\soccer_spain_laliga.csv", 2010).getGames() |> Seq.toList

let kelly myOdd bookerOdd = 
    if (myOdd = 0.) then
        0.
    else if (bookerOdd = 1.) then
        0.
    else    
        (bookerOdd/myOdd - 1.) / (bookerOdd - 1.)

let makeBet win myOdd bookerOdd (amount, alreadyRun) =
    let k = kelly myOdd bookerOdd
    if (not alreadyRun && k * amount > 2. && k < 0.1) then
        if (win) then
            (amount + (bookerOdd - 1.)*k*amount, true)
        else 
            (amount*(1. - k), true)
    else
        (amount, alreadyRun)

let bookies = ["bet365"]
let betGame (g: game) (meanOdds: gameOdd) (gameOdds: gameOdd) (amount: float) =
    if (bookies |> Seq.contains gameOdds.Name |> not) then
        amount
    else
        (amount, false)
        |> makeBet (g.HomeScore > g.AwayScore) meanOdds.HomeOdd gameOdds.HomeOdd
        |> makeBet (g.HomeScore = g.AwayScore) meanOdds.DrawOdd gameOdds.DrawOdd
        |> makeBet (g.HomeScore < g.AwayScore) meanOdds.AwayOdd gameOdds.AwayOdd
        |> fst

let rec betGames g meanOdds odds amount =
    match odds with
    | head :: tail -> betGames g meanOdds tail (betGame g meanOdds head amount)
    | [] -> amount

let betGroupedGame amount (gg: groupedGame) = betGames gg.Game gg.Mean gg.Odds amount

let rec betAll amount games =
    match games with
    | head :: tail -> betAll (betGroupedGame amount head) tail
    | [] -> amount

let amountToBet win myOdd bookerOdd (amount, winAmount, alreadyRun) =
    let k = kelly myOdd bookerOdd
    if (not alreadyRun && k * amount > 2. && k < 0.06) then
        if (win) then
            (k*amount, (bookerOdd - 1.)*k*amount, true)
        else 
            (k*amount, -k*amount, true)
    else
        (amount, winAmount, alreadyRun)
let getAmountToBet g meanOdds gameOdds amount =
    (amount, 0., false)
    |> amountToBet (g.HomeScore > g.AwayScore) meanOdds.HomeOdd gameOdds.HomeOdd
    |> amountToBet (g.HomeScore = g.AwayScore) meanOdds.DrawOdd gameOdds.DrawOdd
    |> amountToBet (g.HomeScore < g.AwayScore) meanOdds.AwayOdd gameOdds.AwayOdd
    |> (fun (a, b, _) -> (a, b))
let rec betAllDayGames amount (dayGames: groupedGame list) = 
    dayGames
    |> Seq.fold (fun (totalAmount, amountLeftToBet) gg ->
        let amountToBet, winAmount = getAmountToBet gg.Game gg.Mean (gg.Odds |> Seq.find (fun f -> bookies |> Seq.contains f.Name)) amountLeftToBet
        (totalAmount + winAmount, amountLeftToBet - amountToBet)
        ) (amount, amount)
    |> fst
let rec betAllByDay amount gamesByDay =
    match gamesByDay with
    | (_, head) :: tail -> betAllByDay (betAllDayGames amount (head |> Seq.toList)) tail
    | [] -> amount

espGames |> Seq.filter (fun s -> s.Game.Date.Year = 2011) |> Seq.sortBy (fun s -> s.Game.Date) |> Seq.groupBy (fun s -> (s.Game.Date.Year, s.Game.Date.Month, s.Game.Date.Day)) |> Seq.toList |> betAllByDay 1000.