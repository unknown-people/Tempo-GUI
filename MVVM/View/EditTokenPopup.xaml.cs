﻿using System;
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
    public partial class EditTokenPopup : Window
    {
        public EditTokenPopup(string token, string email = "", string password = "", string creation = "")
        {
            InitializeComponent();
            Debug.Log("Opened token editing interface");
            TokenIn.Text = token;
            EmailIn.Text = email;
            PasswordIn.Text = password;
            CreationIn.Text = creation;
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
            Confirm_Btn.Cursor = Cursors.AppStarting;
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

            var exec = token + ":" + email + ":" + password + ":" + creation + ":";
            
            using (StreamWriter stream = new StreamWriter(App.strWorkPath + "\\tokens\\tokens1.txt", true))
            {
                using (StreamReader sr = new StreamReader(App.strWorkPath + "\\tokens\\tokens.txt"))
                {
                    var line = sr.ReadLine();
                    while (line != null)
                    {
                        if(line.Split(':').Length == 3)
                        {
                            if (line.Split(':')[0] == token)
                                stream.WriteLine("U:" + exec + "NULL");
                            else
                                stream.WriteLine(line);
                        }
                        else
                        {
                            if (line.Split(':')[1] == token)
                                stream.WriteLine(line.Split(':')[0] + ":" + exec + line.Split(':')[line.Split(':').Length - 1]);
                            else
                                stream.WriteLine(line);
                        }
                        line = sr.ReadLine();
                    }
                }
            }
            File.Delete(App.strWorkPath + "\\tokens\\tokens.txt");
            File.Move(App.strWorkPath + "\\tokens\\tokens1.txt", App.strWorkPath + "\\tokens\\tokens.txt");
            Confirm_Btn.Cursor = Cursors.Arrow;
            this.Hide();;
        }
    }
}
