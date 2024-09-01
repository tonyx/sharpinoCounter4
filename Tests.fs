module Tests

open System
open Sharpino
open FSharpPlus
open FsToolkit.ErrorHandling
open Expecto
open Sharpino.Repositories
open Sharpino.CommandHandler
open SharpinoCounter
open SharpinoCounter.Counter
open SharpinoCounter.CounterContext
open SharpinoCounter.SharpinoCounterApi
open Sharpino.TestUtils
open TestUtils

[<Tests>]
let tests =

    // make sure you properly setup postgres db using dbmate tool if you want to enable the first test line below
    let testConfigs = [
        // ((fun () -> SharpinoCounterApi (pgStorage, doNothingBroker, counterContextStorageStateViewer, counterAggregateStorageStateViewer)), pgStorage)
        ((fun () -> SharpinoCounterApi (inMemoryEventStore, doNothingBroker, counterContextMemoryStateViewer, counterAggregateMemoryStateViewer)), inMemoryEventStore)
    ]

    testList "samples" [
        multipleTestCase "add a counter reference  - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let counterReference: CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
            let counterApi = api ()

            // when
            let counterReferences = counterApi.AddCounter counterReference

            // then
            Expect.isOk counterReferences  "should be ok"

        multipleTestCase "add a counter reference and retrieve it - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let counterReference: CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
            let counterApi = api ()

            // when
            let _ = counterApi.AddCounter counterReference
            let counterReferences = counterApi.GetCounterReferences ()

            // then
            Expect.isOk counterReferences  "should be ok"
            let okValue = counterReferences.OkValue 
            
            Expect.equal okValue [counterReference] "should be empty"

        multipleTestCase "add a counter and retrieve it - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let counterReference: CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
            let counterApi = api ()
            let _ = counterApi.AddCounter counterReference

            // when
            let counterRetrieved = counterApi.GetCounter counterReference.CounterId

            // then
            Expect.isOk counterRetrieved  "should be ok"
            let counter = counterRetrieved.OkValue
            Expect.equal counter.Id counterReference.CounterId "should be equal"
            
        multipleTestCase "Add a counter and retrieve it by its name - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let counterReference: CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
            let counterApi = api ()
            let _ = counterApi.AddCounter counterReference

            // when
            let counterRetrieved = counterApi.GetCounter counterReference.CounterName

            // then
            Expect.isOk counterRetrieved "should be ok"
            let counter = counterRetrieved.OkValue
            Expect.equal counter.Id counterReference.CounterId "should be equal"        

        multipleTestCase "add a counter and increment it - Ok" testConfigs  <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let counterReference: CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid () 
                }
            let counterApi = api ()
            let _ = counterApi.AddCounter counterReference

            // when
            let incrementCounter = counterApi.Increment counterReference.CounterId

            // then
            let counterRetrieved = counterApi.GetCounter counterReference.CounterId
            Expect.isOk counterRetrieved "should be ok"
            let counter = counterRetrieved.OkValue
            Expect.equal counter.State 1 "should be 1"

        multipleTestCase "add a counter, increment it twice - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let counterReference:CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
            let counterApi = api ()
            let _ = counterApi.AddCounter counterReference

            // when
            let incrementCounter = counterApi.Increment counterReference.CounterId
            let incrementCounterAgain = counterApi.Increment counterReference.CounterId 

            // then

            let counterRetrieved = counterApi.GetCounter counterReference.CounterId
            Expect.isOk counterRetrieved "should be ok"
            let counter = counterRetrieved.OkValue
            Expect.equal counter.State 2 "should be 1"

        multipleTestCase "add a counter, increment it twice and then decrement it - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let counterReference: CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
            let counterApi = api ()
            let _ = counterApi.AddCounter counterReference

            // when
            let incrementCounter = counterApi.Increment (counterReference.CounterId)
            let incrementCounterAgain = counterApi.Increment counterReference.CounterId
            let decrementCounter = counterApi.Decrement counterReference.CounterId

            // then

            let counterRetrieved = counterApi.GetCounter counterReference.CounterId
            Expect.isOk counterRetrieved "should be ok"
            let counter = counterRetrieved.OkValue
            Expect.equal counter.State 1 "should be 1"

        multipleTestCase "retrieve an unexisting counter - Error " testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()

            // when
            let counterRetrieved = counterApi.GetCounter newCounterId

            // then
            Expect.isError counterRetrieved "should be error"

        multipleTestCase "cannot add two counters with the same name - Error" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let counterReference: CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
            let newCounterReference: CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
            let counterApi = api ()
            let _ = counterApi.AddCounter counterReference

            // when
            let readded = counterApi.AddCounter newCounterReference

            // then
            Expect.isError readded "should be error"
        
        multipleTestCase "add two counters - Ok" testConfigs <| fun (api, eventStore) -> 
            Setup eventStore
            // given
            let counterReference: CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
            let newCounterId2: CounterReference =
                {
                    CounterName = "counter2"
                    CounterId = Guid.NewGuid ()
                }
            let counterApi = api ()
            let _ = counterApi.AddCounter counterReference
            let _ = counterApi.AddCounter newCounterId2

            // when
            let counterReferences = counterApi.GetCounterReferences ()

            // then
            Expect.isOk counterReferences  "should be ok"
            let okValue = counterReferences.OkValue
            Expect.equal (okValue |> Set.ofList) ([counterReference; newCounterId2] |> Set.ofList) "should be empty"

        multipleTestCase "add two counters and increment them independently - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let counterReference: CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
           
            let newCounterId2: CounterReference =
                {
                    CounterName = "counter2"
                    CounterId = Guid.NewGuid ()
                } 
                
            // let newCounterId = Guid.NewGuid ()
            // let newCounterId2 = Guid.NewGuid ()
            let counterApi = api ()
            let _ = counterApi.AddCounter counterReference
            let _ = counterApi.AddCounter newCounterId2

            // when
            let incrementCounter = counterApi.Increment counterReference.CounterId
            let incrementCounter2 = counterApi.Increment newCounterId2.CounterId 

            // then

            let counterRetrieved = counterApi.GetCounter counterReference.CounterId
            Expect.isOk counterRetrieved "should be ok"
            let counter = counterRetrieved.OkValue
            Expect.equal counter.State 1 "should be 1"

            let counterRetrieved2 = counterApi.GetCounter newCounterId2.CounterId
            Expect.isOk counterRetrieved2 "should be ok"
            let counter2 = counterRetrieved2.OkValue
            Expect.equal counter2.State 1 "should be 1"

        multipleTestCase "add five counters, increment/decrement them - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore

            // let ids = [Guid.NewGuid (); Guid.NewGuid (); Guid.NewGuid (); Guid.NewGuid (); Guid.NewGuid ()]
            // when
            let ids =
                [
                    {
                        CounterName = "counter0"
                        CounterId = Guid.NewGuid ()
                    }
                    {
                        CounterName = "counter1"
                        CounterId = Guid.NewGuid ()
                    }
                    {
                        CounterName = "counter2"
                        CounterId = Guid.NewGuid ()
                    }
                    {
                        CounterName = "counter3"
                        CounterId = Guid.NewGuid ()
                    }
                    {
                        CounterName = "counter4"
                        CounterId = Guid.NewGuid ()
                    }
                ]
            let counterApi = api ()

            let _ = ids |> List.map (fun id -> counterApi.AddCounter id)

            // twice
            [1 .. 2] 
            |>> (fun _ -> counterApi.Increment (List.item 0 (ids |> List.map (fun x -> x.CounterId))) |> ignore)
            [1 .. 2]
            |> ignore

            let incrementFirst =
                counterApi.Increment (List.item 1 (ids |> List.map (fun x -> x.CounterId))) 
            Expect.isOk incrementFirst "should be ok"

            let incrementSecond =
                counterApi.Increment (List.item 2 (ids |> List.map (fun x -> x.CounterId)))
            Expect.isOk incrementSecond  "should be ok"

            let incrementThreeTimes =
                [1 .. 3]
                |> List.traverseResultM (fun _ -> counterApi.Increment (List.item 3 (ids |> List.map (fun x -> x.CounterId))))
            Expect.isOk incrementThreeTimes "should be ok"

            let incrementTenTimes =
                [1 .. 10]
                |> List.traverseResultM (fun _ -> counterApi.Increment (List.item 4 (ids |> List.map (fun x -> x.CounterId))))

            Expect.isOk incrementTenTimes "should be ok"

            // then
            
            let counter0 = counterApi.GetCounter (List.item 0 ids).CounterId
            let counter1 = counterApi.GetCounter (List.item 1 ids).CounterId
            let counter2 = counterApi.GetCounter (List.item 2 ids).CounterId
            let counter3 = counterApi.GetCounter (List.item 3 ids).CounterId
            let counter4 = counterApi.GetCounter (List.item 4 ids).CounterId

            Expect.isOk counter0 "should be ok"
            Expect.isOk counter1 "should be ok"
            Expect.isOk counter2 "should be ok"
            Expect.isOk counter3 "should be ok"
            Expect.isOk counter4 "should be ok"

            Expect.equal counter0.OkValue.State 2 "should be 2"
            Expect.equal counter1.OkValue.State 1 "should be 1"
            Expect.equal counter2.OkValue.State 1 "should be 1"
            Expect.equal counter3.OkValue.State 3 "should be 3"
            Expect.equal counter4.OkValue.State 10 "should be 10"

        multipleTestCase "add and remove a counter, the counter should become unactive - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            let counterStateViewer = getAggregateStorageFreshStateViewer<Counter, CounterEvents, string> eventStore
            // given
            
            let newCounterId =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
              
            let counterApi = api ()
            let _ = counterApi.AddCounter newCounterId
            let retrieved = counterApi.GetCounterReferences ()
            Expect.isOk retrieved "should be ok"
            Expect.equal retrieved.OkValue [newCounterId] "should be equal"

            // when
            let removeCounter = counterApi.RemoveCounter newCounterId.CounterId
            // 
            Expect.isOk removeCounter "should be ok"
            let counters = counterApi.GetCounterReferences ()
            Expect.isOk counters "should be ok"
            Expect.equal counters.OkValue [] "should be empty"
            
            let counter = counterApi.GetCounter newCounterId.CounterId
            Expect.isError counter "should be error"
            
            let counter2 = counterStateViewer newCounterId.CounterId
            let (_, counter2') = counter2.OkValue
            Expect.isOk counter2 "should be ok"
            Expect.isFalse counter2'.IsActive "should be false"

        multipleTestCase "add twice the counter a counter with the same id - Error" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId: CounterReference =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
            let counterApi = api ()
            let added = counterApi.AddCounter newCounterId
            Expect.isOk added "should be ok"

            // when
            let readded = counterApi.AddCounter newCounterId

            // then
            Expect.isError readded "should be error"
            let (Error e)  = readded
            printf "%A" e   


        multipleTestCase "add a counter, remove it, and readd it - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore

            // given
            let newCounterId =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
                
            let counterApi = api ()
            let added = counterApi.AddCounter newCounterId
            Expect.isOk added "should be ok"
            let removed = counterApi.RemoveCounter newCounterId.CounterId
            Expect.isOk removed "should be ok"

            // when
            let readded = counterApi.AddCounter newCounterId

            // then
            Expect.isOk readded "should be ok"
            let retrieved = counterApi.GetCounterReferences ()
            Expect.isOk retrieved "should be ok"
            Expect.equal retrieved.OkValue [newCounterId] "should be equal"

        multipleTestCase "add a counter, increase it and then clear it to zero - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore

            // given
            let newCounterId =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
            let counterApi = api ()
            let added = counterApi.AddCounter newCounterId
            Expect.isOk added "should be ok"
            let incremented = counterApi.Increment newCounterId.CounterId
            Expect.isOk incremented "should be ok"

            // when
            let reset = counterApi.ClearCounter newCounterId.CounterId

            // then
            Expect.isOk reset "should be ok"
            let retrieved = counterApi.GetCounter newCounterId.CounterId
            Expect.isOk retrieved "should be ok"
            Expect.equal retrieved.OkValue.State 0 "should be equal"

        multipleTestCase "add a counter, increase it and then clear it to a value - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore

            // given
            let newCounterId =
                {
                    CounterName = "counter"
                    CounterId = Guid.NewGuid ()
                }
                
            let counterApi = api ()
            let added = counterApi.AddCounter newCounterId
            Expect.isOk added "should be ok"
            let incremented = counterApi.Increment newCounterId.CounterId
            Expect.isOk incremented "should be ok"

            // when
            let reset = counterApi.ClearCounter (newCounterId.CounterId, 10)

            // then
            Expect.isOk reset "should be ok"
            let retrieved = counterApi.GetCounter newCounterId.CounterId
            Expect.isOk retrieved "should be ok"
            Expect.equal retrieved.OkValue.State 10 "should be equal"
    ] 
    |> testSequenced
