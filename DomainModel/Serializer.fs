module Serializer

open DomainModel

type DomainSerializer = 
    static member Serialize (marketPlacement: MarketPlacement) = 
        (
        marketPlacement.ID, 
        marketPlacement.DeskId,
        marketPlacement.Quantity,
        marketPlacement.Filled
        )

    static member Serialize (fillExecution: FillExecution) = 
        (
        fillExecution.PlacementID,
        fillExecution.Quantity
        )

