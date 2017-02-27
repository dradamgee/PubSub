namespace PublisherService

open System
open System.ServiceModel
open Notifications
open DomainModel
open Simulator
open Service

type SendNotificationMessage = 
    | DataChanged of Notifications.DataChange<int, MarketPlacement>

type NotificationSender(clientSubscriber: IClientSubscriber) =    
    
    

    let messageProcessor = MailboxProcessor.Start(fun inbox ->
        
        let sendThese = ResizeArray<(int * int * decimal * decimal) * DataChangeType>()

        let rec loop() = 
            async {        
                let! msg = inbox.Receive()
                
                match msg with
                    | DataChanged dc ->
                        let serialisedMessage = Serializer.Serialize(dc.Data)
                        sendThese.Add(serialisedMessage, dc.DataChangeType)
                
                if sendThese.Count > 0 || inbox.CurrentQueueLength = 0 then
                    clientSubscriber.OnNext(sendThese.ToArray()) |> ignore
                    sendThese.Clear()
                
                do! loop()
                }        
        loop()
    )

    interface IObserver<Notifications.DataChange<int, MarketPlacement>> with
        member this.OnCompleted() = 0 |> ignore
        member this.OnError(e) = raise(e) |> ignore
        member this.OnNext(dc: Notifications.DataChange<int, MarketPlacement>) = 
            messageProcessor.Post(DataChanged dc)
    

[<ServiceBehavior(InstanceContextMode  = InstanceContextMode.Single)>]
type PublisherService(marketPlacementActor: MarketPlacementActor) = 
    let callback() = OperationContext.Current.GetCallbackChannel<IClientSubscriber>()

    interface IPublisherService with
        member this.SubscribeToMarketPlacements(deskId: int) = 
            let callbackObserver = NotificationSender(callback())
            do marketPlacementActor.Subscribe(callbackObserver) |> ignore
                