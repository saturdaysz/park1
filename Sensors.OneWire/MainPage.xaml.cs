using System;
using System.Collections.Generic;
using System.Linq;
using Sensors.Dht;
using Sensors.OneWire.Common;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;

namespace Sensors.OneWire
{
    public sealed partial class MainPage : BindablePage
    {
        private DispatcherTimer _timer = new DispatcherTimer();

        GpioPin _pin = null;
        private IDht _dht11 = null;
        private List<int> _retryCount = new List<int>();

        private const int LED_PIN = 25;
        private GpioPin ledpin;
        public MainPage()
        {
            this.InitializeComponent();

            _timer.Interval = TimeSpan.FromSeconds(1);
            Lightsetup();
          
            _timer.Tick += _timer_Tick;
            _timer.Tick += _timer_Tick2;
            _timer.Tick += _timer_Tick1;
            _timer.Tick += _timer_Tick3;

        }

        private async void Lightsetup()
        {
            var spiSettings = new SpiConnectionSettings(0);
            spiSettings.ClockFrequency = 3600000;
            spiSettings.Mode = SpiMode.Mode0;
            string spiQuery = SpiDevice.GetDeviceSelector("SPI0");

            var deviceInfo = await DeviceInformation.FindAllAsync(spiQuery);
            if (deviceInfo != null && deviceInfo.Count > 0)
            {
                _mcp3008 = await SpiDevice.FromIdAsync(deviceInfo[0].Id, spiSettings);


            }


            else
            {
                textblocklight.Text = "SPI DEVICE BOT FOUND";

            }


            
        }

        private void _timer_Tick3(object sender, object e)
        {
            var transmitBuffer = new byte[3] { 1, 0x80, 0x00 };
            var reciveBuffer = new byte[3];
            _mcp3008.TransferFullDuplex(transmitBuffer, reciveBuffer);
            int result = ((reciveBuffer[1] & 3) << 8);
            result += reciveBuffer[2];

            var mv = result;
            var light = Convert.ToSingle(mv);
       
            string output = light + "c";
            if (light <= 400)
            {
                ledpin.Write(GpioPinValue.High);
            }
            else
            {
                ledpin.Write(GpioPinValue.Low);
            }
            ledpin.SetDriveMode(GpioPinDriveMode.Output);
            textblocklight.Text = output;


            var Client = new HttpClient();
            Client.BaseAddress = new Uri("https://api.thingspeak.com/");
            Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));


            var url = "https://api.thingspeak.com/update?key=923BNP2LN6RHK2U6&field1=" + light;
            HttpResponseMessage response = Client.GetAsync(url).Result;


          
        }

        private void _timer_Tick1(object sender, object e)
        {
            var Client = new HttpClient();
            Client.BaseAddress = new Uri("https://api.thingspeak.com/");
            Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            var url = "https://api.thingspeak.com/update?key=923BNP2LN6RHK2U6&field2=" + this.Temperature;
            HttpResponseMessage response = Client.GetAsync(url).Result;
        }

        private void _timer_Tick2(object sender, object e)
        {
            var Client = new HttpClient();
            Client.BaseAddress = new Uri("https://api.thingspeak.com/");
            Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            var url = "https://api.thingspeak.com/update?key=923BNP2LN6RHK2U6&field3=" + this.Humidity ;
            HttpResponseMessage response = Client.GetAsync(url).Result;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var gpio = GpioController.GetDefault();
            ledpin = gpio.OpenPin(LED_PIN);
            _pin = gpio.OpenPin(4, GpioSharingMode.Exclusive);
            _dht11 = new Dht11(_pin, GpioPinDriveMode.Input);
            _timer.Start();
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

                this.Temperature = Convert.ToSingle(reading.Temperature);
                this.Humidity = Convert.ToSingle(reading.Humidity);
                this.LastUpdated = DateTimeOffset.Now;

            }

            this.OnPropertyChanged(nameof(LastUpdatedDisplay));
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
                this.SetProperty(ref _humidity, value);
                this.OnPropertyChanged(nameof(HumidityDisplay));
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
                this.SetProperty(ref _temperature, value);
                this.OnPropertyChanged(nameof(TemperatureDisplay));
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
        private SpiDevice _mcp3008;

        public DateTimeOffset LastUpdated
        {
            get
            {
                return _lastUpdated;
            }
            set
            {
                this.SetProperty(ref _lastUpdated, value);
                this.OnPropertyChanged(nameof(LastUpdatedDisplay));
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