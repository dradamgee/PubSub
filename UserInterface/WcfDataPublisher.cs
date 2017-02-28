using System;
using System.ServiceModel;
using DomainModel;
using UserInterface.ServiceReference1;

namespace UserInterface
{
    public class MarketPlacementCallbackActor : IPublisherServiceCallback {
        private readonly IObserver<Notifications.DataChange<int, MarketPlacement>> _marketPlacementObserver;
        private readonly IObserver<FillExecution> _fillExecutionObserver;

        public MarketPlacementCallbackActor(IObserver<Notifications.DataChange<int, MarketPlacement>> marketPlacementObserver,
            IObserver<FillExecution> fillExecutionObserver) {
            _marketPlacementObserver = marketPlacementObserver;
            _fillExecutionObserver = fillExecutionObserver;
        }

        public void OnNextMarketPlacment(Tuple<Tuple<int, int, decimal, decimal>, Notifications.DataChangeType>[] messages) {

            foreach (var message in messages)
            {
                var dataChangedArg = message.Item1;
                var type = message.Item2;

                var mp = new MarketPlacement(dataChangedArg.Item1, dataChangedArg.Item2, dataChangedArg.Item3, dataChangedArg.Item4);
                var dataChange = new Notifications.DataChange<int, MarketPlacement>(mp.ID, mp, type);
                _marketPlacementObserver.OnNext(dataChange);
            }
        }

        public void OnNextExecution(Tuple<int, decimal>[] messages)
        {
            foreach (var message in messages)
            {
                var fe = new FillExecution(message.Item1, message.Item2);
                _fillExecutionObserver.OnNext(fe);
            }
        }
    }

    //public class FillExecutionCallbackActor : IPublisherServiceCallback {
    //    private readonly IObserver<FillExecution> _observer;

    //    public FillExecutionCallbackActor(IObserver<FillExecution> observer) {
    //        _observer = observer;
    //    }

    //    public void OnNext(Tuple<int, decimal>[] messages) {

    //        foreach (var message in messages) {

    //            var fe = new FillExecution(message.Item1, message.Item2);
    //            _observer.OnNext(fe);
    //        }
    //    }
    //}

    public class WcfDataPublisher : Notifications.DataPublisher
    {
        public bool SubscribeToPlacements(Notifications.DeskFilter filter, IObserver<Notifications.DataChange<int, MarketPlacement>> observer)
        {
            MarketPlacementCallbackActor marketPlacementCallbackActor = new MarketPlacementCallbackActor(observer, null); // TODO think about splitting out the two methods.
            InstanceContext context = new InstanceContext(marketPlacementCallbackActor);
            var service = new PublisherServiceClient(context);
            service.SubscribeToMarketPlacements(filter.deskID); // TODO change the service contract to take a typed filter.
            return true;
        }

        public bool SubscribeToExecutions(Notifications.DeskFilter deskFilter, IObserver<FillExecution> observer)
        {
            //MarketPlacementCallbackActor marketPlacementCallbackActor = new MarketPlacementCallbackActor(null, observer); // TODO think about splitting out the two methods.
            //InstanceContext context = new InstanceContext(marketPlacementCallbackActor);
            //var service = new PublisherServiceClient(context);
            //service.SubscribeToFillExecutions(deskFilter.deskID); // TODO change the service contract to take a typed filter.
            return true;
        }
    }
}