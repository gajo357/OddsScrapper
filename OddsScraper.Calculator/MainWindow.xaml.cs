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

            GamesControl.ItemsSource = Games;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            FSharp.Scraping.CanopyExtensions.loginToOddsPortalWithData(Username.Text, Password.Text);
        }

        private void RefreashData_Click(object sender, RoutedEventArgs e)
        {
            Games.Clear();

            foreach (var game in FSharp.Scraping.FutureGamesDownload
                .downloadTodaysGames(Convert.ToDouble(Minutes.Text))
                .Select(GameViewModel.Create))
                Games.Add(game);
        }
    }
}
