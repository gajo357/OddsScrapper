using System;
using Xamarin.Forms;

namespace MoneyMaker
{
    public partial class MainPage : ContentPage
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();
        public MainPage()
        {
            InitializeComponent();

            BindingContext = ViewModel;
        }

        private async void LoginButton_Clicked(object sender, EventArgs e) => await ViewModel.LogInAsync();

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            await Navigation.PushModalAsync(new GamePage() { BindingContext = e.SelectedItem });
        }
    }
}
