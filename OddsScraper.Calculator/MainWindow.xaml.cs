using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace OddsScraper.Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<GameViewModel> Games { get; } = new ObservableCollection<GameViewModel>();

        public MainWindow()
        {
            InitializeComponent();

            SetCurrentDate();

            GamesControl.ItemsSource = Games;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentDate();
            DownloadHelper.LogIn(Username.Text, Password.Text);
        }

        private void RefreashData_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentDate();

            Games.Clear();

            foreach (var game in DownloadHelper.GetGames(Convert.ToDouble(Minutes.Text)))
            {
                SetGameMargin(game);
                SetGameAmount(game);
                Games.Add(game);
            }
        }

        private void SetCurrentDate() => Dispatcher.Invoke(() => CurrentDate.Text = DateTime.Now.ToString("g"));

        private void Margin_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            foreach (var game in Games) SetGameMargin(game);
        }

        private void SetGameMargin(GameViewModel game) => game.SetMargin(GetMargin());

        private double GetMargin() => Convert.ToDouble(KellyMargin.Text);

        private void Amount_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            foreach (var game in Games) SetGameAmount(game);
        }

        private void SetGameAmount(GameViewModel game) => game.SetBalance(GetAmount());

        private double GetAmount() => Convert.ToDouble(Amount.Text);
    }
}
