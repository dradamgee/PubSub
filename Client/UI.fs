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
    let fills = ObservableCollection<FillExecution>()
    let mutable model = initialValue
    let propertyChanged = Event<_, _>()
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged = propertyChanged.Publish
    member private this.OnPropertyChanged p = propertyChanged.Trigger(this, PropertyChangedEventArgs(p))
    member this.ID with get() = model.ID
    member this.Quantity with get() = model.Quantity
    member this.Filled with get() = model.Filled

    member this.FillRecieved(fillExecution) = 
        fills.Add(fillExecution)
    
    member this.Fills = fills

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
    | MarketPlacementChanged of DataChange<int, MarketPlacement>
    | FillReceived of FillExecution

    
type MainWindowDataContext (dispatcher: System.Windows.Threading.Dispatcher) =     
    let data = ObservableCollection<MarketPlacementViewModel>()
    let ViewModels = Dictionary<int, MarketPlacementViewModel>()
    
    let messageProcessor = MailboxProcessor.Start(fun inbox ->
        let rec loop() = 
            async {
                let! msg = inbox.Receive()                                

                InboxWatcher.Watch(inbox)                
                match msg with
                    | MarketPlacementChanged mpc ->
                        match ViewModels.TryGetValue(mpc.Key) with
                        | true, vm -> dispatcher.Invoke(fun () -> vm.UpdateFrom(mpc.Data))
                        | false, _ -> 
                            let newVm = MarketPlacementViewModel(mpc.Data)
                            ViewModels.Add(mpc.Key, newVm)
                            dispatcher.Invoke(fun () -> data.Add(newVm))
                    | FillReceived fill ->
                        match ViewModels.TryGetValue(fill.PlacementID) with
                        | true, vm -> dispatcher.Invoke(fun () -> vm.FillRecieved(fill))
                        | false, _ -> ignore // TODO, think about what happens if we get a fill before the order, what about unsolicited.

                do! loop()
            }
        loop()        
    )
    
    member this.PostMessage(message: MainWindowDataContextActions) = messageProcessor.Post(message)

    member this.MarketPlacements = data

type MarketPlacementObserver(dataContext: MainWindowDataContext) =
    
    interface IObserver<DataChange<int, MarketPlacement>> with 
        member this.OnNext(value: DataChange<int, MarketPlacement>) =                                                
            dataContext.PostMessage(MarketPlacementChanged(value))
        member this.OnError(error: Exception) =
            raise (error)
        member this.OnCompleted() =
            raise (NotImplementedException())

type FillExecutionObserver(dataContext: MainWindowDataContext) =
    interface IObserver<FillExecution> with 
        member this.OnNext(value: FillExecution) =                                                
            dataContext.PostMessage(FillReceived(value))
        member this.OnError(error: Exception) =
            raise (error)
        member this.OnCompleted() =
            raise (NotImplementedException())







