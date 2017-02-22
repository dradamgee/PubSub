using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using DomainModel;

namespace DataPublisher {
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract( SessionMode = SessionMode.Required, CallbackContract = typeof(IClientSubscriber))]
    public interface IPublisherService {

        [OperationContract(IsOneWay = true)]
        void SubscribeToMarketPlacements(int deskId);
        void SubscribeToFillExecutions(int placementId);
    }

    public interface IClientSubscriber {
        [OperationContract(IsOneWay = true)]
        void OnNext(string dataChangedArgs);
    }

    

}
