using System;
using System.Collections.Generic;
using System.Windows;
using DomainModel;
using Service;
using Simulator;
using System.Diagnostics;
using UI;
using UserInterface.ServiceReference1;

namespace UserInterface {


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            var dataContext = new UI.MainWindowDataContext(Dispatcher);
            WcfDataPublisher publisher = new WcfDataPublisher();
            publisher.SubscribeToPlacements(new Notifications.DeskFilter(66), new MarketPlacementObserver(dataContext));
            publisher.SubscribeToExecutions(new Notifications.DeskFilter(66), new FillExecutionObserver(dataContext));
            DataContext = dataContext;
        }
    }
}
