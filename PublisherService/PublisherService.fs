namespace PublisherService

open System
open System.ServiceModel
open Notifications
open DomainModel
open Simulator

type CallbackObserver(clientSubscriber: IClientSubscriber) =    
    
    interface IObserver<Notifications.DataChange<int, MarketPlacement>> with
        member this.OnCompleted() = 0 |> ignore
        member this.OnError(e) = raise(e) |> ignore
        member this.OnNext(dc: Notifications.DataChange<int, MarketPlacement>) = 
            let key = dc.Key
            let serializedData = Serializer.Serialize(dc.Data)
            let dataChangeType = dc.DataChangeType            
            clientSubscriber.OnNext([|serializedData|], dataChangeType) |> ignore

type PublisherService() = 
    let mmpp = MockMarketPlacementProvider(1.0)
    do mmpp.MockUpSomeStuff(66, 100) |> ignore
    do mmpp.StartFilling()
    
    let callback() = OperationContext.Current.GetCallbackChannel<IClientSubscriber>()

    interface IPublisherService with
        member this.SubscribeToMarketPlacements(deskId: int) = 
            let callbackObserver = CallbackObserver(callback())
            do (mmpp :> IObservable<Notifications.DataChange<int, MarketPlacement>>).Subscribe(callbackObserver) |> ignore
                