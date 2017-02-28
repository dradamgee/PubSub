namespace Service

open System.Timers
open System
open System.Collections.Generic
open DomainModel
open Notifications
open Diagnostics
    
type MarketPlacementActorMessage =
    | SubscribeMessage of IObserver<DataChange<int, MarketPlacement>>
    | UnsubscribeMessage of IObserver<DataChange<int, MarketPlacement>>
    | PlaceMessage of MarketPlacement
    | FillMessage of FillExecution

type MarketPlacementActor(_placement: MarketPlacement) = 
    let subscribers = new HashSet<IObserver<DataChange<int, MarketPlacement>>>()
    let mutable placement = _placement
    
    let messageProcessor = MailboxProcessor.Start(fun inbox ->
        let rec loop() = 
            async { 
                let! msg = inbox.Receive()
                InboxWatcher.Watch(inbox)
                match msg with
                | SubscribeMessage sub -> 
                    do subscribers.Add(sub) |> ignore
                    sub.OnNext(DataChange(placement.ID, placement, DataChangeType.Add))
                | UnsubscribeMessage sub -> 
                    do subscribers.Remove(sub) |> ignore                
                | FillMessage fill ->                      
                    placement <- placement.Fill(fill)                    
                    for sub in subscribers do sub.OnNext(DataChange(placement.ID, placement, DataChangeType.Update))
                do! loop()
            }
        loop()
    )

    member this.Subscribe(sub) = messageProcessor.Post(SubscribeMessage(sub))
    member this.Unsubscribe(sub) = messageProcessor.Post(UnsubscribeMessage(sub))
    member this.Place(placement) = messageProcessor.Post(PlaceMessage(placement))
    member this.ProcessFill(fill) = messageProcessor.Post(FillMessage(fill))


type MarketPlacementSupervisor() =
    let subscribers = new HashSet<IObserver<DataChange<int, MarketPlacement>>>()
    let placementActors = new System.Collections.Generic.Dictionary<int, MarketPlacementActor>()
    
    let messageProcessor = MailboxProcessor.Start(fun inbox ->
        let rec loop() = 
            async { 
                let! msg = inbox.Receive()
                InboxWatcher.Watch(inbox)
                match msg with
                | SubscribeMessage sub -> 
                    for placementActor in placementActors.Values do placementActor.Subscribe(sub)                    
                    subscribers.Add(sub) |> ignore
                | UnsubscribeMessage sub -> 
                    for placementActor in placementActors.Values do placementActor.Unsubscribe(sub)                    
                | PlaceMessage mp ->                    
                    let mpa = new MarketPlacementActor(mp)
                    placementActors.Add(mp.ID, mpa)
                    for sub in subscribers do mpa.Subscribe(sub)
                | FillMessage fill ->                      
                    let placementActor = placementActors.[fill.PlacementID]
                    do placementActor.ProcessFill(fill)
                    
                do! loop()
            }
        loop()
    )

    member this.Subscribe(sub) = messageProcessor.Post(SubscribeMessage(sub))
    member this.Unsubscribe(sub) = messageProcessor.Post(UnsubscribeMessage(sub))
    member this.Place(placement) = messageProcessor.Post(PlaceMessage(placement))
    member this.ProcessFill(fill) = messageProcessor.Post(FillMessage(fill))



   