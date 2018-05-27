namespace OddsScraper.FSharp.Scraping

module HtmlNodeExtensions =
    open FSharp.Data
    open Common
    
    let GetText (node:HtmlNode) =
        node.InnerText().Trim()

    let GetElementById (name: string) (node:HtmlDocument) =
        node.CssSelect(name).Head

    let GetElements (name: string) (node:HtmlNode) =
        node.Descendants(name)
        
    let GetFirstElement (name: string) (node:HtmlNode) =
        GetElements name node
        |> Seq.head

    let GetAttribute attribute (node:HtmlNode) = node.AttributeValue(attribute)

    let GetHref node = GetAttribute "href" node

    let GetClassAttribute node = GetAttribute "class" node

    let GetElementsByClassName name (node:HtmlNode) =
        node.Descendants(fun n -> (n |> GetClassAttribute) = name)

    let GetTableRows node = GetElements "tr" node

    let GetTdsFromRow node = GetElements "td" node

    let GetAllHrefElements node = GetElements "a" node

    let GetAllHrefAttributeAndText node =
        GetAllHrefElements node
        |> Seq.map (fun n -> (GetHref n, GetText n))
        |> Seq.filter (fst >> IsNonEmptyString)
        |> Seq.toArray

    let GetAllHrefFromElements node =
        GetAllHrefAttributeAndText node
        |> Seq.map fst
        |> Seq.toArray

    let ClassAttributeEquals text node =
        GetClassAttribute node = text

    let ClassAttributeContains text node =
        node|> GetClassAttribute |> Contains text

