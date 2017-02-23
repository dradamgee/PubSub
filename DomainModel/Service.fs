namespace Service

open System.Timers
open System
open System.Collections.Generic
open System.Diagnostics
open DomainModel
open Notifications
    
type MarketPlacementActorMessage =
    | SubscribeMessage of IObserver<DataChange<int, MarketPlacement>>
    | UnsubscribeMessage of IObserver<DataChange<int, MarketPlacement>>
    | PlaceMessage of MarketPlacement
    | FillMessage of FillExecution

type MarketPlacementActor() = 
    let subscribers = new HashSet<IObserver<DataChange<int, MarketPlacement>>>()
    let placements = new System.Collections.Generic.Dictionary<int, MarketPlacement>()
    
    let messageProcessor = MailboxProcessor.Start(fun inbox ->
        let rec loop() = 
            async { 
                let! msg = inbox.Receive()
                match msg with
                | SubscribeMessage sub -> 
                    do subscribers.Add(sub) |> ignore
                    for mp in placements do sub.OnNext(DataChange(mp.Key, mp.Value, DataChangeType.Add))
                | UnsubscribeMessage sub -> 
                    do subscribers.Remove(sub) |> ignore
                | PlaceMessage mp -> 
                    placements.Add(mp.ID, mp)
                    for sub in subscribers do sub.OnNext(DataChange(mp.ID, mp, DataChangeType.Add))
                | FillMessage fill -> 
                    //Debug.WriteLine(fill.ToString() + DateTime.Now.ToString())
                    let oldPlacement = placements.[fill.PlacementID]
                    let newPlacement = oldPlacement.Fill(fill)
                    placements.[fill.PlacementID] <- newPlacement
                    for sub in subscribers do sub.OnNext(DataChange(newPlacement.ID, newPlacement, DataChangeType.Update))
                do! loop()
            }
        loop()
    )

    member this.Subscribe(sub) = messageProcessor.Post(sub)
    member this.Unsubscribe(sub) = messageProcessor.Post(UnsubscribeMessage(sub))
    member this.Place(placement) = messageProcessor.Post(placement)
    member this.ProcessFill(fill) = messageProcessor.Post(fill)

        

   