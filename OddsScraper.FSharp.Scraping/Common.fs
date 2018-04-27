namespace OddsScraper.FSharp.Scraping

module Common =
    open System.Text.RegularExpressions

    let (|FirstRegexGroup|_|) pattern input =
       let m = Regex.Match(input,pattern) 
       if (m.Success) then Some m.Groups.[0].Value else None
       
    let IntegerInString input =
        let m = Regex.Match(input, "\\d+") 
        if (m.Success) then Some (m.Groups.[0].Value |> int) else None

    let TryParseWith tryParseFunc =
        tryParseFunc >> function
        | true, value -> Some value
        | false, _ -> None
    
    let TryParseDouble = TryParseWith System.Double.TryParse

    let IsNonEmptyString input =
        System.String.IsNullOrEmpty(input) = false

