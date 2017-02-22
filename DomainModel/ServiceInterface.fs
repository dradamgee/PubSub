module ServiceInterface

open System
open DomainModel

type DataChangeType = Add | Update | Delete    

type DataChange<'key, 'data> (key: 'key, data: 'data, dataChangeType: DataChangeType) = 
    member this.Key = key
    member this.Data = data
    member this.DataChangeType = dataChangeType

type SubscriptionFilter(deskID: int) = 
    member this.deskID = deskID

type DataPublisher =     
    abstract member SubscribeToMarketPlacements: SubscriptionFilter -> IObservable<DataChange<int, MarketPlacement>>
    abstract member SubscribeToFills: SubscriptionFilter -> IObservable<DataChange<int, MarketPlacement>>

