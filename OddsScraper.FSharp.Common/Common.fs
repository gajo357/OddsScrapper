namespace OddsScraper.FSharp.Common

module Common =
    open System.Text.RegularExpressions
    open System

    let IntegerInString input =
        let m = Regex.Match(input, "\\d+") 
        if (m.Success) then Some (m.Groups.[0].Value |> int) else None
    
    let Remove oldValue (input:string) = 
        input.Replace(oldValue, System.String.Empty)

    let RemoveSeq input valuesToRemove =
        valuesToRemove |> Seq.fold (fun acc year -> Remove year acc) input
        
    let Split (separator:string) (input:string) = 
        input.Split([|separator|], System.StringSplitOptions.RemoveEmptyEntries)
        
    let Join separator (input:string[]) = 
        System.String.Join(separator, input)
    
    let JoinList separator (array: Object list) = 
        String.Join(separator, array)

    let joinCsv = Join ","

    let WriteToConsole (input:String) = 
        Console.WriteLine(input)

    let GetUserInput (message:String) =
        WriteToConsole message
        Console.ReadLine()
    let GetUserInputAsInt = GetUserInput >> IntegerInString
        

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
                use timer = new System.Timers.Timer()
                timer.Interval <- 5000.
                try
                    timer.Elapsed.Add(fun _ -> raise(Exception("")))
                    timer.Start()
                    let result = actionToRepeat()
                    timer.Stop()

                    Some result
                with
                | _ ->
                    timer.Stop()
                    repeatedAction (timesTried + 1) actionToRepeat
            else 
                None

        repeatedAction 0 actionToRepeat
        
    let getUsernameAndPassword() =
        (GetUserInput("Enter username: "), GetUserInput("Enter password: "))

