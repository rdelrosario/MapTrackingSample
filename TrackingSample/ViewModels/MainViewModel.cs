using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TrackingSample.Helpers;
using TrackingSample.Models;
using TrackingSample.Services;
using Xamarin.Forms;

namespace TrackingSample.ViewModels
{
    public class MainViewModel: INotifyPropertyChanged
    {
        public ICommand CalculateRouteCommand { get; set; }
        public ICommand UpdatePositionCommand { get; set; }

        public ICommand LoadRouteCommand { get; set; }
        public ICommand StopRouteCommand { get; set; }
        IGoogleMapsApiService googleMapsApi = new GoogleMapsApiService();

        public event PropertyChangedEventHandler PropertyChanged;

        bool hasRouteRunning = false;
        public ObservableCollection<GooglePlaceAutoCompletePrediction> RecentPlaces { get; set; } = new ObservableCollection<GooglePlaceAutoCompletePrediction>();
        public string OriginLatitud;
        public string OriginLongitud;
        public string DestinationLatitud;
        public string DestinationLongitud;
         
        public MainViewModel()
        {
            LoadRouteCommand = new Command(async () => await LoadRoute());
            StopRouteCommand = new Command(StopRoute);
        }

        public async Task LoadRoute()
        {
            var positionIndex = 1;
            var googleDirection = await googleMapsApi.GetDirections(OriginLatitud, OriginLongitud, DestinationLatitud, DestinationLongitud);
            if(googleDirection.Routes!=null && googleDirection.Routes.Count>0)
            {
                var positions = (Enumerable.ToList(PolylineHelper.Decode(googleDirection.Routes.First().OverviewPolyline.Points)));
                CalculateRouteCommand.Execute(positions);

                hasRouteRunning = true;

                //Location tracking simulation
                Device.StartTimer(TimeSpan.FromSeconds(1),() =>
                {
                    if(positions.Count>positionIndex && hasRouteRunning)
                    {
                        UpdatePositionCommand.Execute(positions[positionIndex]);
                        positionIndex++;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
            }
            else
            {
                await App.Current.MainPage.DisplayAlert(":(", "No route found", "Ok");
            }

        }
        public void StopRoute()
        {
            hasRouteRunning = false;
        }

        public async Task<List<GooglePlaceAutoCompletePrediction>> GetPlacesByName(string placeText)
        {
            var place = await googleMapsApi.GetPlaces(placeText);
            return place.AutoCompletePlaces;
        }

        public async Task<GooglePlace> GetPlacesDetail(GooglePlaceAutoCompletePrediction placeA, string route)
        {
            var place = await googleMapsApi.GetPlaceDetails(placeA.PlaceId);
            if (place != null)
            {
                if (route == "origin")
                {
                    OriginLatitud = $"{place.Latitude}";
                    OriginLongitud = $"{place.Longitude}";
                }
                else
                {
                    DestinationLatitud = $"{place.Latitude}";
                    DestinationLongitud = $"{place.Longitude}";

                    RecentPlaces.Add(placeA);
                }
            }
            return place;
        }

    }
}
