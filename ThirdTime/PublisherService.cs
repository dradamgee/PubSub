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
    public class Returner : IObserver<ServiceInterface.DataChange<int, MarketPlacement>>
    {
        private Returner callback;
        private IClientSubscriber callback1;

        public Returner(IClientSubscriber callback1)
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

        public void OnNext(ServiceInterface.DataChange<int, MarketPlacement> value)
        {
            callback1.OnNext(value.ToString());
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

            Returner returner = new Returner(Callback);

            ((IObservable<ServiceInterface.DataChange<int, MarketPlacement>>) mockMarketPlacementProvider).Subscribe(returner);
        }

        public void SubscribeToFillExecutions(int placementId)
        {
            throw new NotImplementedException();
        }

        public void OnNext(ServiceInterface.DataChange<int, MarketPlacement> value)
        {
            Callback.OnNext(value.ToString());
        }

        public void OnNext(ServiceInterface.DataChange<int, FillExecution> value)
        {
            Callback.OnNext(value.ToString());
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
