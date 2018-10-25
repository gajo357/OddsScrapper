module Playground.Sampling

let shuffleG xs = xs |> Seq.sortBy (fun _ -> System.Guid.NewGuid())
let takePercent percent xs =
    let nElem = System.Convert.ToInt32(float (xs |> Seq.length) * percent)
    xs |> shuffleG |> Seq.take nElem

let shuffleSample nSamples = shuffleG >> Seq.truncate nSamples

let random = System.Random()
let simpleRandIndices nSamples inputSize =
    [1..inputSize]
    |> shuffleG
    |> Seq.truncate nSamples
    |> Set.ofSeq

let simpleSample nSamples data =
    let indices = simpleRandIndices nSamples (data |> Seq.length)
    data 
    |> Seq.mapi(fun i d -> 
        if Set.contains i indices then Some d else None)
    |> Seq.choose id
