namespace SharpinoCounter
open System
open FsToolkit.ErrorHandling
open Sharpino.CommandHandler

open Sharpino.Storage
open Sharpino.Core
open SharpinoCounter.CounterContext
open SharpinoCounter.CounterContextEvents
open SharpinoCounter.CounterContextCommands
open SharpinoCounter.Counter

module SharpinoCounterApi =

    type SharpinoCounterApi (storage: IEventStore<string>, eventBroker: IEventBroker<string>, counterContextStateViewer: StateViewer<CounterContext>, counterViewer: AggregateViewer<Counter>) =

        member this.AddCounter (counterReference: CounterReference) =
            let counter = Counter (counterReference.CounterId, 0)
            result {
                return!  
                    AddCounterReference counterReference
                    |> this.RunInitAndCommand counter 
            }

        member this.GetCounter (counterId: System.Guid) =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                let! counterExists =
                    counterContext.GetCounterReference counterId
                let! (_, counter) = counterViewer counterId
                return counter
            }
            
        member this.GetCounter (counterName: string) =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                let! counterReference =
                    counterContext.GetCounterReference counterName
                let! (_, counter) = counterViewer counterReference.CounterId
                return counter
            }
            
        member this.GetCounterReferences () =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                return counterContext.CountersReferences.GetAll()
            }

        member this.Increment (counterId: System.Guid) =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                let! counterReference =
                    counterContext.GetCounterReference counterId
                return!
                    Increment
                    |> runAggregateCommand<Counter, CounterEvents, string> counterId storage eventBroker
            }    

        member this.Decrement (counterId: System.Guid) =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                let! counterReference =
                    counterContext.GetCounterReference counterId
                
                return!
                    Decrement 
                    |> runAggregateCommand<Counter, CounterEvents, string> counterId storage eventBroker
            }

        member this.ClearCounter (counterId: Guid) =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                let! counterReference =
                    counterContext.GetCounterReference counterId
                return!
                    Clear Unit
                    |> runAggregateCommand<Counter, CounterEvents, string> counterId storage eventBroker
            }

        member this.ClearCounter (counterId: Guid,  x) =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                let! counterReference =
                    counterContext.GetCounterReference counterId
                return!
                    Clear (Int x)
                    |> runAggregateCommand<Counter, CounterEvents, string> counterId storage eventBroker
            }        
            
        member this.RemoveCounter (id: Guid) =
            result {
                let! (_, counter) = counterViewer id
                let deactivated = Deactivate |> runAggregateCommand<Counter, CounterEvents, string> id storage eventBroker
                return! 
                    RemoveCounterReference id
                    |> runCommand<CounterContext, CounterCountextEvents, string> storage eventBroker
            }

        member private this.RunInitAndCommand counter cmd =
            cmd
            |> runInitAndCommand<CounterContext, CounterCountextEvents, Counter, string> storage eventBroker counter

        member private this.RunAggregateCommand counterId cmd =
            cmd
            |> runAggregateCommand<Counter, CounterEvents, string> counterId storage eventBroker 

        member private this.RunCounterContextCommand cmd =
            cmd 
            |> runCommand<CounterContext, CounterCountextEvents, string> storage eventBroker