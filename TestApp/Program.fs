// Learn more about F# at http://fsharp.org

open System
open GussGame

[<EntryPoint>]
let main argv =
    Console.Title <- "Hallo Welt"
    let first = RndNumber 10
    let second = RndNumber 20
    printfn "Text %i" first
    printfn "Text %i" second
    printfn "Hello World from F#!"
    0 // return an integer exit code
