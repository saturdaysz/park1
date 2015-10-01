using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int answer;

        public MainPage()
        {
            this.InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += timer_tick;
            Random rand1 = new Random();
            answer = rand1.Next(1, 75);
        }

        private void timer_tick(object sender, object e)
        {
            Random rand1 = new Random();
            answer = rand1.Next(1, 75);
            List<Data> data = new List<Data>();
            data.Add(new Data() { Category = rand1.ToString(), Value = answer });

            this.Graph1.DataContext = data;


        }
        public class Data
        {
            public string Category { get; set; }

            public double Value { get; set; }
        }
    }
}
