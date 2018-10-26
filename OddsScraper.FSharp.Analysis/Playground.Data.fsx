module Playground.Data

#load "Playground.Engine.fsx"
#r "../packages/FSharp.Data.3.0.0/lib/net45/FSharp.Data.dll"
open FSharp.Data
open Playground.Engine

type GamesCsv = CsvProvider<"../OddsScraper.Analysis/soccer_england_premier-league.csv">

let meanBookies = ["bwin"; "Pinnacle"; "William Hill"]

let getMeanOdds odds =
    {
        HomeOdd = odds |> meanFromFunc (fun a -> float a.HomeOdd) |> toEuOdd
        DrawOdd = odds |> meanFromFunc (fun a -> float a.DrawOdd) |> toEuOdd
        AwayOdd = odds |> meanFromFunc (fun a -> float a.AwayOdd) |> toEuOdd
        Name = ""
    }
let gameFromData (home, away, date, hScore, aScore, season) = 
    { HomeTeam = home; AwayTeam = away; 
        Date = date; Season = season;
        HomeScore = hScore; AwayScore = aScore; 
        }

type Games(path:string) =
    let gamesCsv = GamesCsv.Load(path)

    member __.GetGames() =
        gamesCsv.Rows
        |> Seq.groupBy (fun r -> (r.HomeTeam, r.AwayTeam, r.Date, r.HomeTeamScore, r.HomeTeamScore, r.Season))
        |> Seq.map (fun (gameData, games) -> 
            {
                Game = gameData |> gameFromData
                Odds = games |> Seq.map (fun g -> 
                    { 
                        HomeOdd = g.HomeOdd |> float |> toEuOdd;
                        DrawOdd = g.DrawOdd |> float |> toEuOdd; 
                        AwayOdd = g.AwayOdd |> float |> toEuOdd; 
                        Name = g.Bookmaker }
                    |> normalizeGameOdds
                        ) |> Seq.toList
                Mean = 
                    let b = games |> Seq.filter (fun o -> meanBookies |> Seq.contains o.Bookmaker)
                    { HomeOdd = b |> meanFromFunc (fun g -> float g.HomeOdd) |> toEuOdd; 
                        DrawOdd = b |> meanFromFunc (fun g -> float g.DrawOdd) |> toEuOdd; 
                        AwayOdd = b |> meanFromFunc (fun g -> float g.AwayOdd) |> toEuOdd; 
                        Name = "" }
                    |> normalizeGameOdds
            })

let getGames league =
    (league,
        Games(@"..\OddsScraper.Analysis\" + league + ".csv").GetGames() 
        |> Seq.toList)

// let argGames = getGames "soccer_argentina_superliga"
let azGames = getGames "soccer_azerbaijan_premier-league"
// let belGames = getGames "soccer_belgium_jupiler-league"
// let braGames = getGames "soccer_brazil_serie-a"
let chiGames = getGames "soccer_china_super-league"
let czGames = getGames "soccer_czech-republic_1-liga"
let eng2Games = getGames "soccer_england_championship"
let engGames = getGames "soccer_england_premier-league"
// let finGames = getGames "soccer_finland_veikkausliiga"
// let fraGames = getGames "soccer_france_ligue-1"
// let denGames = getGames "soccer_denmark_superliga"
let espGames = getGames "soccer_spain_laliga"
let gerGames = getGames "soccer_germany_bundesliga"
let greGames = getGames "soccer_greece_super-league"
// let holGames = getGames "soccer_netherlands_eredivisie"
// let norGames = getGames "soccer_norway_eliteserien"
let indGames = getGames "soccer_indonesia_liga-1"
// let irGames = getGames "soccer_iran_persian-gulf-pro-league"
let isrGames = getGames "soccer_israel_ligat-ha-al"
// let itGames = getGames "soccer_italy_serie-a"
// let japGames = getGames "soccer_japan_j-league"
let japCupGames = getGames "soccer_japan_emperors-cup"
let kazGames = getGames "soccer_kazakhstan_premier-league"
// let malGames = getGames "soccer_malaysia_super-league"
let porGames = getGames "soccer_portugal_primeira-liga"
let polGames = getGames "soccer_poland_ekstraklasa"
let romGames = getGames "soccer_romania_liga-1"
let rusGames = getGames "soccer_russia_premier-league"
// let rusCupGames = getGames "soccer_russia_russian-cup"
let scoGames = getGames "soccer_scotland_premiership"
let serGames = getGames "soccer_serbia_super-liga"
// let sloGames = getGames "soccer_slovakia_fortuna-liga"
let sokGames = getGames "soccer_south-korea_k-league-1"
// let sweGames = getGames "soccer_sweden_allsvenskan"
// let swiGames = getGames "soccer_switzerland_super-league"
let turGames = getGames "soccer_turkey_super-lig"
// let usaGames = getGames "soccer_usa_mls"
let welsGames = getGames "soccer_wales_premier-league"
let clGames = getGames "soccer_europe_champions-league"
let elGames = getGames "soccer_europe_europa-league"

// let bAbaGames = getGames "basketball_europe_aba-league"
// let bChiGames = getGames "basketball_china_cba"
// let bElGames = getGames "basketball_europe_euroleague"
// let bSokGames = getGames "basketball_south-korea_kbl"
// let bJapGames = getGames "basketball_japan_b-league"
// let bNbaGames = getGames "basketball_usa_nba"

let goodLeagues = 
    [gerGames; engGames; serGames; espGames; greGames;
        porGames; turGames; polGames; sokGames; chiGames; indGames;
        isrGames; kazGames; azGames; eng2Games; czGames; japCupGames;
        elGames; clGames; welsGames; romGames; rusGames; scoGames]
    |> List.collect snd
