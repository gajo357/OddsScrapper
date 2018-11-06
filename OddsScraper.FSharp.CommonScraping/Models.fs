module OddsScraper.FSharp.CommonScraping.Models


type Bet = Home | Draw | Away
type LeagueInfo = { Country: string; League: string }
type SportInfo = { Sport: string; Leagues: LeagueInfo list }
type Odds = { Home: float; Draw: float; Away: float }

type Game = 
    { 
        HomeTeam: string; AwayTeam: string; 
        Date: System.DateTime; GameLink: string 
        Sport: string; Country: string; League: string 
        HomeMeanOdd: float; DrawMeanOdd: float; AwayMeanOdd: float
        HomeOdd: float; DrawOdd: float; AwayOdd: float
    }

let oddsFromGame g = { Home = g.HomeOdd; Draw = g.DrawOdd; Away = g.AwayOdd }
