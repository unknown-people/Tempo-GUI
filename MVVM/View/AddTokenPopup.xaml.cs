using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for AddTokenPopup.xaml
    /// </summary>
    public partial class AddTokenPopup : Window
    {
        public AddTokenPopup()
        {
            InitializeComponent();
            Debug.Log("Opened add token popup");
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            var token = TokenIn.Text;
            var email = EmailIn.Text;
            var password = PasswordIn.Text;
            var creation = CreationIn.Text;

            if (token == null || token == "")
                return;
            else if (email == null)
                email = "";
            else if (password == null)
                password = "";
            else if (creation == null)
                creation = "";

            var exec = "U:" + token + ":" + email + ":" + password + ":" + creation + ":NULL";
            using (StreamWriter stream = new StreamWriter(App.strWorkPath + "\\tokens\\tokens.txt", true))
            {
                stream.WriteLine(exec);
            }
            this.Hide();;
        }
    }
}
