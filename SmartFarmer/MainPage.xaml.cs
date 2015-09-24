using Sensors.Dht;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartFarmer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer _timer = new DispatcherTimer();

        GpioPin _pin = null;
        private IDht _dht11 = null;
        private List<int> _retryCount = new List<int>();

   

        public MainPage()
        {
            this.InitializeComponent();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _pin = GpioController.GetDefault().OpenPin(4, GpioSharingMode.Exclusive);
            _dht11 = new Dht11(_pin, GpioPinDriveMode.Input);

            _timer.Start();

            //_startedAt = DateTimeOffset.Now;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _timer.Stop();

            _pin.Dispose();
            _pin = null;

            _dht11 = null;

            base.OnNavigatedFrom(e);
        }
        private async void _timer_Tick(object sender, object e)
        {
            DhtReading reading = new DhtReading();
            reading = await _dht11.GetReadingAsync().AsTask();

            _retryCount.Add(reading.RetryCount);
            if (reading.IsValid)
            {
                //this.TotalSuccess++;
                this.Temperature = Convert.ToSingle(reading.Temperature);
                this.Humidity = Convert.ToSingle(reading.Humidity);
                this.LastUpdated = DateTimeOffset.Now;
                //this.OnPropertyChanged(nameof(SuccessRate));
            }
        }
        private float _humidity = 0f;
        public float Humidity
        {
            get
            {
                return _humidity;
            }

            set
            {
               Humiditybox.Text = HumidityDisplay.ToString();
            }
        }
        public string HumidityDisplay
        {
            get
            {
                return string.Format("{0:0}% RH", this.Humidity);
            }
        }

        private float _temperature = 0f;
        public float Temperature
        {
            get
            {
                return _temperature;
            }
            set
            {
               
                temperaturebox.Text =  TemperatureDisplay.ToString();
            }
        }

        public string TemperatureDisplay
        {
            get
            {
                return string.Format("{0:0} °C", this.Temperature);
            }
        }

        private DateTimeOffset _lastUpdated = DateTimeOffset.MinValue;
        public DateTimeOffset LastUpdated
        {
            get
            {
                return _lastUpdated;
            }
            set
            {
                lastupdatebox.Text = LastUpdatedDisplay.ToString();
            }
        }

        public string LastUpdatedDisplay
        {
            get
            {
                string returnValue = string.Empty;

                TimeSpan elapsed = DateTimeOffset.Now.Subtract(this.LastUpdated);

                if (this.LastUpdated == DateTimeOffset.MinValue)
                {
                    returnValue = "never";
                }
                else if (elapsed.TotalSeconds < 60d)
                {
                    int seconds = (int)elapsed.TotalSeconds;

                    if (seconds < 2)
                    {
                        returnValue = "just now";
                    }
                    else
                    {
                        returnValue = string.Format("{0:0} {1} ago", seconds, seconds == 1 ? "second" : "seconds");
                    }
                }
                else if (elapsed.TotalMinutes < 60d)
                {
                    int minutes = (int)elapsed.TotalMinutes == 0 ? 1 : (int)elapsed.TotalMinutes;
                    returnValue = string.Format("{0:0} {1} ago", minutes, minutes == 1 ? "minute" : "minutes");
                }
                else if (elapsed.TotalHours < 24d)
                {
                    int hours = (int)elapsed.TotalHours == 0 ? 1 : (int)elapsed.TotalHours;
                    returnValue = string.Format("{0:0} {1} ago", hours, hours == 1 ? "hour" : "hours");
                }
                else
                {
                    returnValue = "a long time ago";
                }

                return returnValue;
            }
        }

    }
}
