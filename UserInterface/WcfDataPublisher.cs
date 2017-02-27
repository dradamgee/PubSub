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

        public void OnNext(Tuple<int, int, decimal, decimal>[] dataChangedArgs, Notifications.DataChangeType type) {

            foreach (var dataChangedArg in dataChangedArgs)
            {
                var mp = new MarketPlacement(dataChangedArg.Item1, dataChangedArg.Item2, dataChangedArg.Item3, dataChangedArg.Item4);
                var dataChange = new Notifications.DataChange<int, MarketPlacement>(mp.ID, mp, type);
                _observer.OnNext(dataChange);
            }

            
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