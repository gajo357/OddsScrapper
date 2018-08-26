using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace MoneyMaker
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private HttpClient Client { get; } = new HttpClient() {
            BaseAddress = new System.Uri("https://oddsscraperapi.northeurope.cloudapp.azure.com/api/") };

        public MainViewModel()
        {
            Client = new HttpClient()
            {
                BaseAddress = new System.Uri("https://oddsscraperapi.northeurope.cloudapp.azure.com/api/")
            };
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private string _username;
        public string Username { get => _username; set { Set(ref _username, value); } }
        private string _password;
        public string Password { get => _password; set { Set(ref _password, value); } }

        public bool IsNotLoggedIn => !IsLoggedIn;
        private bool _isLoggedIn;
        public bool IsLoggedIn { get => _isLoggedIn; set { Set(ref _isLoggedIn, value); OnPropertyChanged(nameof(IsNotLoggedIn)); } }

        private double _balance;
        public double Balance { get => _balance; set { Set(ref _balance, value); } }

        private double _minutes;
        public double Minutes { get => _minutes; set { Set(ref _minutes, value); } }

        public ObservableCollection<GameViewModel> Games { get; } = new ObservableCollection<GameViewModel>();

        private string _status;
        public string Status { get => _status; set { Set(ref _status, value); } }

        public async Task DownloadGamesAsync()
        {
            try
            {
                StartProcess("Downloading");

                var response = await Client.GetAsync($"Games/{Minutes}");
                if (response.IsSuccessStatusCode)
                {
                    var models = await response.Content.ReadAsAsync<List<GameModel>>();
                    foreach (var game in models.Select(GameViewModel.Create))
                        Games.Add(game);
                }
                else
                {
                    await ReportErrorAsync();
                }
            }
            finally
            {
                StopProcess();
            }
        }

        public async Task LogInAsync()
        {
            try
            {
                StartProcess("Logging in");

                var response = await Client.PostAsJsonAsync($"Login", new { Username, Password });

                IsLoggedIn = response.IsSuccessStatusCode;
            }
            finally
            {
                StopProcess();
            }
        }

        private void StartProcess(string status) => Status = status;
        private void StopProcess() => Status = string.Empty;
        private async Task ReportErrorAsync()
        {
            StartProcess("Error downloading");
            await Task.Delay(1000);
        }
    }
}
