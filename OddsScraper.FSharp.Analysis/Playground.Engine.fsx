module Playground.Engine

[<Measure>] type dkk
[<Measure>] type pct
[<Measure>] type euOdd

type Game = { HomeTeam: string; AwayTeam: string; Date: System.DateTime; HomeScore: int; AwayScore: int; Season: string }
type GameOdd = { HomeOdd: float<euOdd>; DrawOdd: float<euOdd>; AwayOdd: float<euOdd>; Name: string }
type GroupedGame = { Game: Game; Odds: GameOdd list; Mean: GameOdd }

let mean (values: float seq) = 
    if (values |> Seq.isEmpty) then
        1.
    else 
        values |> Seq.average
let meanFromFunc propFunc = (Seq.map propFunc) >> mean
let meanFromSecond propFunc = snd >> meanFromFunc propFunc

let invert = (/) 1.
let toEuOdd = ((*) 1.<euOdd>)
let toPct = ((*) 1.<pct>)
let euOddToPct = float >> invert >> toPct
let pctToEuOdd = float >> invert >> toEuOdd

let normalizePct h d a =
    let whole = h + d + a
    (h/whole, d/whole, a/whole)
let normalizeOdds h d a =
    let (h, d, a) = normalizePct (euOddToPct h) (euOddToPct d) (euOddToPct a)
    ((pctToEuOdd h), (pctToEuOdd d), (pctToEuOdd a))
let normalizeGameOdds odds =
    let (h, d, o) = normalizeOdds odds.HomeOdd odds.DrawOdd odds.AwayOdd
    { odds with
        HomeOdd = h
        DrawOdd = d
        AwayOdd = o
    }

let inline median input = 
    let sorted = input |> Seq.toArray |> Array.sort
    let m1, m2 = 
        let len = sorted.Length-1 |> float
        len/2. |> floor |> int, len/2. |> ceil |> int 
    (sorted.[m1] + sorted.[m2] |> float)/2.

let round (digits: int) (n: float) = System.Math.Round(n, digits)
let roundF2 = round 2
let roundMoney m = m |> float |> round 0 |> (*) 1.<dkk>

let kelly myOdd bookerOdd = 
    if (myOdd <= 0.<euOdd>) then 0.<pct>
    else if (bookerOdd = 1.<euOdd>) then 0.<pct>
    else (bookerOdd/myOdd - 1.) / (bookerOdd - 1.<euOdd>)*1.<pct*euOdd>

let simpleMargin (margin: float<pct>) k (odd: float<euOdd>) = 
    k <= margin
let moneyToBet margin myOdd bookerOdd amount =
    let k = kelly myOdd bookerOdd
    if (k > 0.0<pct> && margin k myOdd) then
        let m = k * amount * 1.</pct>
        if m < 2.0<dkk> then Some 2.0<dkk>
        else Some (m |> roundMoney)
    else None

let balanceChange margin win myOdd bookerOdd amount input =
    match input with
    | (_, _, true) -> input
    | (winAmount, moneyBet, _) ->
        match moneyToBet margin myOdd bookerOdd amount with
        | Some money -> 
            if (win) then
                ((float bookerOdd - 1.)*money |> roundMoney, money, true)
            else 
                (-money, money, true)
        | None -> 
            (winAmount, moneyBet, false)

let winner compare g : bool = compare g.HomeScore g.AwayScore
let homeWin = winner (>)
let draw = winner (=)
let awayWin = winner (<)

let getAmountToBet margin g meanOdds gameOdds amount =
    match gameOdds with
    | Some go ->
        (0.<dkk>, 0.<dkk>, false)
        |> balanceChange margin (homeWin g) meanOdds.HomeOdd go.HomeOdd amount
        |> balanceChange margin (draw g) meanOdds.DrawOdd go.DrawOdd amount
        |> balanceChange margin (awayWin g) meanOdds.AwayOdd go.AwayOdd amount
        |> (fun (winAmount, moneyBet, _) -> (winAmount, moneyBet))
    | None -> (0.<dkk>, 0.<dkk>)

let betGame margin g meanOdds gameOdds amount =
    if (gameOdds.Name <> "bet365") then
        amount
    else
        let _, balanceChange = getAmountToBet margin g meanOdds (Some gameOdds) amount
        amount + balanceChange

let rec betGames margin g meanOdds odds amount =
    match odds with
    | head :: tail -> betGames margin g meanOdds tail (betGame margin g meanOdds head amount)
    | [] -> amount

let betGroupedGame margin amount (gg: GroupedGame) = betGames margin gg.Game gg.Mean gg.Odds amount

let rec betAll margin amount games =
    match games with
    | head :: tail -> betAll margin (betGroupedGame margin amount head) tail
    | [] -> amount
let getSeason season gg = 
    gg.Game.Date > System.DateTime(season, 8, 1) && gg.Game.Date < System.DateTime(season + 1, 8, 1)

let rec betAllDayGames margin amount (dayGames: GroupedGame list) bookie = 
    dayGames
    |> Seq.fold (fun (totalAmount, amountLeftToBet) gg ->
        let winAmount, moneyBet = 
            getAmountToBet margin gg.Game gg.Mean 
                (gg.Odds |> Seq.tryFind (fun f -> bookie = f.Name)) 
                amountLeftToBet
        (totalAmount + winAmount, amountLeftToBet - moneyBet)
        ) (amount, amount)
    |> fst

let rec betAllByDay margin amount bookie gamesByDay =
    match gamesByDay with
    | (_, head) :: tail -> betAllByDay margin (betAllDayGames margin amount (head |> Seq.sortBy (fun s -> s.Game.Date) |> Seq.toList) bookie) bookie tail 
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
        betDaily margin amount bookie ((day, amount)::acc) tail
    | [] -> acc

let daily margin bookie amount games = 
    games
    |> groupByDay
    |> betDaily margin amount bookie []

let betByDay margin games bookie amount = 
    games
    |> groupByDay
    |> betAllByDay margin amount bookie
    |> ((*) (1./amount))

let betBySeason margin g bookie amount = 
    g
    |> groupBySeason
    |> Seq.map (fun (s, games) -> (s, betByDay margin games bookie amount))

let betByMonth margin g bookie amount = 
    g 
    |> Seq.sortBy (fun s -> s.Game.Date)
    |> Seq.groupBy (fun s -> (s.Game.Date.Year, s.Game.Date.Month))
    |> Seq.map (fun ((y,m), games) -> 
        (System.String.Format("{0},{1}", m, y), 
            betByDay margin games bookie amount))

let betBySeasonMargin bookie games amount margin =
    (margin, betBySeason margin games bookie amount)
let betByMonthMargin bookie games amount margin =
    (margin, betByMonth margin games bookie amount)
