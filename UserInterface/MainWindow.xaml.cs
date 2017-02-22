using System;
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

            var dataContext = new UI.MainWindowDataContext(mockMarketPlacementProvider);
            
            DataContext = dataContext;
        }
    }
}
