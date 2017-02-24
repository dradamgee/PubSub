using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace PublisherService {
    //SessionMode = SessionMode.Required, 
    [ServiceContract(CallbackContract = typeof(IClientSubscriber))] 
    public interface IPublisherService
    {
        [OperationContract(IsOneWay = true)]
        void SubscribeToMarketPlacements(int deskId);
        
    }

    public interface IClientSubscriber
    {
        [OperationContract(IsOneWay = true)]
        void OnNext(int key, Tuple<int,int, decimal, decimal> data, Notifications.DataChangeType dataChangeType);
    }
}
