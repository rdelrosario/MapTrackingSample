using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace TrackingSample.Controls
{
    public class ExtendedMap : Xamarin.Forms.GoogleMaps.Map
    {
        public static readonly BindableProperty CalculateCommandProperty =
            BindableProperty.Create(nameof(CalculateCommand), typeof(ICommand), typeof(MainPage), null, BindingMode.TwoWay);

        public ICommand CalculateCommand
        {
            get { return (ICommand)GetValue(CalculateCommandProperty); }
            set { SetValue(CalculateCommandProperty, value); }
        }

        public static readonly BindableProperty UpdateCommandProperty =
          BindableProperty.Create(nameof(UpdateCommand), typeof(ICommand), typeof(MainPage), null, BindingMode.TwoWay);

        public ICommand UpdateCommand
        {
            get { return (ICommand)GetValue(UpdateCommandProperty); }
            set { SetValue(UpdateCommandProperty, value); }
        }

        public static readonly BindableProperty GetActualLocationCommandProperty =
          BindableProperty.Create(nameof(GetActualLocationCommand), typeof(ICommand), typeof(MainPage), null, BindingMode.TwoWay);

        public ICommand GetActualLocationCommand
        {
            get { return (ICommand)GetValue(GetActualLocationCommandProperty); }
            set { SetValue(GetActualLocationCommandProperty, value); }
        }

        public event EventHandler OnCalculate = delegate { };
        public event EventHandler OnLocationError = delegate {};

        public ExtendedMap()
        {

            AddMapStyle();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if(BindingContext !=null)
            {
                CalculateCommand = new Command<List<Position>>(Calculate);
                UpdateCommand = new Command<Position>(Update);
                GetActualLocationCommand = new Command(async () => await GetActualLocation());
            }
        }

        void AddMapStyle()
        {
            var assembly = typeof(ExtendedMap).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream($"TrackingSample.MapStyle.json");
            string styleFile;
            using (var reader = new System.IO.StreamReader(stream))
            {
                styleFile = reader.ReadToEnd();
            }

            MapStyle = MapStyle.FromJson(styleFile);
        }


        async void Update(Xamarin.Forms.GoogleMaps.Position position)
        {
            if (Pins.Count == 1 && Polylines != null && Polylines?.Count > 1)
                return;

            var cPin = Pins.FirstOrDefault();

            if (cPin != null)
            {
                cPin.Position = new Position(position.Latitude, position.Longitude);
                cPin.Icon = (Device.RuntimePlatform == Device.Android) ? BitmapDescriptorFactory.FromBundle("ic_taxi.png") : BitmapDescriptorFactory.FromView(new Image() { Source = "ic_taxi.png", WidthRequest = 25, HeightRequest = 25 });

                await MoveCamera(CameraUpdateFactory.NewPosition(new Position(position.Latitude, position.Longitude)));
                var previousPosition = Polylines?.FirstOrDefault()?.Positions?.FirstOrDefault();
                Polylines?.FirstOrDefault()?.Positions?.Remove(previousPosition.Value);
            }
            else
            {
                //END TRIP
                Polylines?.FirstOrDefault()?.Positions?.Clear();
            }


        }

        void Calculate(List<Xamarin.Forms.GoogleMaps.Position> list)
        {
            OnCalculate?.Invoke(this, default(EventArgs));
            Polylines.Clear();
            var polyline = new Xamarin.Forms.GoogleMaps.Polyline();
            foreach (var p in list)
            {
                polyline.Positions.Add(p);

            }
            Polylines.Add(polyline);
            MoveToRegion(MapSpan.FromCenterAndRadius(new Position(polyline.Positions[0].Latitude, polyline.Positions[0].Longitude), Xamarin.Forms.GoogleMaps.Distance.FromMiles(0.50f)));

            var pin = new Xamarin.Forms.GoogleMaps.Pin
            {
                Type = PinType.Place,
                Position = new Position(polyline.Positions.First().Latitude, polyline.Positions.First().Longitude),
                Label = "First",
                Address = "First",
                Tag = string.Empty,
                Icon = (Device.RuntimePlatform == Device.Android) ? BitmapDescriptorFactory.FromBundle("ic_taxi.png") : BitmapDescriptorFactory.FromView(new Image() { Source = "ic_taxi.png", WidthRequest = 25, HeightRequest = 25 })

            };
            Pins.Add(pin);
            var pin1 = new Xamarin.Forms.GoogleMaps.Pin
            {
                Type = PinType.Place,
                Position = new Position(polyline.Positions.Last().Latitude, polyline.Positions.Last().Longitude),
                Label = "Last",
                Address = "Last",
                Tag = string.Empty
            };
            Pins.Add(pin1);
        }

        async Task GetActualLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    MoveToRegion(MapSpan.FromCenterAndRadius(
                        new Position(location.Latitude, location.Longitude), Distance.FromMiles(0.3)));

                }
            }
            catch (Exception ex)
            {
                OnLocationError?.Invoke(this, default(EventArgs));
            }
        }


    }
}
