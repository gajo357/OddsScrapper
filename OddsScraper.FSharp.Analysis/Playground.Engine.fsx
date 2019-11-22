module Playground.Engine






























[<Measure>]
type dkk

[<Measure>]
type pct

[<Measure>]
type euOdd

type Game =
    { HomeTeam: string
      AwayTeam: string
      Date: System.DateTime
      HomeScore: int
      AwayScore: int
      Season: string }

type GameOdd =
    { HomeOdd: float<euOdd>
      DrawOdd: float<euOdd>
      AwayOdd: float<euOdd>
      Name: string }

type GroupedGame =
    { Game: Game
      Odds: GameOdd list
      Mean: GameOdd }

type Winner =
    | Home
    | Draw
    | Away

type BetPlaced =
    { Kelly: float<pct>
      Winner: Winner
      BookerOdd: float<euOdd>
      MyOdd: float<euOdd>
      MoneyWon: float<dkk>
      MoneyPlaced: float<dkk> }


let mean (values: float seq) =
    if (values |> Seq.isEmpty) then 1.
    else values |> Seq.average

let meanFromFunc propFunc = (Seq.map propFunc) >> mean
let meanFromSecond propFunc = snd >> meanFromFunc propFunc

let invert = (/) 1.
let toEuOdd = ((*) 1.<euOdd>)
let toPct = ((*) 1.<pct>)

let euOddToPct: float<euOdd> -> float<pct> =
    float
    >> invert
    >> toPct

let pctToEuOdd: float<pct> -> float<euOdd> =
    float
    >> invert
    >> toEuOdd

let normalizePct h d a: float<pct> * float<pct> * float<pct> =
    let whole =
        [ h; d; a ]
        |> List.map float
        |> List.sumBy id

    match [ h; d; a ]
          |> List.map
              (float
               >> (fun e -> e / whole)
               >> toPct) with
    | [ h; d; a ] -> (h, d, a)
    | _ -> (1.<pct>, 1.<pct>, 1.<pct>)

let normalizeOdds h d a =
    let h, d, a = normalizePct (euOddToPct h) (euOddToPct d) (euOddToPct a)
    (pctToEuOdd h, pctToEuOdd d, pctToEuOdd a)

let normalizeGameOdds odds =
    let (h, d, o) = normalizeOdds odds.HomeOdd odds.DrawOdd odds.AwayOdd
    { odds with
          HomeOdd = h
          DrawOdd = d
          AwayOdd = o }

let inline median input =
    let sorted =
        input
        |> Seq.toArray
        |> Array.sort

    let m1, m2 =
        let len = sorted.Length - 1 |> float
        len / 2.
        |> floor
        |> int,
        len / 2.
        |> ceil
        |> int

    (sorted.[m1] + sorted.[m2] |> float) / 2.

let round (digits: int) (n: float) = System.Math.Round(n, digits)
let roundF2 = round 2

let roundMoney m =
    m
    |> float
    |> round 0
    |> (*) 1.<dkk>

let kelly myOdd bookerOdd =
    if (myOdd <= 1.<euOdd>) then 0.<pct>
    else if (bookerOdd <= 1.<euOdd>) then 0.<pct>
    else (bookerOdd / myOdd - 1.) / (bookerOdd - 1.<euOdd>) * 1.<pct*euOdd>

let gaus mu sigma x =
    let variance = sigma ** 2.
    1. / sqrt (2. * System.Math.PI * variance) * exp (-((x - mu) ** 2.) / (2. * variance))

let muStdDev data =
    let mu = mean data
    let variance = data |> List.averageBy (fun x -> (x - mu) ** 2.)
    mu, sqrt (variance)

let simpleMargin (margin: float<pct>) k (odd: float<euOdd>) = k <= margin

let moneyToBet margin myOdd bookerOdd amount =
    let k = kelly myOdd bookerOdd
    if (k > 0.0<pct> && margin k myOdd) then
        let m = k * amount * 1.<1/pct>
        if m < 2.0<dkk> then Some 2.0<dkk>
        else Some(m |> roundMoney)
    else
        None

let balanceChange margin win myOdd bookerOdd amount input =
    match input with
    | (_, _, true) -> input
    | (_, _, false) ->
        match moneyToBet margin myOdd bookerOdd amount with
        | Some money ->
            if (win) then ((float bookerOdd - 1.) * money |> roundMoney, money, true)
            else (-money, money, true)
        | None -> input

let winner compare g: bool = compare g.HomeScore g.AwayScore
let homeWin = winner (>)
let draw = winner (=)
let awayWin = winner (<)

let calcMoneyToBet kellyPct amount =
    if (kellyPct > 0.0<pct>) then
        let m = kellyPct * amount * 1.<1/pct>
        if m < 2.0<dkk> then Some 2.0<dkk>
        else Some(m |> roundMoney)
    else
        None

