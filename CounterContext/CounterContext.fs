
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
    
    type CounterContext(state: int, counterRefs: IRepository<CounterReference>) =

        member this.CountersReferences = counterRefs

        member this.AddCounterReference (counterReference: CounterReference) = 
            result {
                let! newRepo =
                    this.CountersReferences.AddWithPredicate (counterReference, (fun (x: CounterReference) -> x.CounterName = counterReference.CounterName), "counter already exists")
                return CounterContext (state, newRepo) 
            }
       
        member this.RemoveCounterReference (id: Guid) =
            result {
                let! newRepo = this.CountersReferences.Remove id "counter does not exist"
                return CounterContext (state, newRepo)
            }     
         
        member this.GetCounterReference (id: Guid) =
            result {
                let! result =
                    this.CountersReferences.Get id
                    |> Result.ofOption "counter not found"
                return result     
            }
        
        member this.GetCounterReference (name: string) =
            result {
                let! result =
                    this.CountersReferences.Find (fun x -> x.CounterName = name)
                    |> Result.ofOption "counter not found"
                return result    
            }
        
        member this.State = state

        static member Zero = CounterContext (0, ListRepository.Zero) 
        static member StorageName = "_countercontext"
        static member Version = "_01"
        static member SnapshotsInterval = 15
        static member Deserialize (json: Json) =
            globalSerializer.Deserialize<CounterContext> json

        member this.Serialize =
            globalSerializer.Serialize this
