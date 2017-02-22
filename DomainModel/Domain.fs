namespace DomainModel

open System.Timers
open System
open System.Collections.Generic
open System.Runtime.Serialization

type FillExecution(id: int, quantity: decimal) = 
    member this.PlacementID = id
    member this.Quantity = quantity

type MarketPlacement(id: int, deskId: int, quantity: decimal, filled: decimal) = 
    member this.ID = id
    member this.DeskId = deskId
    member this.Quantity = quantity
    member this.Filled = filled
    member this.Fill(fill : FillExecution) =         
        MarketPlacement(this.ID,  this.DeskId, this.Quantity, this.Filled + fill.Quantity)
 
    


        

            
            
            
    
    
    


