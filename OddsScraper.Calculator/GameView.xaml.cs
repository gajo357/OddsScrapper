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

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DownloadHelper.RefreashData((GameViewModel)DataContext);
        }
    }
}
