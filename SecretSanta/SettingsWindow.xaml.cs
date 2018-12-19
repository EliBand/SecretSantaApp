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

namespace SecretSanta
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public MainWindow Main { get; set; }
        public SettingsWindow()
        {
            InitializeComponent();
        }
        public SettingsWindow(string server, string user, string pw, string address) : this()
        {
            tb_mailServer.Text = server;
            tb_mailClient.Text = user;
            pb_mailPassword.Password = pw;
            tb_mailAddress.Text = address;
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
