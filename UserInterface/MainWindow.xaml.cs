using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using DomainModel;
using Service;
using Simulator;
using System.Diagnostics;

using UserInterface.ServiceReference1;

namespace UserInterface {

    public class WcfDataPublisher : Notifications.DataPublisher
    {
        public bool Subscribe(Notifications.DeskFilter filter, IObserver<Notifications.DataChange<int, MarketPlacement>> observer)
        {
            MarketPlacementCallbackActor marketPlacementCallbackActor = new MarketPlacementCallbackActor(observer);
            InstanceContext context = new InstanceContext(marketPlacementCallbackActor);
            var service = new PublisherServiceClient(context);
            service.SubscribeToMarketPlacements(filter.deskID); // TODO change the service contract to take a typed filter.
            return true;
        }
    }

    public class MarketPlacementCallbackActor : IPublisherServiceCallback {
        private readonly IObserver<Notifications.DataChange<int, MarketPlacement>> _observer;

        public MarketPlacementCallbackActor(IObserver<Notifications.DataChange<int, MarketPlacement>> observer)
        {
            _observer = observer;
        }

        public void OnNext(int key, Tuple<int, int, decimal, decimal> dataChangedArgs, Notifications.DataChangeType type)
        {
            var mp = new MarketPlacement(dataChangedArgs.Item1, dataChangedArgs.Item2, dataChangedArgs.Item3, dataChangedArgs.Item4);
            var dataChange = new Notifications.DataChange<int, MarketPlacement>(key, mp, type);
            _observer.OnNext(dataChange); 
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            var dataContext = new UI.MainWindowDataContext(Dispatcher);
            WcfDataPublisher publisher = new WcfDataPublisher();
            publisher.Subscribe(new Notifications.DeskFilter(66), dataContext);
            DataContext = dataContext;
        }
    }
}
