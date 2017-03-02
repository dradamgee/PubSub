module Notifications

open System
open DomainModel
open System.IO
open System.Runtime.Serialization.Formatters.Binary

type DataChangeType = Add | Update | Delete    

type DataChange<'key, 'data> (key: 'key, data: 'data, dataChangeType: DataChangeType) = 
    member this.Key = key
    member this.Data = data
    member this.DataChangeType = dataChangeType

type DeskFilter(deskID: int) = 
    member this.deskID = deskID

type PlacementFilter(placementId: int) = 
    member this.PlacementId = placementId

type DataPublisher =     
    abstract member SubscribeToPlacements: DeskFilter -> IObserver<DataChange<int, MarketPlacement>> -> bool
    abstract member SubscribeToExecutions: DeskFilter -> IObserver<FillExecution> -> bool

type BinarySerializer = 
    static member Serialize(value: obj) = 
        use stream = new MemoryStream()
        let formatter = new BinaryFormatter()
        do formatter.Serialize(stream, value)
        stream.ToArray()
    static member Deserialize(message: byte[]) =
        use stream = new MemoryStream(message)
        let formatter = new BinaryFormatter()
        formatter.Deserialize(stream)
