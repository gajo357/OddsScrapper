using System.Windows.Controls;

namespace OddsScraper.Calculator
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await DownloadHelper.RefreashData((GameViewModel)DataContext);
        }
    }
}
