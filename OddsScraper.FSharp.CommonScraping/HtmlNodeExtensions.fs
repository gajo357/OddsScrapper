namespace OddsScraper.FSharp.CommonScraping

module HtmlNodeExtensions =
    open FSharp.Data
    open OddsScraper.FSharp.Common.Common
    
    let getText (node:HtmlNode) =
        node.InnerText().Trim()

    let getElementById (name: string) (node:HtmlDocument) =
        node.CssSelect(name).Head

    let getElements (name: string) (node:HtmlNode) =
        node.Descendants(name)
        
    let getFirstElement (name: string) (node:HtmlNode) =
        getElements name node
        |> Seq.head

    let getAttribute attribute (node:HtmlNode) = node.AttributeValue(attribute)

    let getHref node = getAttribute "href" node

    let getClassAttribute node = getAttribute "class" node

    let getElementsByClassName name (node:HtmlNode) =
        node.Descendants(fun n -> (n |> getClassAttribute) = name)

    let getTableRows node = getElements "tr" node

    let getTdsFromRow node = getElements "td" node

    let getAllHrefElements node = getElements "a" node

    let getAllHrefAttributeAndText node =
        getAllHrefElements node
        |> Seq.map (fun n -> (getHref n, getText n))
        |> Seq.filter (fst >> IsNonEmptyString)

    let getAllHrefFromElement node =
        getAllHrefAttributeAndText node
        |> Seq.map fst

    let classAttributeEquals text node =
        getClassAttribute node = text

    let classAttributeContains text node =
        node|> getClassAttribute |> Contains text

