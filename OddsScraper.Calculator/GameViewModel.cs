using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OddsScraper.Calculator
{
    public class GameViewModel : INotifyPropertyChanged
    {
        public string _sport;
        public string Sport { get => _sport; set { Set(ref _sport, value); } }
        public string _country;
        public string Country { get => _country; set { Set(ref _country, value); } }
        public string _league;
        public string League { get => _league; set { Set(ref _league, value); } }

        public string _homeTeam;
        public string HomeTeam { get => _homeTeam; set { Set(ref _homeTeam, value); } }
        public string _awayTeam;
        public string AwayTeam { get => _awayTeam; set { Set(ref _awayTeam, value); } }

        public DateTime _date;
        public DateTime Date { get => _date; set { Set(ref _date, value); } }

        public double _homeMeanOdd;
        public double HomeMeanOdd { get => _homeMeanOdd; set { Set(ref _homeMeanOdd, value); } }
        public double _drawMeanOdd;
        public double DrawMeanOdd { get => _drawMeanOdd; set { Set(ref _drawMeanOdd, value); } }
        public double _awayMeanOdd;
        public double AwayMeanOdd { get => _awayMeanOdd; set { Set(ref _awayMeanOdd, value); } }

        public double _homeOdd;
        public double HomeOdd { get => _homeOdd; set { Set(ref _homeOdd, value); } }
        public double _drawOdd;
        public double DrawOdd { get => _drawOdd; set { Set(ref _drawOdd, value); } }
        public double _awayOdd;
        public double AwayOdd { get => _awayOdd; set { Set(ref _awayOdd, value); } }

        public double _homeAmount;
        public double HomeAmount { get => _homeAmount; set { Set(ref _homeAmount, value); } }
        public double _drawAmount;
        public double DrawAmount { get => _drawAmount; set { Set(ref _drawAmount, value); } }
        public double _awayAmount;
        public double AwayAmount { get => _awayAmount; set { Set(ref _awayAmount, value); } }

        public string GameLink { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Set<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            field = value;

            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(HomeOdd))
                CalculateHomeAmount();
            else if (propertyName == nameof(DrawOdd))
                CalculateDrawAmount();
            else if (propertyName == nameof(AwayOdd))
                CalculateAwayAmount();
        }

        private void CalculateAmounts()
        {
            CalculateHomeAmount();
            CalculateDrawAmount();
            CalculateAwayAmount();
        }
        private void CalculateHomeAmount() => HomeAmount = CalculateAmount(HomeMeanOdd, HomeOdd);
        private void CalculateDrawAmount() => DrawAmount = CalculateAmount(DrawMeanOdd, DrawOdd);
        private void CalculateAwayAmount() => AwayAmount = CalculateAmount(AwayMeanOdd, AwayOdd);

        public double _balance;
        public void SetBalance(double balance)
        {
            _balance = balance;
            CalculateAmounts();
        }

        private double _margin;
        public void SetMargin(double margin)
        {
            _margin = margin;
            CalculateAmounts();
        }
        private double CalculateAmount(double meanOdd, double bookerOdd) 
            => DownloadHelper.CalculateAmount(_margin, _balance, meanOdd, bookerOdd);

        public static GameViewModel Create(FSharp.CommonScraping.Models.Game model)
        {
            return new GameViewModel
            {
                Sport = model.Sport,
                Country = model.Country,
                League = model.League,

                HomeTeam = model.HomeTeam,
                AwayTeam = model.AwayTeam,

                Date = model.Date,

                HomeMeanOdd = model.HomeMeanOdd,
                DrawMeanOdd = model.DrawMeanOdd,
                AwayMeanOdd = model.AwayMeanOdd,

                HomeOdd = model.HomeOdd,
                DrawOdd = model.DrawOdd,
                AwayOdd = model.AwayOdd,

                GameLink = model.GameLink
            };
        }
    }
}
