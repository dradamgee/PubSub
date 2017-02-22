module ServiceInterface

open System
open DomainModel

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
    abstract member SubscribeToMarketPlacements: DeskFilter -> IObservable<DataChange<int, MarketPlacement>>
    abstract member SubscribeToFillExecutions: PlacementFilter -> IObservable<DataChange<int, FillExecution>>

