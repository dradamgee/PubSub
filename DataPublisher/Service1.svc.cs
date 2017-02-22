﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using DomainModel;
using Simulator;

namespace DataPublisher {
    //[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class Service1 : IPublisherService/*, IObserver<ServiceInterface.DataChange<int, MarketPlacement>>, IObserver<ServiceInterface.DataChange<int, FillExecution>> */{
        private MockMarketPlacementProvider mockMarketPlacementProvider;


        public Service1()
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
            //((IObservable<ServiceInterface.DataChange<int, MarketPlacement>>) mockMarketPlacementProvider).Subscribe(this);
        }

        public void SubscribeToFillExecutions(int placementId)
        {
            throw new NotImplementedException();
        }

        public void OnNext(ServiceInterface.DataChange<int, MarketPlacement> value)
        {
            Callback.OnNext(value.ToString());
        }

        public void OnNext(ServiceInterface.DataChange<int, FillExecution> value) {
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
