using LiveCharts.Wpf;
using LiveCharts;
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
using System.Data;
using System.IO;
using System.Net;
using System.Data.SqlClient;

namespace Krebsregister
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        //const string constring = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\lilia\\source\\repos\\stadlbauera\\PRE-Krebsregister\\Krebsregister\\Krebsregister_Database.mdf;Integrated Security=True";
        //const string path_rest_icd10 = "C:\\Users\\lilia\\Source\\Repos\\stadlbauera\\PRE-Krebsregister\\Krebsregister\\CSV-Dateien\\restlicheICD10Codes.csv";

        public MainWindow()
        {
           


            InitializeCharts();
            DataContext = this;
            InitializeComponent();
            
        }

        private void InitializeCharts()
        {
            PieChart();
            BarChart();
            AreaChart();
            NegativStackChart();
            GeoMap();
        }

        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //DatabaseMethods.FillDatabase();
        }

        #region GeoMap

        public Dictionary<string, double> MapData { get; set; } = new Dictionary<string, double>();

        public void GeoMap()
        {
            MapData = new Dictionary<string, double>()
            {
                // Welche Id zu welchem Bundesland gehört, findet ihr in der maps/austria.xml Datei
                { "2273",20.0 },
                { "2274",100.0 },
                { "2275",40.0 },
                { "2276",10.0 },
                { "2277",30.0 },
                { "2278",60.0 },
                { "2280",30.0 },
                { "2281",70.0 },
                { "2282",50.0 }
            };

            //InitializeComponent();
            //LiveCharts.Wpf.GeoMap geoMap = new LiveCharts.Wpf.GeoMap();
            //Random r = new Random();
            //Dictionary<string, double> map = new Dictionary<string, double>();
            //map["W"] = r.Next(0, 100);
            //map["OOE"] = r.Next(0, 100);
            //map["NOE"] = r.Next(0, 100);
            //map["BGL"] = r.Next(0, 100);
            //map["SBG"] = r.Next(0, 100);
            //map["KTN"] = r.Next(0, 100);
            //map["TIR"] = r.Next(0, 100);
            //map["VAB"] = r.Next(0, 100);
            //map["SMK"] = r.Next(0, 100);
            //geoMap.HeatMap = map;
            //geoMap.Source = @"Austria.xml";

            //DataContext = this;
        }

        #endregion

        #region PieChart
        public Func<ChartPoint, string> PointLabel { get; set; }
        public void PieChart()
        {
            PointLabel = chartPoint =>
                string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            DataContext = this;
        }

        public void PieChart_DataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (LiveCharts.Wpf.PieChart)chartpoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }

        #endregion

        #region NegativStackChart
        public SeriesCollection SeriesCollectionNSC { get; set; }
        public string[] LabelsNSC { get; set; }
        public Func<double, string> FormatterNSC { get; set; }
        private void NegativStackChart()
        {
            SeriesCollectionNSC = new SeriesCollection
            {
                new StackedRowSeries
                {
                    Title = "Male",
                    Values = new ChartValues<double> {.5, .7, .8, .8, .6, .2, .6}
                },
                new StackedRowSeries
                {
                    Title = "Female",
                    Values = new ChartValues<double> {-.5, -.7, -.8, -.8, -.6, -.2, -.6}
                }
            };

            LabelsNSC = new[] { "0-20", "20-35", "35-45", "45-55", "55-65", "65-70", ">70" };
            FormatterNSC = value => Math.Abs(value).ToString("P");

            DataContext = this;
        }

        #endregion

        #region AreaChart
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        private void AreaChart()
        {
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 4, 6, 5, 2 ,4 }
                },
                new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> { 6, 7, 3, 4 ,6 },
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Series 3",
                    Values = new ChartValues<double> { 4,2,7,2,7 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }
            };

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            YFormatter = value => value.ToString("C");

            //modifying the series collection will animate and update the chart
            SeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });

            //modifying any series values will also animate and update the chart
            SeriesCollection[3].Values.Add(5d);

            DataContext = this;
        }

        #endregion

        #region BarChart
        public SeriesCollection SeriesCollectionBC { get; set; }
        public string[] LabelsBC { get; set; }
        public Func<double, string> Formatter { get; set; }
        private void BarChart()
        {
            SeriesCollectionBC = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "2015",
                    Values = new ChartValues<double> { 10, 50, 39, 50 }
                }
            };

            //adding series will update and animate the chart automatically
            SeriesCollectionBC.Add(new ColumnSeries
            {
                Title = "2016",
                Values = new ChartValues<double> { 11, 56, 42 }
            });

            //also adding values updates and animates the chart automatically
            SeriesCollectionBC[1].Values.Add(48d);

            LabelsBC = new[] { "Maria", "Susan", "Charles", "Frida" };
            Formatter = value => value.ToString("N");

            DataContext = this;
        }
        #endregion

        #region Krebsmeldung

        public MainWindow(Krebsmeldung neueKrebsmeldung, bool erstellen)
        {
            
            InitializeCharts();
            InitializeComponent();

            lblException.Content = "";
            if (erstellen)
            {
                //Bestätigungs-Fenster
            }
            else
            {
                if(neueKrebsmeldung != null)
                {
                    cbKrebsart.Text = $"{neueKrebsmeldung.ICD10Code} - {neueKrebsmeldung.Krebsart}";
                    cbGeschlecht.Text = neueKrebsmeldung.Geschlecht;
                    cbBundesland.Text = neueKrebsmeldung.Bundesland;
                    nudJahr.NudContent = neueKrebsmeldung.Jahr;
                    nudAnzahl.NudContent = neueKrebsmeldung.Anzahl;
                }
            }
            tiKrebsmeldung.IsSelected = true;
            DataContext = this;
            
        }

        private void bNeueKrebsmeldung_Click(object sender, RoutedEventArgs e)
        {
            lblException.Content = "";
            if(cbKrebsart.Text.Equals("") || cbGeschlecht.Text.Equals("") || cbBundesland.Text.Equals(""))
            {
                lblException.Content = "Bitte füllen Sie alle Felder aus!";
            }
            else
            {
                Krebsmeldung neueKrebsmeldung = new Krebsmeldung
                {
                    Krebsart = cbKrebsart.Text.Split(" - ")[1],
                    ICD10Code = cbKrebsart.Text.Split(" - ")[0],
                    Geschlecht = cbGeschlecht.Text,
                    Bundesland = cbBundesland.Text,
                    Anzahl = nudAnzahl.NudContent,
                    Jahr = nudJahr.NudContent
                };
                Window KrebsmeldungConfirm = new KrebsmeldungConfirm(neueKrebsmeldung);
                KrebsmeldungConfirm.Show();
                this.Close();
            }
        }

        #endregion
    }
}
