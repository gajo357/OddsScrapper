System.Environment.CurrentDirectory <- @"C:\Users\Gajo\Documents\Visual Studio 2017\Projects\OddsScrapper\OddsScraper.Analysis\"

#r "../packages/FSharp.Data.2.4.6/lib/net45/FSharp.Data.dll"
#r "../packages/FSharp.Charting.2.1.0/lib/net45/FSharp.Charting.dll"
#load "../packages/FSharp.Charting.2.1.0/FSharp.Charting.fsx"
open FSharp.Data
open FSharp.Charting
open System

type GamesCsv = CsvProvider<"../OddsScraper.Analysis/soccer_england_premier-league.csv">
type game = { HomeTeam: string; AwayTeam: string; Date: System.DateTime; HomeScore: int; AwayScore: int; Season: string }
type gameOdd = { HomeOdd: float; DrawOdd: float; AwayOdd: float; Name: string }
type groupedGame = { Game: game; Odds: gameOdd list; Mean: gameOdd } 

let meanBookies = ["bwin"; "Pinnacle"; "888sport"; "Unibet"; "William Hill"]

let mean (values: float seq) = 
    if (values |> Seq.isEmpty) then
        1.
    else 
        values |> Seq.average
let meanFromFunc propFunc = (Seq.map propFunc) >> mean
let meanFromSecond propFunc = snd >> meanFromFunc propFunc
let getMeanOdds odds =
    {
        HomeOdd = odds |> meanFromFunc (fun a -> a.HomeOdd)
        DrawOdd = odds |> meanFromFunc (fun a -> a.DrawOdd) 
        AwayOdd = odds |> meanFromFunc (fun a -> a.AwayOdd)
        Name = ""
    }
let gameFromData (home, away, date, hScore, aScore, season) = { HomeTeam = home; AwayTeam = away; Date = date; HomeScore = hScore; AwayScore = aScore; Season = season }

type games(path:string, year:int) =
    let gamesCsv = GamesCsv.Load(path)

    member __.getGames() =
        gamesCsv.Rows
        |> Seq.groupBy (fun r -> (r.HomeTeam, r.AwayTeam, r.Date, r.HomeTeamScore, r.HomeTeamScore, r.Season))
        |> Seq.map (fun r -> 
            {
                Game = r |> fst |> gameFromData
                Odds = r |> snd |> Seq.map (fun g -> { HomeOdd = (float)g.HomeOdd; DrawOdd = (float)g.DrawOdd; AwayOdd = (float)g.AwayOdd; Name = g.Bookmaker }) |> Seq.toList
                Mean = 
                    let b = r |> snd |> Seq.filter (fun o -> meanBookies |> Seq.contains o.Bookmaker)
                    { HomeOdd = b |> meanFromFunc (fun g -> (float)g.HomeOdd); DrawOdd = b |> meanFromFunc (fun g -> (float)g.DrawOdd); AwayOdd = b |> meanFromFunc (fun g -> (float)g.AwayOdd); Name = "" }
            })

