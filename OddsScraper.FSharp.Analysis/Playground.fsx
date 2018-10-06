﻿System.Environment.CurrentDirectory <- @"D:\Projects\OddsScrapper\OddsScraper.Analysis\"
System.Environment.CurrentDirectory <- @"C:\Users\gm.DK\Documents\GitHub\OddsScrapper\OddsScraper.Analysis\"

#r "../packages/FSharp.Data.3.0.0-beta/lib/net45/FSharp.Data.dll"
#r "../packages/FSharp.Charting.2.1.0/lib/net45/FSharp.Charting.dll"
#load "../packages/FSharp.Charting.2.1.0/FSharp.Charting.fsx"
open FSharp.Data
open FSharp.Charting
open System

type GamesCsv = CsvProvider<"../OddsScraper.Analysis/soccer_england_premier-league.csv">
type Game = { HomeTeam: string; AwayTeam: string; Date: System.DateTime; HomeScore: int; AwayScore: int; Season: string }
type GameOdd = { HomeOdd: float; DrawOdd: float; AwayOdd: float; Name: string }
type GroupedGame = { Game: Game; Odds: GameOdd list; Mean: GameOdd } 

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

type Games(path:string, year:int) =
    let gamesCsv = GamesCsv.Load(path)

    member __.GetGames() =
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
        Games(@"..\OddsScraper.Analysis\" + league + ".csv", 2010).GetGames() 
        |> Seq.toList)

let argGames = getGames "soccer_argentina_superliga"
let azGames = getGames "soccer_azerbaijan_premier-league"
let belGames = getGames "soccer_belgium_jupiler-league"
let braGames = getGames "soccer_brazil_serie-a"
let chiGames = getGames "soccer_china_super-league"
let czGames = getGames "soccer_czech-republic_1-liga"
let eng2Games = getGames "soccer_england_championship"
let engGames = getGames "soccer_england_premier-league"
let finGames = getGames "soccer_finland_veikkausliiga"
let fraGames = getGames "soccer_france_ligue-1"
let denGames = getGames "soccer_denmark_superliga"
let espGames = getGames "soccer_spain_laliga"
let gerGames = getGames "soccer_germany_bundesliga"
let greGames = getGames "soccer_greece_super-league"
let holGames = getGames "soccer_netherlands_eredivisie"
let norGames = getGames "soccer_norway_eliteserien"
let indGames = getGames "soccer_indonesia_liga-1"
let irGames = getGames "soccer_iran_persian-gulf-pro-league"
let isrGames = getGames "soccer_israel_ligat-ha-al"
let itGames = getGames "soccer_italy_serie-a"
let japGames = getGames "soccer_japan_j-league"
let japCupGames = getGames "soccer_japan_emperors-cup"
let kazGames = getGames "soccer_kazakhstan_premier-league"
let malGames = getGames "soccer_malaysia_super-league"
let porGames = getGames "soccer_portugal_primeira-liga"
let polGames = getGames "soccer_poland_ekstraklasa"
let romGames = getGames "soccer_romania_liga-1"
let rusGames = getGames "soccer_russia_premier-league"
let rusCupGames = getGames "soccer_russia_russian-cup"
let scoGames = getGames "soccer_scotland_premiership"
let serGames = getGames "soccer_serbia_super-liga"
let sloGames = getGames "soccer_slovakia_fortuna-liga"
let sokGames = getGames "soccer_south-korea_k-league-1"
let sweGames = getGames "soccer_sweden_allsvenskan"
let swiGames = getGames "soccer_switzerland_super-league"
let turGames = getGames "soccer_turkey_super-lig"
let usaGames = getGames "soccer_usa_mls"
let welsGames = getGames "soccer_wales_premier-league"
let clGames = getGames "soccer_europe_champions-league"
let elGames = getGames "soccer_europe_europa-league"

let bAbaGames = getGames "basketball_europe_aba-league"
let bChiGames = getGames "basketball_china_cba"
let bElGames = getGames "basketball_europe_euroleague"
let bSokGames = getGames "basketball_south-korea_kbl"
let bJapGames = getGames "basketball_japan_b-league"
let bNbaGames = getGames "basketball_usa_nba"