let calcBet win myOdd bookerOdd winner amount input =
    match input with
    | Some _ -> input
    | None ->
            let k = kelly myOdd bookerOdd
            match calcMoneyToBet k amount with
            | Some money ->
                let moneyWon =
                    if win then (float bookerOdd - 1.) * money
                    else -money
                Some
                    { Kelly = k
                      Winner = winner
                      BookerOdd = bookerOdd
                      MyOdd = myOdd
                      MoneyWon = moneyWon |> roundMoney
                      MoneyPlaced = money }
            | None -> None

let placeBetOnGame g meanOdds gameOdds amount =
    None
    |> calcBet (homeWin g) meanOdds.HomeOdd gameOdds.HomeOdd Home amount
    |> calcBet (draw g) meanOdds.DrawOdd gameOdds.DrawOdd Draw amount
    |> calcBet (awayWin g) meanOdds.AwayOdd gameOdds.AwayOdd Away amount

let getAmountToBet margin g meanOdds gameOdds amount =
    match gameOdds with
    | Some go ->
        match placeBetOnGame g meanOdds go amount with
        | Some bet ->
            if margin bet.Kelly bet.MyOdd then Some bet
            else None
        | None -> None
    | None -> None

let betGame margin g meanOdds gameOdds amount =
    if (gameOdds.Name <> "bet365") then
        amount
    else
        match getAmountToBet margin g meanOdds (Some gameOdds) amount with
        | Some bet -> amount + bet.MoneyWon
        | None -> amount

let rec betGames margin g meanOdds odds amount =
    match odds with
    | head :: tail -> betGames margin g meanOdds tail (betGame margin g meanOdds head amount)
    | [] -> amount

let betGroupedGame margin amount (gg: GroupedGame) = betGames margin gg.Game gg.Mean gg.Odds amount

let rec betAll margin amount games =
    match games with
    | head :: tail -> betAll margin (betGroupedGame margin amount head) tail
    | [] -> amount

let isFromSeason season gg =
    gg.Game.Date > System.DateTime(season, 8, 1) && gg.Game.Date < System.DateTime(season + 1, 8, 1)

let rec betAllDayGames margin amount (dayGames: GroupedGame list) bookie =
    dayGames
    |> Seq.fold (fun (totalAmount, amountLeftToBet) gg ->
        match getAmountToBet margin gg.Game gg.Mean (gg.Odds |> Seq.tryFind (fun f -> bookie = f.Name)) amountLeftToBet with
        | Some bet -> (totalAmount + bet.MoneyWon, amountLeftToBet - bet.MoneyPlaced)
        | None -> (totalAmount, amountLeftToBet)) (amount, amount)
    |> fst

let rec betAllByDay margin amount bookie gamesByDay =
    match gamesByDay with
    | (_, head) :: tail ->
        betAllByDay margin
            (betAllDayGames margin amount
                 (head
                  |> Seq.sortBy (fun s -> s.Game.Date)
                  |> Seq.toList) bookie) bookie tail
    | [] -> amount

let groupByDay games =
    games
    |> Seq.groupBy (fun s -> System.DateTime(s.Game.Date.Year, s.Game.Date.Month, s.Game.Date.Day))
    |> Seq.sortBy fst
    |> Seq.toList

let groupBySeason games =
    games
    |> Seq.groupBy (fun s -> s.Game.Season)
    |> Seq.sortBy fst

let groupByYear games =
    games
    |> Seq.groupBy (fun s -> s.Game.Date.Year)
    |> Seq.sortBy fst

let rec betDaily margin amount bookie acc gamesByDay =
    match gamesByDay with
    | (day, head) :: tail ->
        let amount = betAllDayGames margin amount (head |> Seq.toList) bookie
        betDaily margin amount bookie ((day, amount) :: acc) tail
    | [] -> acc

let daily margin bookie amount games =
    games
    |> groupByDay
    |> betDaily margin amount bookie []

let betByDay margin games bookie amount =
    games
    |> groupByDay
    |> betAllByDay margin amount bookie
    |> ((*) (1. / amount))

let betBySeason margin g bookie amount =
    g
    |> groupBySeason
    |> Seq.map (fun (s, games) -> (s, betByDay margin games bookie amount))

let betByMonth margin g bookie amount =
    g
    |> Seq.sortBy (fun s -> s.Game.Date)
    |> Seq.groupBy (fun s -> (s.Game.Date.Year, s.Game.Date.Month))
    |> Seq.map (fun ((y, m), games) -> (System.String.Format("{0},{1}", m, y), betByDay margin games bookie amount))

let betBySeasonMargin bookie games amount margin = (margin, betBySeason margin games bookie amount)
let betByMonthMargin bookie games amount margin = (margin, betByMonth margin games bookie amount)
