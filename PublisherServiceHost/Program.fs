// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System.ServiceModel
open PublisherService

[<EntryPoint>]
let main argv = 
    use serviceHost = ServiceHost(typedefof<PublisherService.PublisherService>)
    serviceHost.Open()
    printfn "service started"
    System.Console.In.ReadLine() |> ignore
    0    
        