let getGames league =
    (league,
        games(@"..\OddsScraper.Analysis\" + league + ".csv", 2010).getGames() 
        |> Seq.toList)

let argGames = getGames "soccer_argentina_superliga"
let braGames = getGames "soccer_brazil_serie-a"
let eng2Games = getGames "soccer_england_championship"
let engGames = getGames "soccer_england_premier-league"
let denGames = getGames "soccer_denmark_superliga"
let gerGames = getGames "soccer_germany_bundesliga"
let greGames = getGames "soccer_greece_super-league"
let fraGames = getGames "soccer_france_ligue-1"
let itGames = getGames "soccer_italy_serie-a"
let holGames = getGames "soccer_netherlands_eredivisie"
let porGames = getGames "soccer_portugal_primeira-liga"
let serGames = getGames "soccer_serbia_super-liga"
let espGames = getGames "soccer_spain_laliga"
let turGames = getGames "soccer_turkey_super-lig"
let usaGames = getGames "soccer_usa_mls"
let clGames = getGames "soccer_europe_champions-league"
let elGames = getGames "soccer_europe_europa-league"

let round (n: float) = System.Math.Round(n, 2)
let kelly myOdd bookerOdd = 
    if (myOdd = 0.) then
        0.
    else if (bookerOdd = 1.) then
        0.
    else    
        (bookerOdd/myOdd - 1.) / (bookerOdd - 1.)

let betAmount = 500.
let bookies = ["bwin"; "bet365"; "Betfair"; "888sport"; "Unibet"]

let makeBet win myOdd bookerOdd (amount, alreadyRun) =
    let k = kelly myOdd bookerOdd
    if (not alreadyRun && k * amount > 2. && k < 0.03) then
        let moneyToBet = k*amount |> round
        if (win) then
            (amount - moneyToBet + (bookerOdd*moneyToBet |> round), true)
        else 
            (amount - moneyToBet, true)
    else
        (amount, alreadyRun)
let betGame (g: game) (meanOdds: gameOdd) (gameOdds: gameOdd) (amount: float) =
    if (gameOdds.Name <> "bet365") then
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
let getSeason season gg = gg.Game.Date > DateTime(season, 8, 1) && gg.Game.Date < DateTime(season + 1, 8, 1)

[2011..2018]
|> Seq.map (fun s ->
    (s, 
        gerGames |> snd |> (Seq.append (engGames |> snd)) |> (Seq.append (serGames |> snd)) 
        |> (Seq.append (espGames |> snd)) |> (Seq.append (greGames |> snd))
        |> (Seq.append (porGames |> snd)) |> (Seq.append (turGames|> snd)) 
        //|> Seq.filter (fun s -> s.Game.Date.Year > 2016)
        |> Seq.filter (getSeason s)
        |> Seq.sortBy (fun s -> s.Game.Date) 
        |> Seq.toList 
        |> betAll betAmount 
        |> (fun b -> b/betAmount)))
|> Seq.toArray

let amountToBet margin win myOdd bookerOdd (amount, winAmount, alreadyRun) =
    let k = kelly myOdd bookerOdd
    if (not alreadyRun && k * amount > 2. && k < margin) then
        let moneyToBet = k*amount |> round
        if (win) then
            (moneyToBet, (bookerOdd - 1.)*moneyToBet |> round, true)
        else 
            (moneyToBet, -moneyToBet, true)
    else
        (amount, winAmount, alreadyRun)
let getAmountToBet margin g meanOdds gameOdds amount =
    match gameOdds with
    | Some go ->
        (amount, 0., false)
        |> amountToBet margin (g.HomeScore > g.AwayScore) meanOdds.HomeOdd go.HomeOdd
        |> amountToBet margin (g.HomeScore = g.AwayScore) meanOdds.DrawOdd go.DrawOdd
        |> amountToBet margin (g.HomeScore < g.AwayScore) meanOdds.AwayOdd go.AwayOdd
        |> (fun (a, b, _) -> (a, b))
    | None -> (amount, 0.)
let rec betAllDayGames margin amount (dayGames: groupedGame list) bookie = 
    dayGames
    |> Seq.fold (fun (totalAmount, amountLeftToBet) gg ->
        let amountToBet, winAmount = getAmountToBet margin gg.Game gg.Mean (gg.Odds |> Seq.tryFind (fun f -> bookie = f.Name)) amountLeftToBet
        (totalAmount + winAmount, amountLeftToBet - amountToBet)
        ) (amount, amount)
    |> fst
let rec betAllByDay margin amount bookie gamesByDay =
    match gamesByDay with
    | (_, head) :: tail -> betAllByDay margin (betAllDayGames margin amount (head |> Seq.sortBy (fun s -> s.Game.Date) |> Seq.toList) bookie) bookie tail 
    | [] -> amount
let bet margin games bookie = 
    games
    |> Seq.groupBy (fun s -> (s.Game.Date.Year, s.Game.Date.Month, s.Game.Date.Day)) 
    |> Seq.toList 
    |> betAllByDay margin betAmount bookie
    |> (fun b -> b/betAmount)
let betBySeason margin g bookie = 
    g 
    |> Seq.groupBy (fun s -> s.Game.Season)
    |> Seq.sortBy fst
    |> Seq.map (fun (s, games) -> (s, bet margin games bookie))
let betByBookie margin g =
    bookies
    |> Seq.map (fun b -> (b, betBySeason margin g b))

let joinTab (s: Object seq) = System.String.Join("\t", s)
let printAnalysis margin g = 
    betByBookie margin g 
    |> Seq.iter (fun (bookie, seasons) -> 
        printfn "%A" bookie
        seasons 
        |> Seq.iter (fun (season, result) -> printfn "%A - %f" season result)
        )

let plotAnalysis margins (league, games) = 
    Chart.Combine(
        margins
        |> Seq.map (fun m -> (m, betBySeason m games "bet365"))
        |> Seq.map (fun (m, seasons) -> Chart.Line(seasons, Name = m.ToString()))
        |> Seq.toList
    ).WithLegend(Alignment = Drawing.StringAlignment.Near, Docking = ChartTypes.Docking.Left).WithYAxis(MajorGrid = ChartTypes.Grid(Interval = 1.)).WithTitle(Text = league)

[engGames; eng2Games; denGames; gerGames; greGames; fraGames; itGames; holGames; porGames; serGames; espGames; turGames; usaGames; clGames]
[argGames; braGames]
[elGames]
|> Seq.map (plotAnalysis [0.01..0.01..0.1])
|> Seq.iter (fun p -> p.ShowChart() |> ignore)

printAnalysis 0.03 (serGames |> snd)