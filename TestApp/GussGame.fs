module GussGame

open System


let Rnd = Random()

let RndNumber min = 
    let inst = Rnd
    let max = 50
    inst.Next(min, max)