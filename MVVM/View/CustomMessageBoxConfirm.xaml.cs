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

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for CustomMessageBoxConfirm.xaml
    /// </summary>
    public partial class CustomMessageBoxConfirm : Window
    {
        public bool hasConfirmed { get; set; } = false;
        public CustomMessageBoxConfirm()
        {
            InitializeComponent();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            hasConfirmed = true;
            this.Close();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
