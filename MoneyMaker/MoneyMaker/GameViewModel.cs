using System;

namespace MoneyMaker
{
    public class GameViewModel : NotifyPropertyChanged
    {
        private string _sport;
        public string Sport { get => _sport; set { Set(ref _sport, value); } }
        private string _country;
        public string Country { get => _country; set { Set(ref _country, value); } }
        private string _league;
        public string League { get => _league; set { Set(ref _league, value); } }

        private string _homeTeam;
        public string HomeTeam { get => _homeTeam; set { Set(ref _homeTeam, value); } }
        private string _awayTeam;
        public string AwayTeam { get => _awayTeam; set { Set(ref _awayTeam, value); } }

        private DateTime _date;
        public DateTime Date { get => _date; set { Set(ref _date, value); } }

        private double _homeMeanOdd;
        public double HomeMeanOdd { get => _homeMeanOdd; set { Set(ref _homeMeanOdd, value); } }
        private double _drawMeanOdd;
        public double DrawMeanOdd { get => _drawMeanOdd; set { Set(ref _drawMeanOdd, value); } }
        private double _awayMeanOdd;
        public double AwayMeanOdd { get => _awayMeanOdd; set { Set(ref _awayMeanOdd, value); } }

        private double _homeOdd;
        public double HomeOdd { get => _homeOdd; set { Set(ref _homeOdd, value); } }
        private double _drawOdd;
        public double DrawOdd { get => _drawOdd; set { Set(ref _drawOdd, value); } }
        private double _awayOdd;
        public double AwayOdd { get => _awayOdd; set { Set(ref _awayOdd, value); } }

        private double _homeAmount;
        public double HomeAmount { get => _homeAmount; set { Set(ref _homeAmount, value); } }
        private double _drawAmount;
        public double DrawAmount { get => _drawAmount; set { Set(ref _drawAmount, value); } }
        private double _awayAmount;
        public double AwayAmount { get => _awayAmount; set { Set(ref _awayAmount, value); } }

        private double _balance;
        public double Balance { get => _balance; set { Set(ref _balance, value); } }

        protected override void OnPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(HomeOdd))
                CalculateHomeAmount();
            else if (propertyName == nameof(DrawOdd))
                CalculateDrawAmount();
            else if (propertyName == nameof(AwayOdd))
                CalculateAwayAmount();
            else if (propertyName == nameof(Balance))
                CalculateAmounts();
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

        private double _margin = 0.02;
        public void SetMargin(double margin)
        {
            _margin = 0.02;
            CalculateAmounts();
        }

        private double CalculateAmount(double meanOdd, double bookerOdd) => GetAmountToBet(_margin, Balance, meanOdd, bookerOdd);

        public static GameViewModel Create(GameModel model)
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

                Balance = 500
            };
        }

        private static double Kelly(double myOdd, double bookerOdd)
        {
            if (myOdd == 0)
                return 0;
            if (bookerOdd == 1)
                return 0;
            return (bookerOdd / myOdd - 1) / (bookerOdd - 1);
        }

        private static double GetAmountToBet(double maxPercent, double amount, double myOdd, double bookerOdd)
        {
            var k = Kelly(myOdd, bookerOdd);
            if (k > 0 && k <= maxPercent)
                return k * amount;
            return 0;
        }
    }
}
