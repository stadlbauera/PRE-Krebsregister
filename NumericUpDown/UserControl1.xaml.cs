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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NumericUpDown
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public const string PfeilObenAsci = "\u2014"; // &#9650";
        public UserControl1()
        {
           
            InitializeComponent();

            drawPfeilDOWN();
        }

        private void drawPfeilDOWN()
        {
            Polyline pl = new Polyline();
            pl.Stroke = Brushes.Black;
            pl.Fill = Brushes.Black;
            pl.Points.Add(new Point(canDOWN.ActualHeight / 2 + canDOWN.ActualHeight / 2 * 0.3, canDOWN.ActualWidth/2));
            pl.Points.Add(new Point(canDOWN.ActualHeight / 2 - canDOWN.ActualHeight / 2 * 0.3, canDOWN.ActualWidth / 2 + canDOWN.ActualWidth/2 * 0.3));
            pl.Points.Add(new Point(canDOWN.ActualHeight / 2 - canDOWN.ActualHeight / 2 * 0.3, canDOWN.ActualWidth / 2 - canDOWN.ActualWidth / 2 * 0.3));

            canDOWN.Children.Add(pl);
        }

        private void bUP_Click(object sender, RoutedEventArgs e)
        {
            int anzahl = Int32.Parse(tbAnzahl.Text);
            tbAnzahl.Text = "" + anzahl++;
        }

        private void bDOWN_Click(object sender, RoutedEventArgs e)
        {
            int anzahl = Int32.Parse(tbAnzahl.Text);
            if (anzahl > 1) tbAnzahl.Text = "" + anzahl--;
        }

        private void canDOWN_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            drawPfeilDOWN();
        }
    }
}