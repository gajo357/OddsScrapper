using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
