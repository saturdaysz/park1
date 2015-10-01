using Sensors.Dht;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
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

        private const int LED_PIN = 25;
        private GpioPin ledpin;
        private const int LED_PIN2 = 13;
        private GpioPin ledpin2;
        public MainPage()
        {
            this.InitializeComponent();

            this._timer.Interval = TimeSpan.FromSeconds(1);
            Lightsetup();


            _timer.Tick += _timer_Tick;
            _timer.Tick += _timer_Tick2;
            _timer.Tick += _timer_Tick1;
            _timer.Tick += _timer_Tick3;

           





        }

        public class DataItem
        {
            public string Category { get; set; }
            public double Value { get; set; }
            public float Value1 { get; internal set; }
            public float Value2 { get; internal set; }
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
                lighttext.Text = "SPI DEVICE BOT FOUND";

            }
        }


        // MCP3008 READ AND LIGHT TEXT SHOW
        private void _timer_Tick3(object sender, object e)
        {
            var transmitBuffer = new byte[3] { 1, 0x80, 0x00 };
            var reciveBuffer = new byte[3];
            _mcp3008.TransferFullDuplex(transmitBuffer, reciveBuffer);
            int result = ((reciveBuffer[1] & 3) << 8);
            result += reciveBuffer[2];

            var mv = result;
            var light = Convert.ToSingle(mv);

            string output = light + " c";
            if (light <= 300)
            {
                ledpin2.Write(GpioPinValue.High);


            }
            else
            {
                ledpin2.Write(GpioPinValue.Low);


            }
            ledpin2.SetDriveMode(GpioPinDriveMode.Output);
            lighttext.Text = output;




            var Client = new HttpClient();
            Client.BaseAddress = new Uri("https://api.thingspeak.com/");
            Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));


            var url = "https://api.thingspeak.com/update?key=923BNP2LN6RHK2U6&field1=" + light;
            HttpResponseMessage response = Client.GetAsync(url).Result;
            lineSeries.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.lineSeries.Series[0].DataContext = new List<DataItem>
            {
                  new DataItem() {  Value = this.Humidity},
                  new DataItem() {  Value = this.Humidity}

            };
            this.lineSeries.Series[1].DataContext = new List<DataItem>
            {
                new DataItem() {Value1 = this.Temperature},
                new DataItem() {Value1 = this.Temperature}
            };
            this.lineSeries.Series[2].DataContext = new List<DataItem>
            {
                new DataItem() {Value2 =light },
                new DataItem() {Value2 =light }   
            };
            //List<Data> data1 = new List<Data>();
            //data1.Add(new Data() { Value = this.Temperature });
            //data1.Add(new Data() { Value = this.Temperature });

            //List<Data> data2 = new List<Data>();
            //data2.Add(new Data() { Value1 = this.Humidity });
            //data2.Add(new Data() { Value1 = this.Humidity });

            //List<Data> data3 = new List<Data>();
            //data3.Add(new Data() { Value2 = light });
            //data3.Add(new Data() { Value2 = light }); 

            //this.lineSeries.Series[0].DataContext = data1;
            //this.lineSeries.Series[1].DataContext = data2;
            //this.lineSeries.Series[2].DataContext = data3;




        }
     
        // TEMPERATURE SHOW
        private void _timer_Tick1(object sender, object e)
        {
            var Client = new HttpClient();
            Client.BaseAddress = new Uri("https://api.thingspeak.com/");
            Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            var url = "https://api.thingspeak.com/update?key=923BNP2LN6RHK2U6&field2=" + this.Temperature;
            HttpResponseMessage response = Client.GetAsync(url).Result;
            temptext.Text = TemperatureDisplay;
            if (this.Temperature > 21)
            {
                ledpin.Write(GpioPinValue.High);


            }
            else
            {
                ledpin.Write(GpioPinValue.Low);


            }
            ledpin.SetDriveMode(GpioPinDriveMode.Output);

            //Random rand1 = new Random();
            //answer = rand1.Next(1, 75);

        }
        // HUMIDITY SHOW
        private void _timer_Tick2(object sender, object e)
        {
            var Client = new HttpClient();
            Client.BaseAddress = new Uri("https://api.thingspeak.com/");
            Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            var url = "https://api.thingspeak.com/update?key=923BNP2LN6RHK2U6&field3=" + this.Humidity;
            HttpResponseMessage response = Client.GetAsync(url).Result;
            humitext.Text = HumidityDisplay;



        }
        // SETUP GPIO PIN
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var gpio = GpioController.GetDefault();
            ledpin = gpio.OpenPin(LED_PIN);
            ledpin2 = gpio.OpenPin(LED_PIN2);
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
        // USE DHT11 REFERENCE TO READING DATA

        private async void _timer_Tick(object sender, object e)
        {
            DhtReading reading = new DhtReading();

            reading = await _dht11.GetReadingAsync().AsTask();

            _retryCount.Add(reading.RetryCount);

            if (reading.IsValid)
            {

                this.Temperature = Convert.ToSingle(reading.Temperature);
                this.Humidity = Convert.ToSingle(reading.Humidity);
            }
        }

        public float Humidity;

        public string HumidityDisplay
        {
            get
            {
                return string.Format("{0:0}% RH", this.Humidity);
            }
        }

        public float Temperature;
        public string TemperatureDisplay
        {
            get
            {
                return string.Format("{0:0} °C", this.Temperature);
            }
        }

        private DateTimeOffset _lastUpdated = DateTimeOffset.MinValue;
        private SpiDevice _mcp3008;

        public class Data
        {
            public string Category { get; set; }

            public double Value { get; set; }
            public double Value1 { get; set; }
            public double Value2 { get; set; }
        }
    }
}
