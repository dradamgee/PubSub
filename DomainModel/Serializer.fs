module Serializer

open DomainModel

let Serialize (marketPlacement: MarketPlacement) = 
    (
    marketPlacement.ID, 
    marketPlacement.DeskId,
    marketPlacement.Quantity,
    marketPlacement.Filled
    )

