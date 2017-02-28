namespace UI

open System
open System.Collections.Generic
open System.Collections.ObjectModel
open System.ComponentModel
open DomainModel
open Service
open Notifications
open System.Windows
open Diagnostics

type MarketPlacementViewModel(initialValue: MarketPlacement) =
    let mutable model = initialValue
    let propertyChanged = Event<_, _>()
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged = propertyChanged.Publish
    member private this.OnPropertyChanged p = propertyChanged.Trigger(this, PropertyChangedEventArgs(p))
    member this.ID with get() = model.ID
    member this.Quantity with get() = model.Quantity
    member this.Filled with get() = model.Filled

    member this.UpdateFrom(newModel: MarketPlacement) = 
        let oldmodel = model
        model <- newModel
        if oldmodel.ID <> newModel.ID then
            this.OnPropertyChanged("ID")
        if oldmodel.Quantity <> newModel.Quantity then
            this.OnPropertyChanged("Quantity")
        if oldmodel.Filled <> newModel.Filled then
            this.OnPropertyChanged("Filled")



type MainWindowDataContextActions = 
    | OnNext of DataChange<int, MarketPlacement>



type MainWindowDataContext (dispatcher: System.Windows.Threading.Dispatcher) =     
    let data = ObservableCollection<MarketPlacementViewModel>()
    let ViewModels = Dictionary<int, MarketPlacementViewModel>()
    
    let messageProcessor = MailboxProcessor.Start(fun inbox ->
        let rec loop() = 
            async {
                let! msg = inbox.Receive()                                

                InboxWatcher.Watch(inbox)                
                match msg with
                    | MainWindowDataContextActions.OnNext value ->
                        match ViewModels.TryGetValue(value.Key) with
                        | true, vm -> dispatcher.Invoke(fun () -> vm.UpdateFrom(value.Data))
                        | false, _ -> 
                            let newVm = MarketPlacementViewModel(value.Data)
                            ViewModels.Add(value.Key, newVm)
                            dispatcher.Invoke(fun () -> data.Add(newVm))
                do! loop()
            }
        loop()        
    )
    
    member this.MarketPlacements = data

    interface IObserver<DataChange<int, MarketPlacement>> with 
        member this.OnNext(value: DataChange<int, MarketPlacement>) =                                                
            messageProcessor.Post(OnNext(value))
        member this.OnError(error: Exception) =
            raise (error)
        member this.OnCompleted() =
            raise (NotImplementedException())







