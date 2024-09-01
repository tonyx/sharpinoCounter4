
namespace SharpinoCounter
open Sharpino.Lib.Core.Commons
open Sharpino.Repositories
open SharpinoCounter.Commons
open System
open Sharpino
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils

module CounterContext =
    
    type CounterReference =
        {
            CounterName: string
            CounterId: Guid
        }
        interface Entity with
            member this.Id = this.CounterId
    
    type CounterContext(state: int, counterRefs: List<CounterReference>) =

        member this.CountersReferences = ListRepository<CounterReference>.Create counterRefs

        member this.AddCounterReference (counterReference: CounterReference) = 
            result {
                do!
                    counterRefs
                    |> List.exists (fun (x: CounterReference) -> x.CounterId = counterReference.CounterId || x.CounterName = counterReference.CounterName)
                    |> not
                    |> Result.ofBool "counter already exists"
                return CounterContext (state, counterRefs @ [counterReference]) 
            }
        member this.RemoveCounterReference (counterReference: CounterReference) =
            result {
                do! 
                    counterRefs |> List.exists (fun x -> x = counterReference)
                    |> Result.ofBool "counter does not exist"
                return CounterContext (state, counterRefs |> List.filter (fun x -> x <> counterReference))
            }
         
        member this.GetCounterReference (id: Guid) =
            result {
                let! result =
                    counterRefs |> List.tryFind (fun x -> x.CounterId = id)
                    |> Result.ofOption "counter not found"
                return result     
            }
        
        member this.GetCounterReference (name: string) =
            result {
                let! result =
                    counterRefs |> List.tryFind (fun x -> x.CounterName = name)
                    |> Result.ofOption "counter not found"
                return result    
            }
        
        member this.State = state

        static member Zero = CounterContext (0, []) 
        static member StorageName = "_countercontext"
        static member Version = "_01"
        static member SnapshotsInterval = 15
        static member Deserialize (json: Json) =
            globalSerializer.Deserialize<CounterContext> json

        member this.Serialize =
            globalSerializer.Serialize this
