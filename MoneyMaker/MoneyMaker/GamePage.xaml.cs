using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MoneyMaker
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GamePage : ContentPage
	{
		public GamePage ()
		{
			InitializeComponent ();
		}

        private async void Button_Clicked(object sender, EventArgs e) => await Navigation.PopModalAsync();
    }
}