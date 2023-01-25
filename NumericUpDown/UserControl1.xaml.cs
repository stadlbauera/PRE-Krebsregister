using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private int nudContent;
        [Category("NumericUpDown"),Description("Text vom TextBlock")]
        public int NudContent
        {
            get {
                nudContent = Int32.Parse(tbAnzahl.Text);
                return nudContent; }
            set { nudContent = value;
                tbAnzahl.Text = "" + nudContent; 
            }
        }

        private Boolean isAnzahl;

        public Boolean IsAnzahl
        {
            get { return isAnzahl; }
            set { isAnzahl = value; }
        }


        public UserControl1()
        {
           
            InitializeComponent();
            tbAnzahl.Text = ""+1;
            drawPfeilDOWN();
        }

        private void drawPfeilDOWN()
        {
            Polygon pg = new Polygon();
            pg.Fill = Brushes.Black;
            pg.Points.Add(new Point(canDOWN.ActualHeight / 2 + canDOWN.ActualHeight / 2 * 0.3, canDOWN.ActualWidth / 2));
            pg.Points.Add(new Point(canDOWN.ActualHeight / 2 - canDOWN.ActualHeight / 2 * 0.3, canDOWN.ActualWidth / 2 + canDOWN.ActualWidth / 2 * 0.3));
            pg.Points.Add(new Point(canDOWN.ActualHeight / 2 - canDOWN.ActualHeight / 2 * 0.3, canDOWN.ActualWidth / 2 - canDOWN.ActualWidth / 2 * 0.3));

            canDOWN.Children.Add(pg);
        }

        private void bUP_Click(object sender, RoutedEventArgs e)
        {
            int anzahl = Int32.Parse(tbAnzahl.Text);

            if (!isAnzahl && anzahl == DateTime.Now.Year) anzahl--;
            anzahl++;
            tbAnzahl.Text = "" + anzahl;
        }

        private void bDOWN_Click(object sender, RoutedEventArgs e)
        {
            int anzahl = Int32.Parse(tbAnzahl.Text);
            anzahl--;
            if (anzahl >= 1) tbAnzahl.Text = "" + anzahl;
        }

        private void canDOWN_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            drawPfeilDOWN();
        }
    }
}