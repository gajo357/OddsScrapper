namespace OddsScraper.FSharp.Scraping

module Common =
    open System.Text.RegularExpressions

    let (|FirstRegexGroup|_|) pattern input =
       let m = Regex.Match(input,pattern) 
       if (m.Success) then Some m.Groups.[0].Value else None
       
    let IntegerInString input =
        let m = Regex.Match(input, "\\d+") 
        if (m.Success) then Some (m.Groups.[0].Value |> int) else None

