namespace OddsScraper.FSharp.CommonScraping

module NodeExtensions =
    open OpenQA.Selenium
    open OddsScraper.FSharp.Common.Common
    
    let GetText (node:IWebElement) =
        node.Text.Trim()

    let GetElements name (node:IWebElement) =
        node.FindElements(By.TagName(name))

    let GetElementsByClassName name (node:IWebElement) =
        node.FindElements(By.ClassName(name))

    let GetAttribute attribute (node:IWebElement) = node.GetAttribute(attribute)

    let GetHref node = GetAttribute "href" node

    let GetClassAtttribute node = GetAttribute "class" node

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
        GetClassAtttribute node = text

    let ClassAttributeContains text node =
        node|> GetClassAtttribute |> Contains text

