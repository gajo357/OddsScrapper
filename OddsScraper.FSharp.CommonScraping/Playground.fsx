#r "../packages/FSharp.Data.3.0.0-beta/lib/net45/FSharp.Data.dll"
#r "../packages/FSharp.Charting.2.1.0/lib/net45/FSharp.Charting.dll"
#load "../packages/FSharp.Charting.2.1.0/FSharp.Charting.fsx"

open FSharp.Data

type GameProvider = HtmlProvider<"https://widgets.oddsportal.com/7812aab9e9b2e3d/s/">

let htmlString = FSharp.Data.Http.RequestString("https://widgets.oddsportal.com/7812aab9e9b2e3d/s/")
let html = (GameProvider.Parse htmlString).Html

let page = GameProvider.Load("https://widgets.oddsportal.com/7812aab9e9b2e3d/s/")

let getElementById (name: string) (node:HtmlDocument) = node.CssSelect(name).Head
let getElements (name: string) (node:HtmlNode) = node.Descendants(name)
let getFirstElement (name: string) (node:HtmlNode) = getElements name node |> Seq.head

let getGameInfosFromTable gamesTable = 
    gamesTable
    |> getTableRows
    |> Seq.choose getGameInfoFromRow

getElementById "#table" page.Html
let table = getFirstElement "table" (page.Html.Body())

let games = getGameInfosFromTable table |> Seq.toArray
