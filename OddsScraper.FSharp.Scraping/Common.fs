namespace OddsScraper.FSharp.Scraping

module Common =
    open System.Text.RegularExpressions

    let IntegerInString input =
        let m = Regex.Match(input, "\\d+") 
        if (m.Success) then Some (m.Groups.[0].Value |> int) else None

    let RemoveFromString whatToRemove (input:string) = 
        input.Replace(whatToRemove, System.String.Empty)
        
    let SplitString (split:string) (input:string) = 
        input.Split([|split|], System.StringSplitOptions.RemoveEmptyEntries)

    let TryParseWith tryParseFunc =
        tryParseFunc >> function
        | true, value -> Some value
        | false, _ -> None
    
    let TryParseDouble = TryParseWith System.Double.TryParse

    let IsNonEmptyString input =
        System.String.IsNullOrEmpty(input) = false

