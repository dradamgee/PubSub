using System;
using System.Collections.Generic;
using System.Windows;
using DomainModel;
using Service;
using Simulator;
using System.Diagnostics;

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
            publisher.Subscribe(new Notifications.DeskFilter(66), dataContext);
            DataContext = dataContext;
        }
    }
}
