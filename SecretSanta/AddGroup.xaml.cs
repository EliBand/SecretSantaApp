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
    /// Interaction logic for AddGroup.xaml
    /// </summary>
    public partial class AddGroup : Window
    {
        public MainWindow Main { get; set; }
        public AddGroup(MainWindow main)
        {
            InitializeComponent();
            Main = main;
        }

        private void btn_addGroup_Click(object sender, RoutedEventArgs e)
        {
            Main.saveGroup(tb_newGroup.Text);
        }
    }
}
