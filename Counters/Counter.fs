
namespace SharpinoCounter
open SharpinoCounter.Commons
open System
open Sharpino
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils

module Counter =
    open Sharpino.Core
    type Counter private (id: Guid, value: int, active: bool) =
        new (id: Guid, value: int) = Counter (id, value, true)

        member this.Id = id
        member this.State = value
        member this.IsActive = active

        member this.Clear () = Counter (this.Id, 0, active) |> Ok
        member this.Clear value  = Counter (this.Id, value,  active) |> Ok
        
        member this.Deactivate () =
            Counter (this.Id, this.State, false) |> Ok
            
        member this.Activate () =
            Counter (this.Id, this.State, true) |> Ok
        
        member this.Increment () =
            result 
                {
                    do! 
                        this.State < 99
                        |> Result.ofBool "must be lower than 99"
                    return Counter (this.Id, this.State + 1, active)
                }

        member this.Decrement () =
            result
                {
                    do!
                        this.State > 0
                        |> Result.ofBool "must be greater than 0"
                    return Counter (this.Id, this.State - 1, active)
                }

        member this.Serialize  =
            this
            |> globalSerializer.Serialize
        static member Deserialize (json: Json) =
            json 
            |> globalSerializer.Deserialize<Counter>

        static member Version = "_01"
        static member StorageName = "_counter"
        static member SnapshotsInterval = 15

        interface Aggregate<string> with
            member this.Id = this.Id
            member this.Serialize  =
                this.Serialize 
