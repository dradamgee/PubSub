// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System.ServiceModel
open PublisherService
open Simulator

[<EntryPoint>]
let main argv = 
    let mmpp = MockMarketPlacementProvider(1.0)
    do mmpp.MockUpSomeStuff(66, 100) |> ignore
    do mmpp.StartFilling()

    let serviceType = typedefof<PublisherService.PublisherService>
    let service = PublisherService.PublisherService(mmpp.PlacementActor)    

    use serviceHost = new ServiceHost(service)
    serviceHost.Open()
    printfn "service started"
    System.Console.In.ReadLine() |> ignore
    0    
        
