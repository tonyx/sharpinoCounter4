
namespace SharpinoCounter
open SharpinoCounter.Commons
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Sharpino.Core
open SharpinoCounter.CounterContext

module CounterContextEvents =

    type CounterCountextEvents =
        | CounterAdded of CounterReference
        | CounterRemoved of Guid
            interface Event<CounterContext> with
                member this.Process (counter: CounterContext) =
                    match this with
                    | CounterAdded id -> counter.AddCounterReference id
                    | CounterRemoved id -> counter.RemoveCounterReference id

// ---
        static member Deserialize (json: Json) =
            globalSerializer.Deserialize<CounterCountextEvents>(json)
        member this.Serialize  =
            this
            |> globalSerializer.Serialize

