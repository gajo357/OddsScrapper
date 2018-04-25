namespace OddsScraper.FSharp.Scraping

module NodeExtensions =
    open OpenQA.Selenium

    let GetElements name (node:IWebElement) =
        node.FindElements(By.TagName(name))

    let GetHref (node:IWebElement) =
        node.GetAttribute("href")

    let GetTableRows node = GetElements "tr" node

    let GetTdsFromRow node = GetElements "td" node

    let GetAllHrefElements node = GetElements "a" node

    let GetAllHrefFromElements node =
        GetAllHrefElements node
        |> Seq.map GetHref
        |> Seq.filter (fun href -> System.String.IsNullOrEmpty(href) = false)

    let GetClassAtttribute (node:IWebElement) =
        node.GetAttribute("class")

    let ClassAttributeEquals text node =
        GetClassAtttribute node = text

    let ClassAttributeContains text node =
        (GetClassAtttribute node).Contains text

