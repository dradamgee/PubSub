using System;
using System.ServiceModel;
using System.Windows;
using DomainModel;
using Service;
using Simulator;
using System.Diagnostics;
using ThirdTime;
using UserInterface.ServiceReference1;

namespace UserInterface {



    public class CallbackActor : IPublisherServiceCallback
    {
        public void OnNext(string dataChangedArgs)
        {
            Debug.WriteLine(dataChangedArgs);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            var mockMarketPlacementProvider = new MockMarketPlacementProvider(1);
            mockMarketPlacementProvider.MockUpSomeStuff(66, 100);
            mockMarketPlacementProvider.StartFilling();

            InstanceContext context = new InstanceContext(new CallbackActor());
            var Service = new ServiceReference1.PublisherServiceClient(context);

            Service.SubscribeToMarketPlacements(66);




            var dataContext = new UI.MainWindowDataContext(mockMarketPlacementProvider);
            
            DataContext = dataContext;
        }
    }
}
