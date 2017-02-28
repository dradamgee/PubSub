namespace Service

open System.Timers
open System
open System.Collections.Generic
open DomainModel
open Notifications
open Diagnostics
    
type MarketPlacementActorMessage =
    | MarketPlacementSubscribeMessage of IObserver<DataChange<int, MarketPlacement>>
    | MarketPlacementUnsubscribeMessage of IObserver<DataChange<int, MarketPlacement>>
    | FillExecutionSubscribeMessage of IObserver<FillExecution>
    | FillExecutionUnsubscribeMessage of IObserver<FillExecution>
    | PlaceMessage of MarketPlacement
    | FillMessage of FillExecution

type MarketPlacementActor(_placement: MarketPlacement) = 
    let marketPlacementSubscribers = new HashSet<IObserver<DataChange<int, MarketPlacement>>>()
    let fillExecutionSubscribers = new HashSet<IObserver<FillExecution>>()
    let fills = new ResizeArray<FillExecution>()
    let mutable placement = _placement
    
    let messageProcessor = MailboxProcessor.Start(fun inbox ->
        let rec loop() = 
            async { 
                let! msg = inbox.Receive()
                InboxWatcher.Watch(inbox)
                match msg with
                | MarketPlacementSubscribeMessage sub -> 
                    do marketPlacementSubscribers.Add(sub) |> ignore
                    sub.OnNext(DataChange(placement.ID, placement, DataChangeType.Add))
                | MarketPlacementUnsubscribeMessage sub -> 
                    do marketPlacementSubscribers.Remove(sub) |> ignore                
                | FillExecutionSubscribeMessage sub -> 
                    do fillExecutionSubscribers.Add(sub) |> ignore
                    for fillExecution in fills
                        do sub.OnNext(fillExecution)
                | FillExecutionUnsubscribeMessage sub -> 
                    do fillExecutionSubscribers.Remove(sub) |> ignore                
                | FillMessage fill ->                     
                    fills.Add(fill)
                    placement <- placement.Fill(fill)                    
                    for sub in marketPlacementSubscribers do sub.OnNext(DataChange(placement.ID, placement, DataChangeType.Update))
                    for sub in fillExecutionSubscribers do sub.OnNext(fill)
                do! loop()
            }
        loop()
    )

    member this.Subscribe(sub) = messageProcessor.Post(MarketPlacementSubscribeMessage(sub))
    member this.Unsubscribe(sub) = messageProcessor.Post(MarketPlacementUnsubscribeMessage(sub))
    member this.Subscribe(sub) = messageProcessor.Post(FillExecutionSubscribeMessage(sub))
    member this.Unsubscribe(sub) = messageProcessor.Post(FillExecutionUnsubscribeMessage(sub))
    member this.Place(placement) = messageProcessor.Post(PlaceMessage(placement))
    member this.ProcessFill(fill) = messageProcessor.Post(FillMessage(fill))


type MarketPlacementSupervisor() =
    let subscribers = new HashSet<IObserver<DataChange<int, MarketPlacement>>>()
    let placementActors = new System.Collections.Generic.Dictionary<int, MarketPlacementActor>()
    let fillSubscribers = new HashSet<IObserver<FillExecution>>()
    let fills = new ResizeArray<FillExecution>()
    
    let messageProcessor = MailboxProcessor.Start(fun inbox ->
        let rec loop() = 
            async { 
                let! msg = inbox.Receive()
                InboxWatcher.Watch(inbox)
                match msg with
                | MarketPlacementSubscribeMessage sub -> 
                    for placementActor in placementActors.Values do placementActor.Subscribe(sub)                    
                    subscribers.Add(sub) |> ignore
                | MarketPlacementUnsubscribeMessage sub -> 
                    for placementActor in placementActors.Values do placementActor.Unsubscribe(sub)                    
                | FillExecutionSubscribeMessage sub ->                    
                    fillSubscribers.Add(sub) |> ignore
                    for fill in fills do sub.OnNext(fill)
                | FillExecutionUnsubscribeMessage sub ->                    
                    fillSubscribers.Remove(sub) |> ignore                    
                | PlaceMessage mp ->                    
                    let mpa = new MarketPlacementActor(mp)
                    placementActors.Add(mp.ID, mpa)
                    for sub in subscribers do mpa.Subscribe(sub)
                | FillMessage fill ->                      
                    let placementActor = placementActors.[fill.PlacementID]
                    do placementActor.ProcessFill(fill)
                    for sub in fillSubscribers do sub.OnNext(fill)                    
                do! loop()
            }
        loop()
    )

    member this.Subscribe(sub) = messageProcessor.Post(MarketPlacementSubscribeMessage(sub))
    member this.Unsubscribe(sub) = messageProcessor.Post(MarketPlacementUnsubscribeMessage(sub))
    member this.Subscribe(sub) = messageProcessor.Post(FillExecutionSubscribeMessage(sub))
    member this.Unsubscribe(sub) = messageProcessor.Post(FillExecutionUnsubscribeMessage(sub))
    member this.Place(placement) = messageProcessor.Post(PlaceMessage(placement))
    member this.ProcessFill(fill) = messageProcessor.Post(FillMessage(fill))



   