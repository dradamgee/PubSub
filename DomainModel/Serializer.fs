module Serializer

open DomainModel

//type SerializableMarketPlacement(id: int, deskId: int, quantity: decimal, filled: decimal) = 
//    member this.ID = id
//    member this.DeskId = deskId
//    member this.Quantity = quantity
//    member this.Filled = filled

  

let Serialize (marketPlacement: MarketPlacement) = 
    (
    marketPlacement.ID, 
    marketPlacement.DeskId,
    marketPlacement.Quantity,
    marketPlacement.Filled
    )

