namespace OddsScraper.FSharp.Scraping

module Common =
    open System.Text.RegularExpressions

    let IntegerInString input =
        let m = Regex.Match(input, "\\d+") 
        if (m.Success) then Some (m.Groups.[0].Value |> int) else None

    let Remove oldValue (input:string) = 
        input.Replace(oldValue, System.String.Empty)
        
    let Split (separator:string) (input:string) = 
        input.Split([|separator|], System.StringSplitOptions.RemoveEmptyEntries)
        
    let Contains value (input:string) = 
        input.Contains(value)

    let TryParseWith tryParseFunc =
        tryParseFunc >> function
        | true, value -> Some value
        | false, _ -> None
    
    let TryParseDouble = TryParseWith System.Double.TryParse

    let IsNonEmptyString input =
        System.String.IsNullOrEmpty(input) = false

