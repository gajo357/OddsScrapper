namespace OddsScraper.Repository

module Models =
    open System

    type Id = int64
    type Score = int64
    type SimpleModel = { Id: Id; Name: string }
    type League = { IdName: SimpleModel; Sport: Id; Country: Id }
    type Game = { 
        HomeTeam: SimpleModel; AwayTeam: SimpleModel
        HomeScore: Score; AwayScore: Score
        Date: DateTime; Season: string
        IsPlayoffs: bool; IsOvertime: bool
        GameLink: string
        }
    type GameOdd = { 
        Game: Id; Bookkeeper: SimpleModel
        HomeOdd: float; DrawOdd: float; AwayOdd: float; 
        IsValid: bool }