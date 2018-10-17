module Playground.MonteCarlo

#load "Playground.Sampling.fsx"
open Playground.Sampling

let monteCarlo 
    (sample: int -> 'a seq -> 'a seq) 
    (simulate: 'a seq -> seq<'b*float>) nRuns nSamples data =
    Seq.init nRuns (fun _ -> sample nSamples data)
    |> Seq.map simulate

let simpleMonteCarlo simulate nRuns nSamples data = 
    monteCarlo simpleSample simulate nRuns nSamples data
let shuffleMonteCarlo simulate nRuns nSamples data = 
    monteCarlo shuffleSample simulate nRuns nSamples data

let seqiToFloat input = input |> Seq.map(fun i -> (i, float i))

simpleMonteCarlo seqiToFloat 5 10 [1..1000] |> Seq.toList
shuffleMonteCarlo seqiToFloat 5 10 [1..1000] |> Seq.toList