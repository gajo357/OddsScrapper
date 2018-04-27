namespace OddsScraper.FSharp.Scraping

module NodeExtensions =
    open OpenQA.Selenium
    open OddsScraper.FSharp.Scraping.Common

    let GetText (node:IWebElement) =
        node.Text.Trim()

    let GetElements name (node:IWebElement) =
        node.FindElements(By.TagName(name))

    let GetHref (node:IWebElement) =
        node.GetAttribute("href")

    let GetTableRows node = GetElements "tr" node

    let GetTdsFromRow node = GetElements "td" node

    let GetAllHrefElements node = GetElements "a" node

    let GetAllHrefTextAndAttribute node =
        GetAllHrefElements node
        |> Seq.map (fun n -> (GetHref n, n.Text))
        |> Seq.filter (fst >> IsNonEmptyString)

    let GetAllHrefFromElements node =
        GetAllHrefTextAndAttribute node
        |> Seq.map fst

    let GetClassAtttribute (node:IWebElement) =
        node.GetAttribute("class")

    let ClassAttributeEquals text node =
        GetClassAtttribute node = text

    let ClassAttributeContains text node =
        (GetClassAtttribute node).Contains text

