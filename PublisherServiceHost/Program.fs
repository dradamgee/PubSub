// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System.ServiceModel
//open PublisherService
open RabbitMQPublisherService
open Simulator

[<EntryPoint>]
let main argv = 
    let mmpp = MockMarketPlacementProvider(500.0)
    do mmpp.MockUpSomeStuff(66, 100) |> ignore
    do mmpp.StartFilling()
    
//    use serviceHost = new ServiceHost(PublisherService.MarketPlacementPublisherService mmpp.PlacementActor)
//    serviceHost.Open()
//
//    use serviceHost = new ServiceHost(PublisherService.FillExecutionPublisherService mmpp.PlacementActor)
//    serviceHost.Open()

    let marketPlacementService = new MarketPlacementPublisherService(mmpp.PlacementActor)
    marketPlacementService.Start()

    let fillExecutionService = new FillExecutionPublisherService(mmpp.PlacementActor)
    fillExecutionService.Start()

    printfn "service started"
    System.Console.In.ReadLine() |> ignore
    0   
