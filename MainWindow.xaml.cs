using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using System.Security.Principal;
using Discord;
using Discord.Gateway;
using Discord.Media;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Timers;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.AccessControl;
using Music_user_bot;
using System.ServiceProcess;
using YoutubeExplode;
using TempoWithGUI.MVVM.ViewModel;
using TempoWithGUI.MVVM.View;

namespace TempoWithGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (MainViewModel.log == null)
                MainViewModel.log = new Log();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if(TrackQueue.currentSong != null)
                TrackQueue.currentSong.CancellationTokenSource.Cancel();
            if(App.mainClient != null)
                App.mainClient.Dispose();
            this.Hide();;
            Application.Current.Shutdown();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Logs_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.log.Show();
            MainViewModel.log.Activate();
        }
    }
}
