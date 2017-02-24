// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System.ServiceModel
open PublisherService

[<EntryPoint>]
let main argv = 

    let serviceType = typedefof<PublisherService.PublisherService>
    use serviceHost = new ServiceHost(serviceType)
    serviceHost.Open()
    printfn "service started"
    System.Console.In.ReadLine() |> ignore
    0    
        
