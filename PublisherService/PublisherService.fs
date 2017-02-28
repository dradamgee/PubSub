namespace PublisherService

open System
open System.ServiceModel
open Notifications
open DomainModel
open Simulator
open Service
open Diagnostics
open Serializer

//
//[<Interface>]
//type IClientSubscriber =
//    [<OperationContract(IsOneWay = true)>] // TODO is this line needed?
//    abstract member OnNextExecution : data:(int * decimal) [] -> unit
//    [<OperationContract(IsOneWay = true)>] // TODO is this line needed?
//    abstract member OnNextMarketPlacment : data:((int * int * decimal * decimal) * DataChangeType) [] -> unit
//

[<Interface>]
type IClientMarketPlacementSubscriber =
    [<OperationContract(IsOneWay = true)>] // TODO is this line needed?
    abstract member OnNext : data:((int * int * decimal * decimal) * DataChangeType) [] -> unit

type IClientFillExecutionSubscriber =
    [<OperationContract(IsOneWay = true)>] // TODO is this line needed?
    abstract member OnNext : data:(int * decimal * decimal) [] -> unit

[<Interface>]
[<ServiceContract(CallbackContract = typedefof<IClientMarketPlacementSubscriber>)>] 
type IMarketPlacementPublisherService = 
    [<OperationContract(IsOneWay = true)>]
    abstract member SubscribeToMarketPlacements: deskId: int -> unit

[<Interface>]
[<ServiceContract(CallbackContract = typedefof<IClientFillExecutionSubscriber>)>] 
type IFillExecutionPublisherService = 
    [<OperationContract(IsOneWay = true)>]
    abstract member SubscribeToFillExecutions: deskId: int -> unit

type SendNotificationMessage = 
    | MarketPlacementChanged of Notifications.DataChange<int, MarketPlacement>
    | FillExecutionMessage of FillExecution
    
type MarketPlacementNotificationSender(clientSubscriber: IClientMarketPlacementSubscriber) =        
    let messageProcessor = MailboxProcessor.Start(fun inbox ->        
        let sendThese = ResizeArray<(int * int * decimal * decimal) * DataChangeType>()
        let rec loop() = 
            async {        
                let! msg = inbox.Receive()                
                InboxWatcher.Watch(inbox)
                match msg with
                    | MarketPlacementChanged dc ->
                        let serialisedMessage = DomainSerializer.Serialize(dc.Data)
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
            messageProcessor.Post(MarketPlacementChanged dc)
    


type FillExecutionNotificationSender(clientSubscriber: IClientFillExecutionSubscriber) =        
    let messageProcessor = MailboxProcessor.Start(fun inbox ->        
        let sendThese = ResizeArray<int * decimal * decimal>()
        let rec loop() = 
            async {        
                let! msg = inbox.Receive()                
                InboxWatcher.Watch(inbox)
                match msg with
                    | FillExecutionMessage fe ->
                        let serialisedMessage = DomainSerializer.Serialize(fe)
                        sendThese.Add(serialisedMessage)                
                if sendThese.Count > 50 || inbox.CurrentQueueLength = 0 then
                    clientSubscriber.OnNext(sendThese.ToArray()) |> ignore
                    sendThese.Clear()                
                do! loop()
                }        
        loop()
    )

    interface IObserver<FillExecution> with
        member this.OnCompleted() = 0 |> ignore
        member this.OnError(e) = raise(e) |> ignore
        member this.OnNext(fe: FillExecution) = 
            messageProcessor.Post(FillExecutionMessage fe)
    

[<ServiceBehavior(InstanceContextMode  = InstanceContextMode.Single)>]
type MarketPlacementPublisherService(marketPlacementSupervisor: MarketPlacementSupervisor) = 
    let subscriberCallback() = OperationContext.Current.GetCallbackChannel<IClientMarketPlacementSubscriber>()    

    interface IMarketPlacementPublisherService with
        member this.SubscribeToMarketPlacements(deskId: int): unit = 
            let callbackObserver = MarketPlacementNotificationSender(subscriberCallback())
            do marketPlacementSupervisor.Subscribe(callbackObserver) |> ignore

[<ServiceBehavior(InstanceContextMode  = InstanceContextMode.Single)>]
type FillExecutionPublisherService(marketPlacementSupervisor: MarketPlacementSupervisor) = 
    let subscriberCallback() = OperationContext.Current.GetCallbackChannel<IClientFillExecutionSubscriber>()    

    interface IFillExecutionPublisherService with
        member this.SubscribeToFillExecutions(deskId: int) = 
            let callbackObserver = FillExecutionNotificationSender(subscriberCallback())
            do marketPlacementSupervisor.Subscribe(callbackObserver) |> ignore
                