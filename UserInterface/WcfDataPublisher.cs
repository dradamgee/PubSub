using System;
using System.ServiceModel;
using DomainModel;
using UserInterface.ServiceReference1;

namespace UserInterface
{
    public class MarketPlacementCallbackActor : IPublisherServiceCallback {
        private readonly IObserver<Notifications.DataChange<int, MarketPlacement>> _observer;

        public MarketPlacementCallbackActor(IObserver<Notifications.DataChange<int, MarketPlacement>> observer) {
            _observer = observer;
        }

        public void OnNext(int key, Tuple<int, int, decimal, decimal> dataChangedArgs, Notifications.DataChangeType type) {
            var mp = new MarketPlacement(dataChangedArgs.Item1, dataChangedArgs.Item2, dataChangedArgs.Item3, dataChangedArgs.Item4);
            var dataChange = new Notifications.DataChange<int, MarketPlacement>(key, mp, type);
            _observer.OnNext(dataChange);
        }
    }

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
}