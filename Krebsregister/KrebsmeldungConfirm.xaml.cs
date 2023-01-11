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
    /// Interaction logic for KrebsmeldungConfirm.xaml
    /// </summary>
    public partial class KrebsmeldungConfirm : Window
    {

        public Krebsmeldung neueKrebsmeldung { get; set; }
        public KrebsmeldungConfirm(Krebsmeldung neueKrebsmeldung)
        {
            InitializeComponent();
            this.neueKrebsmeldung = neueKrebsmeldung;
            lblKrebsart.Content = neueKrebsmeldung.Krebsart;
            lblGeschlecht.Content = neueKrebsmeldung.Geschlecht;
            lblBundesland.Content = neueKrebsmeldung.Bundesland;
            lblJahr.Content = neueKrebsmeldung.Jahr;
            lblAnzahl.Content = neueKrebsmeldung.Anzahl;
        }

        public KrebsmeldungConfirm()
        {
            InitializeComponent();
        }

        private void bZurueck_Click(object sender, RoutedEventArgs e)
        {
            Window MainWindow = new MainWindow(neueKrebsmeldung, false);
            MainWindow.Show();
            this.Close();
        }

        private void bAbbrechen_Click(object sender, RoutedEventArgs e)
        {
            neueKrebsmeldung = null;
            Window MainWindow = new MainWindow(neueKrebsmeldung, false);
            MainWindow.Show();
            this.Close();
        }

        private void bNeueKrebsmeldung_Click(object sender, RoutedEventArgs e)
        {
            Window Confirmation = new Confirmation(neueKrebsmeldung);
            Confirmation.Show();
            this.Close();
        }
    }
}
