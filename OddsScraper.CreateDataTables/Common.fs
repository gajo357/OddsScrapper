namespace OddsScraper.FSharp.Scraping

module Common =
    open System
    open System.Text.RegularExpressions

    let Join separator (array: Object list) = 
        String.Join(separator, array)

    let WriteToConsole (input:String) = 
        Console.WriteLine(input)
    
    let IntegerInString input =
        let m = Regex.Match(input, "\\d+")
        if (m.Success) then Some (m.Groups.[0].Value |> int) else None