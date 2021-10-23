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
using TempoWithGUI.MVVM.View.ProfilesView;

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for ProfilesRaid.xaml
    /// </summary>
    public partial class ProfilesRaid : Window
    {
        public ProfilesRaid()
        {
            InitializeComponent();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            imageProfiles.isChanging = false;
            this.Hide();
        }
    }
}
