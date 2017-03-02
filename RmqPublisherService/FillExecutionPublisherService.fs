namespace RabbitMQPublisherService

open RabbitMQ.Client
open RabbitMQ.Client.Events
open System
open Service
open Notifications
open DomainModel
open System.Runtime.Serialization.Formatters.Binary
open System.IO

// TODO make the cilent callback generic
type FillExecutionClientCallback(connection: IConnection, responseQueueName: string) = 
    let channel = connection.CreateModel()
    do channel.QueueDeclare(responseQueueName, false, false, false, null) |> ignore
    
    interface IObserver<FillExecution> with
        member this.OnNext(value: FillExecution) = 
            use stream = new MemoryStream()
            let formatter = new BinaryFormatter()
            let message = formatter.Serialize(stream, value)
            do channel.BasicPublish("", responseQueueName, null, stream.ToArray()) |> ignore

        member this.OnError(error : Exception)  : unit = 
            raise (error)
        member this.OnCompleted() : unit = 
            raise (NotImplementedException())
    

type FillExecutionPublisherService(marketPlacementSupervisor: MarketPlacementSupervisor) = 
    let factory = new ConnectionFactory()
    do factory.HostName <- "localhost"
    let connection = factory.CreateConnection()
    let channel = connection.CreateModel()
    do channel.QueueDeclare("FillExecutionScriptionRequest", false, false, false, null) |> ignore
    let consumer = new EventingBasicConsumer(channel)
    member this.ProcessScriptionRequest(message: byte[]) = 
        let responseQueueName = BinarySerializer.Deserialize(message) :?> string
        let callback = new FillExecutionClientCallback(connection, responseQueueName)
        do marketPlacementSupervisor.Subscribe(callback) 
    
    member this.Start() =
        do consumer.Received.Add(
            fun (model) -> 
                this.ProcessScriptionRequest(model.Body)
            )
        do channel.BasicConsume("FillExecutionScriptionRequest", true, consumer) |> ignore
    



