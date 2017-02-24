using System;
using System.ServiceModel;
using DomainModel;
using Simulator;

namespace PublisherService {
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
            //TODO here we are blocking the notification until the message is sent, need to implement mailbox
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