let goodLeagues = 
    gerGames |> snd 
    |> (Seq.append (engGames |> snd)) |> (Seq.append (serGames |> snd)) 
    |> (Seq.append (espGames |> snd)) |> (Seq.append (greGames |> snd))
    |> (Seq.append (porGames |> snd)) |> (Seq.append (turGames|> snd))
    |> (Seq.append (polGames|> snd)) |> (Seq.append (sokGames|> snd))
    |> (Seq.append (chiGames|> snd)) |> (Seq.append (indGames|> snd))
    |> (Seq.append (isrGames|> snd)) |> (Seq.append (kazGames|> snd))
    |> (Seq.append (azGames|> snd)) |> (Seq.append (eng2Games|> snd))

let round (digits: int) (n: float) = System.Math.Round(n, digits)
let roundF2 = round 2
let kelly myOdd bookerOdd = 
    if (myOdd = 0.) then
        0.
    else if (bookerOdd = 1.) then
        0.
    else    
        (bookerOdd/myOdd - 1.) / (bookerOdd - 1.)
let shuffleG xs = xs |> Seq.sortBy (fun _ -> Guid.NewGuid())
let takePercent percent xs =
    let nElem = System.Convert.ToInt32(float (xs |> Seq.length) * percent)
    xs |> shuffleG |> Seq.take nElem

let betAmount = 500.
let bookies = ["bwin"; "bet365"; "Betfair"; "888sport"; "Unibet"]
let moneyToBet kelly amount =
    let m = kelly * amount
    if m < 2.0 then 2.0
    else m |> roundF2
let amountToBet margin win myOdd bookerOdd (amount, amountBet, winAmount, alreadyRun) =
    let k = kelly myOdd bookerOdd
    if (not alreadyRun && k > 0.0 && k < margin) then
        let moneyToBet = moneyToBet k amount
        if (win) then
            (amount, moneyToBet, (bookerOdd - 1.)*moneyToBet |> roundF2, true)
        else 
            (amount, moneyToBet, -moneyToBet, true)
    else
        (amount, amountBet, winAmount, alreadyRun)
let getAmountToBet margin g meanOdds gameOdds amount =
    match gameOdds with
    | Some go ->
        (amount, 0., 0., false)
        |> amountToBet margin (g.HomeScore > g.AwayScore) meanOdds.HomeOdd go.HomeOdd
        |> amountToBet margin (g.HomeScore = g.AwayScore) meanOdds.DrawOdd go.DrawOdd
        |> amountToBet margin (g.HomeScore < g.AwayScore) meanOdds.AwayOdd go.AwayOdd
        |> (fun (_, amountToBet, winAmount, _) -> (amountToBet, winAmount))
    | None -> (0., 0.)

let betGame (g: Game) (meanOdds: GameOdd) (gameOdds: GameOdd) (amount: float) =
    if (gameOdds.Name <> "bet365") then
        amount
    else
        let _, winAmount = getAmountToBet 0.02 g meanOdds (Some gameOdds) amount
        amount + winAmount
let rec betGames g meanOdds odds amount =
    match odds with
    | head :: tail -> betGames g meanOdds tail (betGame g meanOdds head amount)
    | [] -> amount
let betGroupedGame amount (gg: GroupedGame) = betGames gg.Game gg.Mean gg.Odds amount
let rec betAll amount games =
    match games with
    | head :: tail -> betAll (betGroupedGame amount head) tail
    | [] -> amount
let getSeason season gg = gg.Game.Date > DateTime(season, 8, 1) && gg.Game.Date < DateTime(season + 1, 8, 1)

[2005..2018]
|> Seq.map (fun s ->
    (s, 
        goodLeagues
        |> takePercent 1.
        |> Seq.filter (getSeason s)
        |> Seq.sortBy (fun s -> s.Game.Date) 
        |> Seq.toList 
        |> betAll betAmount 
        |> (fun b -> b/betAmount)))
|> Seq.toArray

