namespace PublisherService

open System
open System.ServiceModel
open Notifications
open DomainModel
open Simulator
open Service

type CallbackObserver(clientSubscriber: IClientSubscriber) =    
    
    interface IObserver<Notifications.DataChange<int, MarketPlacement>> with
        member this.OnCompleted() = 0 |> ignore
        member this.OnError(e) = raise(e) |> ignore
        member this.OnNext(dc: Notifications.DataChange<int, MarketPlacement>) = 
            let key = dc.Key
            let serializedData = Serializer.Serialize(dc.Data)
            let dataChangeType = dc.DataChangeType            
            clientSubscriber.OnNext([|serializedData|], dataChangeType) |> ignore

[<ServiceBehavior(InstanceContextMode  = InstanceContextMode.Single)>]
type PublisherService(marketPlacementActor: MarketPlacementActor) = 
    
    
    let callback() = OperationContext.Current.GetCallbackChannel<IClientSubscriber>()

    interface IPublisherService with
        member this.SubscribeToMarketPlacements(deskId: int) = 
            let callbackObserver = CallbackObserver(callback())
            do marketPlacementActor.Subscribe(callbackObserver) |> ignore
                