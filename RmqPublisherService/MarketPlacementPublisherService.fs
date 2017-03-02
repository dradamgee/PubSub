namespace RabbitMQPublisherService

open RabbitMQ.Client
open RabbitMQ.Client.Events
open System
open Service
open Notifications
open DomainModel
open System.Runtime.Serialization.Formatters.Binary
open System.IO

type MarketPlacementClientCallback(connection: IConnection, responseQueueName: string) = 
    let channel = connection.CreateModel()
    do channel.QueueDeclare(responseQueueName, false, false, false, null) |> ignore
    
    interface IObserver<DataChange<int, MarketPlacement>> with
        member this.OnNext(value: DataChange<int, MarketPlacement>) = 
            use stream = new MemoryStream()
            let formatter = new BinaryFormatter()
            let message = formatter.Serialize(stream, value)
            do channel.BasicPublish("", responseQueueName, null, stream.ToArray()) |> ignore

        member this.OnError(error : Exception)  : unit = 
            raise (error)
        member this.OnCompleted() : unit = 
            raise (NotImplementedException())
    
        
type MarketPlacementPublisherService(marketPlacementSupervisor: MarketPlacementSupervisor) = 
    let factory = new ConnectionFactory()
    do factory.HostName <- "localhost"
    let connection = factory.CreateConnection()
    let channel = connection.CreateModel()
    do channel.QueueDeclare("MarketPlacementScriptionRequest", false, false, false, null) |> ignore
    let consumer = new EventingBasicConsumer(channel)
    member this.ProcessScriptionRequest(message: byte[]) =         
        let responseQueueName = BinarySerializer.Deserialize(message) :?> string
        let callback = new MarketPlacementClientCallback(connection, responseQueueName)
        do marketPlacementSupervisor.Subscribe(callback) 
    
    member this.Start() =
        do consumer.Received.Add(
            fun (model) -> 
                this.ProcessScriptionRequest(model.Body)
            )
        do channel.BasicConsume("MarketPlacementScriptionRequest", true, consumer) |> ignore
    



