using System;
using System.ServiceModel;
using System.Windows;
using DomainModel;
using Service;
using Simulator;


namespace UserInterface {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            var mockMarketPlacementProvider = new MockMarketPlacementProvider(1);
            mockMarketPlacementProvider.MockUpSomeStuff(66, 100);
            mockMarketPlacementProvider.StartFilling();

            //InstanceContext context = new InstanceContext(this);
            //var Service = new ServiceReference1.PublisherServiceClient();

            //Service.SubscribeToMarketPlacements(66);

            
            

            var dataContext = new UI.MainWindowDataContext(mockMarketPlacementProvider);
            
            DataContext = dataContext;
        }
    }
}
