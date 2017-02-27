namespace PublisherService

open System
open System.ServiceModel
open Notifications
open DomainModel
open Simulator
open Service
open System.Diagnostics

[<Interface>]
type IClientSubscriber =
    [<OperationContract(IsOneWay = true)>] // TODO is this line needed?
    abstract member OnNext : data:((int * int * decimal * decimal) * DataChangeType) [] -> unit

[<Interface>]
[<ServiceContract(CallbackContract = typedefof<IClientSubscriber>)>] 
type IPublisherService = 
    [<OperationContract(IsOneWay = true)>]
    abstract member SubscribeToMarketPlacements: deskId: int -> unit

type SendNotificationMessage = 
    | DataChanged of Notifications.DataChange<int, MarketPlacement>

type NotificationSender(clientSubscriber: IClientSubscriber) =    
    
    let messageProcessor = MailboxProcessor.Start(fun inbox ->
        
        let sendThese = ResizeArray<(int * int * decimal * decimal) * DataChangeType>()

        let rec loop() = 
            async {        
                let! msg = inbox.Receive()                
                if inbox.CurrentQueueLength > 1000 then Debug.WriteLine("NotificationSender " + inbox.CurrentQueueLength.ToString())

                match msg with
                    | DataChanged dc ->
                        let serialisedMessage = Serializer.Serialize(dc.Data)
                        sendThese.Add(serialisedMessage, dc.DataChangeType)
                
                if sendThese.Count > 50 || inbox.CurrentQueueLength = 0 then
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
                