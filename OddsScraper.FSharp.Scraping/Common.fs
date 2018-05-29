namespace OddsScraper.FSharp.Scraping

module Common =
    open System.Text.RegularExpressions
    open System
    open System.Numerics

    let IntegerInString input =
        let m = Regex.Match(input, "\\d+") 
        if (m.Success) then Some (m.Groups.[0].Value |> int) else None

    let Remove oldValue (input:string) = 
        input.Replace(oldValue, System.String.Empty)
        
    let Split (separator:string) (input:string) = 
        input.Split([|separator|], System.StringSplitOptions.RemoveEmptyEntries)
        
    let Join (separator:string) (input:string[]) = 
        System.String.Join(separator, input)
        
    let Contains value (input:string) = 
        input.Contains(value)

    let StartsWith value (input:string) = 
        input.StartsWith(value)

    let TryParseWith tryParseFunc =
        tryParseFunc >> function
        | true, value -> Some value
        | false, _ -> None
    
    let TryParseDateTime = TryParseWith System.DateTime.TryParse
    let TryParseDouble = TryParseWith System.Double.TryParse

    let ConvertOptionToNullable = function
        | None -> new System.Nullable<_>()
        | Some value -> new System.Nullable<_>(value)

    let IsNonEmptyString input =
        System.String.IsNullOrEmpty(input) = false

    let TryAndForget action =
        try
            action()
        with
        | _ -> ignore

    let InvokeRepeatedIfFailed actionToRepeat =
        let rec repeatedAction timesTried actionToRepeat =
            if timesTried < 2 then
                try
                    Some (actionToRepeat())
                with
                | _ -> repeatedAction (timesTried + 1) actionToRepeat
            else 
                None

        repeatedAction 0 actionToRepeat

