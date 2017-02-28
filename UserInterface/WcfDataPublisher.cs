using System;
using System.ServiceModel;
using DomainModel;
using UserInterface.ServiceReference1;
using UserInterface.ServiceReference2;

namespace UserInterface
{
    public class MarketPlacementCallbackActor : IMarketPlacementPublisherServiceCallback
    {
        private readonly IObserver<Notifications.DataChange<int, MarketPlacement>> _marketPlacementObserver;

        public MarketPlacementCallbackActor(IObserver<Notifications.DataChange<int, MarketPlacement>> marketPlacementObserver)
        {
            _marketPlacementObserver = marketPlacementObserver;
        }

        public void OnNext(Tuple<Tuple<int, int, decimal, decimal>, Notifications.DataChangeType>[] data)
        {
            foreach (var message in data) {
                var dataChangedArg = message.Item1;
                var type = message.Item2;

                var mp = new MarketPlacement(dataChangedArg.Item1, dataChangedArg.Item2, dataChangedArg.Item3, dataChangedArg.Item4);
                var dataChange = new Notifications.DataChange<int, MarketPlacement>(mp.ID, mp, type);
                _marketPlacementObserver.OnNext(dataChange);
            }
        }
    }

    public class FillExecutionCallbackActor : IFillExecutionPublisherServiceCallback {
        private readonly IObserver<FillExecution> _fillExecutionObserver;

        public FillExecutionCallbackActor(IObserver<FillExecution> fillExecutionObserver)
        {
            _fillExecutionObserver = fillExecutionObserver;
        }

        public void OnNext(Tuple<int, decimal, decimal>[] data)
        {
            foreach (var message in data) {
                var fe = new FillExecution(message.Item1, message.Item2, message.Item3);
                _fillExecutionObserver.OnNext(fe);
            }
        }
    }

    public class WcfDataPublisher : Notifications.DataPublisher
    {
        public bool SubscribeToPlacements(Notifications.DeskFilter filter, IObserver<Notifications.DataChange<int, MarketPlacement>> observer)
        {
            var callbackActor = new MarketPlacementCallbackActor(observer);
            var context = new InstanceContext(callbackActor);
            var service = new MarketPlacementPublisherServiceClient(context);
            service.SubscribeToMarketPlacements(filter.deskID); // TODO change the service contract to take a typed filter.
            return true;
        }

        public bool SubscribeToExecutions(Notifications.DeskFilter deskFilter, IObserver<FillExecution> observer)
        {
            var callbackActor = new FillExecutionCallbackActor(observer);
            var context = new InstanceContext(callbackActor);
            var service = new FillExecutionPublisherServiceClient(context);
            service.SubscribeToFillExecutions(deskFilter.deskID);
            return true;
        }
    }
}