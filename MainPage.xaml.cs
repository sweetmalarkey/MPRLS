﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MPRLS_Driver
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        MPRLS mPRLS;
        DispatcherTimer timer;
        
        public MainPage()
        {
            this.InitializeComponent();
            mPRLS = new MPRLS();

            InitalizeSensor();

            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);

            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {           

            var measurement = mPRLS.ReadPressure();
            PressureDislay.Text = measurement.ToString();

            Debug.WriteLine(measurement);
        }

        public async void InitalizeSensor()
        {
            await mPRLS.Initialize();
        }

        
    }
}
