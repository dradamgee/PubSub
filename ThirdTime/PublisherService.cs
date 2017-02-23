using DomainModel;
using Simulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ThirdTime
{
    public class CallbackObserver : IObserver<Notifications.DataChange<int, MarketPlacement>>
    {
        private readonly IClientSubscriber callback1;

        public CallbackObserver(IClientSubscriber callback1)
        {
            this.callback1 = callback1;
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(Notifications.DataChange<int, MarketPlacement> value)
        {
            callback1.OnNext(value.Key, Serializer.Serialize(value.Data), value.DataChangeType);
        }
    }

    //[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class PublisherService : IPublisherService

    {
        private MockMarketPlacementProvider mockMarketPlacementProvider;


        public PublisherService()
        {
            mockMarketPlacementProvider = new MockMarketPlacementProvider(1);
            mockMarketPlacementProvider.MockUpSomeStuff(66, 100);
            mockMarketPlacementProvider.StartFilling();
        }

        IClientSubscriber Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IClientSubscriber>();
            }
        }

        public void SubscribeToMarketPlacements(int deskId)
        {
            CallbackObserver callbackObserver = new CallbackObserver(Callback);
            ((IObservable<Notifications.DataChange<int, MarketPlacement>>) mockMarketPlacementProvider).Subscribe(callbackObserver);
        }
    }
}
