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

namespace Krebsregister
{
    /// <summary>
    /// Interaction logic for Confirmation.xaml
    /// </summary>
    public partial class Confirmation : Window
    {
        public Krebsmeldung neueKrebsmeldung { get; set; }
        public Confirmation(Krebsmeldung neueKrebsmeldung)
        {
            this.neueKrebsmeldung = neueKrebsmeldung;
            InitializeComponent();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            Window MainWindow = new MainWindow(neueKrebsmeldung, true);
            MainWindow.Show();
            this.Close();
        }
    }
}