let rec betAllDayGames margin amount (dayGames: GroupedGame list) bookie = 
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
    |> takePercent 1.
    |> Seq.groupBy (fun s -> (s.Game.Date.Year, s.Game.Date.Month, s.Game.Date.Day)) 
    |> Seq.toList 
    |> betAllByDay margin betAmount bookie
    |> (fun b -> b/betAmount)
let betBySeason margin g bookie = 
    g 
    |> Seq.groupBy (fun s -> s.Game.Season)
    |> Seq.sortBy fst
    |> Seq.map (fun (s, games) -> (s, bet margin games bookie))
let betByMonth margin g bookie = 
    g 
    |> Seq.sortBy (fun s -> s.Game.Date)
    |> Seq.groupBy (fun s -> (s.Game.Date.Year, s.Game.Date.Month))
    |> Seq.map (fun ((y,m), games) -> (System.String.Format("{0},{1}", m, y), bet margin games bookie))
let betByBookie margin g =
    bookies
    |> Seq.map (fun b -> (b, betBySeason margin g b))
let betBySeasonMargin bookie games margin =
    (margin, betBySeason margin games bookie)
let betByMonthMargin bookie games margin =
    (margin, betByMonth margin games bookie)

let printAnalysis margin g = 
    betByBookie margin g 
    |> Seq.iter (fun (bookie, seasons) -> 
        printfn "%A" bookie
        seasons 
        |> Seq.iter (fun (season, result) -> printfn "%A - %f" season result)
        )
let printMonthAnalysis margin g = 
    betByMonthMargin "bet365" g margin
    |> snd
    |> Seq.iter (fun (month, seasons) -> printfn "%A - %f" month seasons)

let plotSeasonAnalysis margins (league, games) = 
    Chart.Combine(
        margins
        |> Seq.map ((betBySeasonMargin "bet365" games) >> (fun (m, seasons) -> Chart.Line(data = seasons, Name = m.ToString())))
        |> Seq.toList
    ).WithLegend(Alignment = Drawing.StringAlignment.Near, Docking = ChartTypes.Docking.Left).WithYAxis(Max = 10., MajorGrid = ChartTypes.Grid(Interval = 1.)).WithTitle(Text = league)
let plotMonthAnalysis margins (league, games) = 
    Chart.Combine(
        margins
        |> Seq.map ((betByMonthMargin "bet365" games) >> (fun (m, seasons) -> Chart.Line(data = seasons, Name = m.ToString())))
        |> Seq.toList
    ).WithLegend(Alignment = Drawing.StringAlignment.Near, Docking = ChartTypes.Docking.Left).WithYAxis(Max = 3., MajorGrid = ChartTypes.Grid(Interval = 1.)).WithTitle(Text = league)
let plotMonthlyAgainst margin gs =
    Chart.Combine(
        gs 
        |> Seq.map (fun g -> betByMonthMargin "bet365" g margin) 
        |> Seq.map snd
        |> Seq.map (fun g -> Chart.Line(g))
        |> Seq.toList
    ).WithLegend(Alignment = Drawing.StringAlignment.Near, Docking = ChartTypes.Docking.Left)

[engGames; gerGames; greGames;
    polGames; porGames; serGames; espGames; turGames; usaGames;
    clGames; elGames; rusGames; rusCupGames; scoGames; sokGames;
    chiGames]
[azGames; belGames; irGames; indGames; isrGames; kazGames]
[eng2Games; fraGames]
[romGames; czGames; welsGames; swiGames; sloGames]
[finGames]
|> Seq.map (plotMonthAnalysis [0.02;0.025;0.03])
|> Seq.iter (fun p -> p.ShowChart() |> ignore)

[azGames; belGames; irGames; indGames; isrGames; kazGames]
[eng2Games; fraGames]
|> Seq.map (plotSeasonAnalysis [0.02;0.025;0.03])
|> Seq.iter (fun p -> p.ShowChart() |> ignore)

printAnalysis 0.02 (engGames |> snd)
printMonthAnalysis 0.02 (goodLeagues)

plotMonthlyAgainst 0.02 [goodLeagues; goodLeagues |> (Seq.append (eng2Games|> snd))]
