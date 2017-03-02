namespace RmqPublisherClient

open Notifications
open RabbitMQ.Client
open System
open System.IO
open System.Runtime.Serialization.Formatters.Binary
open DomainModel
open RabbitMQ.Client.Events

    



type PublisherClient(clientId: string) = 
    let hostName = "localhost"
    let marketPlacementRequestQueueName = "MarketPlacementScriptionRequest"
    let fillExecutionRequestQueueName = "FillExecutionScriptionRequest"
    let marketPlacementResponeQueueName = "mp" + clientId
    let fillExecutionResponeQueueName = "fe" + clientId
    
    let factory = new ConnectionFactory()
    do factory.HostName <- hostName
    let connection = factory.CreateConnection()
    let channel = connection.CreateModel()
    
    // Declare the request channel incase it's not there.
    
    do channel.QueueDeclare(marketPlacementRequestQueueName, false, false, false, null) |> ignore
    do channel.QueueDeclare(fillExecutionRequestQueueName, false, false, false, null) |> ignore
    do channel.QueueDeclare(marketPlacementResponeQueueName, false, false, false, null) |> ignore
    do channel.QueueDeclare(fillExecutionResponeQueueName, false, false, false, null) |> ignore
    
    member this.ProcessMarketPlacementRespone(observer: IObserver<Notifications.DataChange<int, MarketPlacement>>)(message: byte[]) = 
        let value = BinarySerializer.Deserialize(message) :?> Notifications.DataChange<int, MarketPlacement>
        do observer.OnNext(value)
    member this.ProcessFillExecutionRespone(observer: IObserver<FillExecution>)(message: byte[]) = 
        let value = BinarySerializer.Deserialize(message) :?> FillExecution
        do observer.OnNext(value)

    interface Notifications.DataPublisher with        
        member this.SubscribeToPlacements (filter: Notifications.DeskFilter) (observer: IObserver<Notifications.DataChange<int, MarketPlacement>>) = 
            // request a response
            let message = BinarySerializer.Serialize(marketPlacementResponeQueueName)
            do channel.BasicPublish("", marketPlacementRequestQueueName, null, message) |> ignore            
            // start the response processor
            let consumer = new EventingBasicConsumer(channel)
            do consumer.Received.Add(
                fun (model) -> this.ProcessMarketPlacementRespone observer model.Body |> ignore
            )
            do channel.BasicConsume(marketPlacementResponeQueueName, true, consumer) |> ignore            
            true
            
        member this.SubscribeToExecutions(filter: DeskFilter) (observer: IObserver<FillExecution>): bool = 
            // request a response
            let message = BinarySerializer.Serialize(fillExecutionResponeQueueName)
            do channel.BasicPublish("", fillExecutionRequestQueueName, null, message) |> ignore
            // start the response processor            
            let consumer = new EventingBasicConsumer(channel)
            do consumer.Received.Add(
                fun (model) -> this.ProcessFillExecutionRespone observer model.Body |> ignore
            )
            do channel.BasicConsume(fillExecutionResponeQueueName, true, consumer) |> ignore            
            true
        
            
        
