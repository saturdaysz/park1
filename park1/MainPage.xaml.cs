
using Windows.UI.Xaml.Controls;
using Sensors.Dht;
using Windows.UI.Xaml;
using Windows.Devices.Gpio;

using System.Collections.Generic;
using System;
using Windows.UI.Xaml.Navigation;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace park1
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
        private DateTimeOffset _startedAt = DateTime.MinValue;
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

            _startedAt = DateTimeOffset.Now;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _timer.Stop();

            _pin.Dispose();
            _pin = null;

            _dht11 = null;

            base.OnNavigatedFrom(e);
        }

        private void _timer_Tick(object sender, object e)
        {
            throw new NotImplementedException();
        }
    }
}
