using System;
using System.Collections.Generic;
using TrackingSample.Models;
using TrackingSample.ViewModels;
using Xamarin.Forms;

namespace TrackingSample.Views
{
    public partial class SearchPlacePage : ContentPage
    {
        public SearchPlacePage()
        {
            InitializeComponent();
        }

        public async void Handle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(sender is Entry entry)
            {
                //To detect if the entry is from origin or destination
                list.ClassId = entry.ClassId;

                var places = await (this.BindingContext as MainViewModel).GetPlacesByName(entry.Text);
                if (places != null && places.Count > 0)
                {
                    list.ItemsSource = places;
                    list.Header = null;
                }
                else
                {
                    list.ItemsSource = (this.BindingContext as MainViewModel).RecentPlaces;
                    list.Header = recentSearchText;
                }
            }
        }

        public async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (sender is Xamarin.Forms.ListView list && e.Item is GooglePlaceAutoCompletePrediction item && this.BindingContext is MainViewModel viewmodel)
            {
                var place = await viewmodel.GetPlacesDetail(item, list.ClassId);

                if (list.ClassId == "origin" || list.ClassId==null)
                {
                    originEntry.Text = place.Name;
                    list.ClassId = "destination";
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        destinationEntry.Focus();
                    });
                }
                else
                {
                    if (viewmodel.OriginLatitud == viewmodel.DestinationLatitud && viewmodel.OriginLongitud == viewmodel.DestinationLongitud)
                    {
                        await App.Current.MainPage.DisplayAlert("Error", "Origin route should be different than destination route", "Ok");
                    }
                    else
                    {
                        viewmodel.LoadRouteCommand.Execute(null);
                        await Navigation.PopAsync(false);
                    }
                   
                }
            }
        }

    }
}
