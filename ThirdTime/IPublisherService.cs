using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ThirdTime
{
    //SessionMode = SessionMode.Required, 
    [ServiceContract(CallbackContract = typeof(IClientSubscriber))] 
    public interface IPublisherService
    {
        [OperationContract(IsOneWay = true)]
        void SubscribeToMarketPlacements(int deskId);
        void SubscribeToFillExecutions(int placementId);
    }

    public interface IClientSubscriber
    {
        [OperationContract(IsOneWay = true)]
        void OnNext(string dataChangedArgs);
    }
}
