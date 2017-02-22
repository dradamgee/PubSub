using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using Client.Annotations;
using DomainModel;

namespace Client {
    public class Client 
    {
        public ObservableCollection<MarketPlacementViewModel> MarketPlacements { get; }

        public Client()
        {
            MarketPlacements = new ObservableCollection<MarketPlacementViewModel>();

            var mp1 = new MarketPlacement(1, 1000m, 0m);

            var mpo1 = new MarketPlacementObserable(mp1);

            var fillPublisher = new FillPublisher();

            fillPublisher.Subscribe(mpo1);

            MarketPlacementViewModel mpvm1 = new MarketPlacementViewModel(mpo1);
            
            MarketPlacements.Add(mpvm1);
        }
    }



    public class MarketPlacementViewModel : INotifyPropertyChanged, IObserver<MarketPlacement> {
        public MarketPlacementViewModel(MarketPlacementObserable datasource)
        {
            datasource.Subscribe(this);
        }

        private int id;
        private decimal quantity;
        private decimal filled;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnNext(MarketPlacement value)
        {
            ID = value.ID;
            Quantity = value.Quantity;
            Filled = value.Filled;
        }

        public int ID
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Quantity
        {
            get { return quantity; }
            set
            {
                if (quantity != value) {
                    quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal Filled
        {
            get { return filled; }
            set
            {
                if (filled != value) {
                    filled = value;
                    OnPropertyChanged();
                }
            }
        }

        public void OnError(Exception error) {throw new NotImplementedException();}
        public void OnCompleted() {throw new NotImplementedException();}
    }


    
    public class MarketPlacementObserable : IObserver<FillReceived>, IObservable<MarketPlacement>
    {
        private MarketPlacement _marketPlacement;

        public MarketPlacementObserable(MarketPlacement marketPlacement)
        {
            _marketPlacement = marketPlacement;
        }

        private List<IObserver<MarketPlacement>> _observers = new List<IObserver<MarketPlacement>>();
        
        public void OnNext(FillReceived value)
        {
            _marketPlacement = _marketPlacement.Fill(value);
            _observers.ForEach(o => o.OnNext(_marketPlacement));
        }

        public void OnError(Exception error){throw new NotImplementedException();}
        public void OnCompleted(){throw new NotImplementedException();}
        public IDisposable Subscribe(IObserver<MarketPlacement> observer)
        {
            lock (_observers)
            {
                _observers.Add(observer);
            }
            return new DisposibleAction(() => { lock (_observers) { _observers.Remove(observer); } });
        }

        public void Unsubscribe(IObserver<MarketPlacement> observer) {
            lock (_observers) {
                _observers.Remove(observer);
            }
        }
    }
    
}
