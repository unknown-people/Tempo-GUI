using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TempoWithGUI.MVVM.ViewModel;

namespace TempoWithGUI.MVVM.View.RaidView
{
    /// <summary>
    /// Interaction logic for GuildRaid.xaml
    /// </summary>
    public partial class GuildRaid : Window
    {
        public GuildRaid()
        {
            InitializeComponent();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            JoinGuild.joining = false;
            SpamGuild.spamming = false;
            RaidModel.guildOn = false;
            this.Hide();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
