namespace Simulator

open System.Timers
open System
open System.Collections.Generic
open DomainModel
open Service
open Notifications
open Diagnostics

type SellSideSimActions = 
    | OnNext of DataChange<int, MarketPlacement>
    | Tick    

type SellSideSim(MarketPlacementSupervisor: MarketPlacementSupervisor) as self =
    

    let placements = Dictionary<int, MarketPlacement>()
    let random = Random()
    do MarketPlacementSupervisor.Subscribe(self)

    let messageProcessor = MailboxProcessor.Start(fun inbox ->
        let rec loop() = 
            async { 
                let! msg = inbox.Receive()                                                

                InboxWatcher.Watch(inbox)
                
                match msg with
                | OnNext value ->
                    match value.DataChangeType with
                    | DataChangeType.Add -> placements.Add(value.Key, value.Data) |> ignore
                    | DataChangeType.Delete -> placements.Remove(value.Key) |> ignore
                    | DataChangeType.Update -> 
                        do placements.[value.Key] <- value.Data
                | Tick ->                         
                    for placement in placements.Values do
                        let nextFillQty = 1m //decimal(random.Next 10)
                        FillExecution(placement.ID, nextFillQty, nextFillQty + placement.Filled)                        
                        |> MarketPlacementSupervisor.ProcessFill
                
                do! loop()
              }     
        loop()
    )

    interface IObserver<DataChange<int, MarketPlacement>> with
        member this.OnNext(value: DataChange<int, MarketPlacement>) = 
            messageProcessor.Post(OnNext(value))
        member this.OnError(error : Exception)  : unit = 
            raise (error)
        member this.OnCompleted() : unit = 
            raise (NotImplementedException())
    
    member this.Tick() = 
        messageProcessor.Post(Tick)

type DisposibleAction(action : Action) = 
    interface System.IDisposable with 
        member this.Dispose() = 
            action.Invoke()  

type MockMarketPlacementProvider(fillRate: float) = 
    let mutable id = 0
    let placementActor = MarketPlacementSupervisor()    

    let createPlacement(deskId: int, newId: int) =                
        let mp = MarketPlacement(newId, deskId, 1000m, 0m)
        placementActor.Place(mp)
    
    let rec createPlacements desk startFrom number =        
        if number = 0 then startFrom
        else 
            createPlacement(desk, startFrom)
            createPlacements desk (startFrom + 1) (number - 1)

    let sss = SellSideSim(placementActor)
    let timer = new Timer(fillRate)
    do timer.Elapsed.Add(fun _ -> sss.Tick())
      
    member this.PlacementActor with get() = placementActor
       
    member this.MockUpSomeStuff(deskId: int, count: int) =
        id = createPlacements deskId  id count
        

    member this.StartFilling() = 
        timer.Start()
    member this.StopFilling() = 
        timer.Stop()        
