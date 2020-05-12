using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TrackingSample.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using System.Reflection;

namespace TrackingSample
{
    public partial class MainPage : ContentPage
    {
        

        public MainPage()
        {
            InitializeComponent();

           
           
        }

       
       

        public async void OnEnterAddressTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SearchPlacePage() { BindingContext = this.BindingContext }, false);

        }

        public void Handle_Stop_Clicked(object sender, EventArgs e)
        {
            searchLayout.IsVisible = true;
            stopRouteButton.IsVisible = false;
            map.Polylines.Clear();
            map.Pins.Clear();
        }

        //Center map in actual location 
        protected override void OnAppearing()
        {
            base.OnAppearing();
            map.GetActualLocationCommand.Execute(null);
        }

        void OnCalculate(System.Object sender, System.EventArgs e)
        {
            searchLayout.IsVisible = false;
            stopRouteButton.IsVisible = true;
        }

        async void OnLocationError(System.Object sender, System.EventArgs e)
        {
            await DisplayAlert("Error", "Unable to get actual location", "Ok");
        }



    }
}
