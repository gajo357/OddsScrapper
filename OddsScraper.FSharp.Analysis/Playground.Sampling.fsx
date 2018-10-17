module Playground.Sampling

let shuffleG xs = xs |> Seq.sortBy (fun _ -> System.Guid.NewGuid())
let takePercent percent xs =
    let nElem = System.Convert.ToInt32(float (xs |> Seq.length) * percent)
    xs |> shuffleG |> Seq.take nElem

let shuffleSample nSamples = shuffleG >> Seq.take nSamples

let random = System.Random()
let simpleRandIndices nSamples inputSize =
    random
    |> Seq.unfold (fun r -> Some(r.Next(inputSize), r))
    |> Seq.distinct
    |> Seq.take nSamples
    |> Set.ofSeq

let simpleSample nSamples data =
    let indices = simpleRandIndices nSamples (data |> Seq.length)
    data 
    |> Seq.mapi(fun d i -> 
        if Set.contains i indices then Some d else None)
    |> Seq.choose id
