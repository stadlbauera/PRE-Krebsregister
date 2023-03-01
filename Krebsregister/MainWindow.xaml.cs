﻿using LiveCharts.Wpf;
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
using System.Diagnostics.Metrics;
using LiveCharts.Definitions.Charts;
using LiveCharts.Wpf.Charts.Base;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Xml;
using Path = System.IO.Path;
using System.Numerics;

namespace Krebsregister
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string constring { get; set; }

        public string path_rest_icd10 { get; set; }

        public string path_xml { get; set; } = "C:\\Users\\lilia\\Source\\Repos\\stadlbauera\\PRE-Krebsregister\\Krebsregister\\Dateien\\Pfade.xml";

        public MainWindow()
        {
            InitializeComponent();

            lblTitleGeoHeatMap.Content = "C00 1994 in Ö";
            lblTitleNegativStackedChart.Content = "Vergleich Männer und Frauen: C00";
            lblTitlePieChart.Content = "C00 Verteilung auf die Bundesländer";
            lblTitleAreaChart.Content = "C00 und C01 im Jahr 1983 - 1989";
            lblTitlePieChart2.Content = "C00 in Oberösterreich über die Jahre";
            lblTitleGridView.Content = "Alle Einträge in der DB";

            DataContext = this;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


            nudJahr.MaxValue = DateTime.Now.Year;
            nudJahr.Value = DateTime.Now.Year;

            nudAnzahl.MinValue = 1;

            FillCharts();
            FillComboBoxes();
        }

        private void GetPaths()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(path_xml);

            XmlNodeList nodeList = xml.GetElementsByTagName("Lili");
            foreach (XmlNode personalNode in nodeList)
            {
                foreach (XmlNode targetNode in personalNode.ChildNodes)
                {
                    if (targetNode.Name == "ConStringPfad")
                    {
                        constring = targetNode.InnerText;
                    }
                    else if (targetNode.Name == "RestICD10Pfad")
                    {
                        path_rest_icd10 = targetNode.InnerText;
                    }
                }
            }
        }

        public List<int> jahreForLiveChart { get; set; }

        private void FillCharts()
        {
            GetPaths();
            //DatabaseMethods.FillDatabase(constring, path_rest_icd10);
            List<Krebsmeldung> list_krebsmeldung = DatabaseMethods.GetDataFromDatabase_Eintrag(constring);


            List<Krebsmeldung> pieChartRelevant = list_krebsmeldung.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals("C00")).ToList();
            PieChart(pieChartRelevant);


            List<Krebsmeldung> negativStackChartRelevant = list_krebsmeldung.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals("C00")).ToList();
            NegativStackChart(negativStackChartRelevant);


            List<Krebsmeldung> barChartRelevant = list_krebsmeldung.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals("C00")).ToList();
            //BarChart(barChartRelevant);

            List<int> jahre = new List<int> { 1983, 1984, 1985, 1986, 1987, 1988, 1989 };
            List<int> anzahlVonC00 = list_krebsmeldung.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals("C00") && (krebsmeldung.Jahr >= 1983 && krebsmeldung.Jahr <= 1989)).ToList().MySum(jahre);
            List<int> anzahlVonC01 = list_krebsmeldung.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals("C01") && (krebsmeldung.Jahr >= 1983 && krebsmeldung.Jahr <= 1989)).ToList().MySum(jahre);
            List<List<int>> anzahls = new List<List<int>> { anzahlVonC00, anzahlVonC01 };
            AreaChart(anzahls);


            List<Krebsmeldung> geoHeatMapRelevant = list_krebsmeldung.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals("C00") && krebsmeldung.Jahr == 1994).ToList();
            GeoMap(geoHeatMapRelevant);


            List<Krebsmeldung> pieChart2Relevant = list_krebsmeldung.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals("C00") && krebsmeldung.Bundesland.Equals("Oberösterreich")).ToList();
            PieChart2(pieChart2Relevant);


            jahreForLiveChart = list_krebsmeldung.Select(x => x.Jahr).Distinct().ToList();
            List<int> liveChartRelevantC00 = list_krebsmeldung.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals("C00")).ToList().MySum(jahreForLiveChart);
            LiveChart(liveChartRelevantC00, jahreForLiveChart);

            Table(list_krebsmeldung);
        }

        #region Table

        private void Table(List<Krebsmeldung> list_krebsmeldung)
        {
            List<Krebsmeldung> krebsmeldungen = new List<Krebsmeldung>();
            foreach (Krebsmeldung k in list_krebsmeldung)
            {
                krebsmeldungen.Add(k);
            }
            lvKrebsmeldungen.ItemsSource = krebsmeldungen;
        }

        #endregion

        #region GeoMap


        public void GeoMap(List<Krebsmeldung> show)
        {
            Dictionary<string, double> bundeslaenderCounter = new Dictionary<string, double>();
            Dictionary<string, string> bundeslaenderIDs = new Dictionary<string, string>()
            {
                {"Vorarlberg", "2273"},
                {"Burgenland","2274"},
                {"Steiermark", "2275"},
                {"Kärnten","2276"},
                {"Oberösterreich", "2277"},
                {"Salzburg", "2278" },
                {"Tirol", "2280"},
                {"Niederösterreich", "2281"},
                {"Wien", "2282"},
            };

            foreach (Krebsmeldung krebsmeldung in show)
            {
                if (!bundeslaenderCounter.ContainsKey(bundeslaenderIDs[krebsmeldung.Bundesland]))
                {
                    bundeslaenderCounter.Add(bundeslaenderIDs[krebsmeldung.Bundesland], 0);
                }
                bundeslaenderCounter[bundeslaenderIDs[krebsmeldung.Bundesland]] += krebsmeldung.Anzahl;
            }
            geoMap.HeatMap = bundeslaenderCounter;

            var tooltip = new DefaultTooltip
            {
                SelectionMode = TooltipSelectionMode.Auto,
                IsEnabled = false
            };

            geoMap.ToolTip = tooltip;
        }

        #endregion

        #region PieChart Bundesländer C00
        public Func<ChartPoint, string> PointLabel { get; set; }
        public void PieChart(List<Krebsmeldung> show)
        {
            //show beinhaltet alle Krebsmeldungen mit ICD10 C00
            //Dictionary muss erstellt werden mit allen Bundesländern und der anzahl der krebsmeldungen dazu
            //dazu muss man die liste (jede krebsmeldugn) durchgehen und das bundesland herausfinden und den counter um die anzahl erhöhen
            Dictionary<string, int> bundeslaenderCounter = new Dictionary<string, int>();

            bundeslaenderCounter.Add("Burgenland", 0);
            bundeslaenderCounter.Add("Kärnten", 0);
            bundeslaenderCounter.Add("Niederösterreich", 0);
            bundeslaenderCounter.Add("Oberösterreich", 0);
            bundeslaenderCounter.Add("Salzburg", 0);
            bundeslaenderCounter.Add("Steiermark", 0);
            bundeslaenderCounter.Add("Tirol", 0);
            bundeslaenderCounter.Add("Vorarlberg", 0);
            bundeslaenderCounter.Add("Wien", 0);

            foreach (Krebsmeldung krebsmeldung in show)
            {
                bundeslaenderCounter[krebsmeldung.Bundesland] += krebsmeldung.Anzahl;
            }
            pieChart1.Series.Clear();

            SeriesCollection series = new SeriesCollection();

            foreach (KeyValuePair<string, int> bundesland in bundeslaenderCounter)
            {
                series.Add(new PieSeries() { Title = bundesland.Key, Values = new ChartValues<double> { bundesland.Value } });
            }
            pieChart1.Series = series;

            var tooltip = new DefaultTooltip
            {
                SelectionMode = TooltipSelectionMode.SharedYValues,
                IsEnabled = false

            };

            pieChart1.DataTooltip = tooltip;

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

        #region PieChart C00 in Bundesland in Berichtsjahr

        public void PieChart2(List<Krebsmeldung> show)
        {

            Dictionary<int, int> berichtsJahrCounter = new Dictionary<int, int>();



            foreach (Krebsmeldung krebsmeldung in show)
            {
                if (!berichtsJahrCounter.ContainsKey(krebsmeldung.Jahr)) berichtsJahrCounter.Add(krebsmeldung.Jahr, 0);

                berichtsJahrCounter[krebsmeldung.Jahr] += krebsmeldung.Anzahl;

            }
            pieChart2.Series.Clear();

            SeriesCollection series = new SeriesCollection();

            foreach (KeyValuePair<int, int> berichtsJahr in berichtsJahrCounter)
            {
                series.Add(new PieSeries() { Title = "" + berichtsJahr.Key, Values = new ChartValues<double> { berichtsJahr.Value } });
            }
            pieChart2.Series = series;
        }

        #endregion

        #region NegativStackChart
        public SeriesCollection SeriesCollectionNSC { get; set; }
        public string[] LabelsNSC { get; set; }
        public Func<double, string> FormatterNSC { get; set; }
        private void NegativStackChart(List<Krebsmeldung> show)
        {
            ChartValues<Int32> cvMale = new ChartValues<int>();
            ChartValues<Int32> cvFemale = new ChartValues<int>();
            Dictionary<Int32, Int32> dMale = new Dictionary<int, int>();
            Dictionary<Int32, Int32> dFemale = new Dictionary<int, int>();
            List<int> labels = new List<int>();
            foreach (Krebsmeldung krebsmeldung in show)
            {
                if (!labels.Contains(krebsmeldung.Jahr))
                {
                    labels.Add(krebsmeldung.Jahr);
                    dMale.Add(krebsmeldung.Jahr, 0);
                    dFemale.Add(krebsmeldung.Jahr, 0);
                }
                if (krebsmeldung.Geschlecht.Equals("männlich"))
                {

                    dMale[krebsmeldung.Jahr] -= krebsmeldung.Anzahl;

                }
                else if (krebsmeldung.Geschlecht.Equals("weiblich"))
                {

                    dFemale[krebsmeldung.Jahr] += krebsmeldung.Anzahl;

                }
            }


            foreach (KeyValuePair<int, int> keyValuePair in dMale)
            {
                cvMale.Add(keyValuePair.Value);
            }
            foreach (KeyValuePair<int, int> keyValuePair in dFemale)
            {
                cvFemale.Add(keyValuePair.Value);
            }


            SeriesCollectionNSC = new SeriesCollection
            {

                new StackedRowSeries
                {
                    //DataLabels = 
                    
                    Title = "Male",
                    Values = cvMale


                },
                new StackedRowSeries
                {
                    Title = "Female",
                    Values = cvFemale
                }

            };
            negativStackChart.Series = SeriesCollectionNSC;

            LabelsNSC = new string[labels.Count];
            for (int i = 0; i < LabelsNSC.Length; i++)
            {
                LabelsNSC[i] = labels[i].ToString();
            }
            //LabelsNSC = new[] { "0-20", "20-35", "35-45", "45-55", "55-65", "65-70", ">70" };
            FormatterNSC = value => Math.Abs(value).ToString();

            DataContext = this;
        }

        #endregion

        #region AreaChart
        public SeriesCollection SeriesCollectionAC { get; set; }
        public string[] LabelsAC { get; set; }
        public Func<int, string> YFormatterAC { get; set; }
        public Func<int, string> XFormatterAC { get; set; }
        private void AreaChart(List<List<int>> anzahls)
        {
            List<ChartValues<int>> cValues = new List<ChartValues<int>>();
            foreach (var list_anzahlen in anzahls)
            {
                ChartValues<int> cv_current = new ChartValues<int>();
                foreach (var anzahl in list_anzahlen)
                {
                    cv_current.Add(anzahl);
                }
                cValues.Add(cv_current);
            }

            SeriesCollectionAC = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "C00",
                    Values = cValues[0]
                },
                new LineSeries
                {
                    Title = "C01",
                    Values = cValues[1],
                    PointGeometry = null
                }
            };
            id_lineChart.Series = SeriesCollectionAC;
            LabelsAC = new[] { "1983", "1984", "1985", "1986", "1987", "1988", "1989" };
            YFormatterAC = value => value.ToString("C");


            DataContext = this;
        }

        #endregion

        #region BarChart
        public SeriesCollection SeriesCollectionBC { get; set; }

        public string[] LabelsBC { get; set; }
        public Func<double, string> Formatter { get; set; }


        private void BarChart(List<Krebsmeldung> show)
        { //Balken für C00 und Balken für C01, man soll etwa 3-4 Jahre darstellen können

            ChartValues<Int32> tumorart00 = new ChartValues<int>();
            ChartValues<Int32> tumorart01 = new ChartValues<int>();
            Dictionary<string, double> tumorartC00 = new Dictionary<string, double>();
            Dictionary<string, double> tumorartC01 = new Dictionary<string, double>();
            List<int> labels = new List<int>();

            //Testversuch
            List<KeyValuePair<int, int>> series = new List<KeyValuePair<int, int>>();
            series.Add(new KeyValuePair<int, int>());

            foreach (Krebsmeldung item in show)
            {
                tumorartC00[item.Krebsart] += item.Anzahl;
            }
            //barChart.Series.Clear();


            SeriesCollectionBC = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "",
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

            LabelsBC = new string[labels.Count]; //{ "Maria", "Susan", "Charles", "Frida" }
            for (int i = 0; i < LabelsBC.Length; i++)
            {
                LabelsBC[i] = labels[i].ToString();
            };

            Formatter = value => value.ToString("N");

            DataContext = this;
        }
        #endregion

        #region LiveChart

        public int MinValueX { get; set; }
        public int MaxValueX { get; set; }

        public string[] LabelsLC { get; set; }

        public Func<int, string> XFormatterLC { get; set; }

        

        private void LiveChart(List<int> anzahls, List<int> jahre)
        {
            var values = new ChartValues<int>();

            foreach (var anzahl in anzahls)
            {
                values.Add(anzahl);
            }

            liveChart.Series.Add(new LineSeries
            {
                Values = values
            });
            

            LabelsLC = jahre.Select(j => j.ToString()).ToArray();

        }

        private void PreviousOnClick(object sender, EventArgs e)
        {
            int newIntervallMin = (int)(liveChart.AxisX[0].MinValue - 2);
            int newIntervallMax = (int)(liveChart.AxisX[0].MaxValue - 2);
            if(newIntervallMin + jahreForLiveChart.Min() >= jahreForLiveChart.Min() && newIntervallMax + jahreForLiveChart.Min() <= jahreForLiveChart.Max())
            {
                liveChart.AxisX[0].MinValue -= 2;
                liveChart.AxisX[0].MaxValue -= 2;
            }
            else
            {
                liveChart.AxisX[0].MinValue = 0;
                liveChart.AxisX[0].MaxValue = 2;
            }
        }

        private void NextOnClick(object sender, EventArgs e)
        {
            int newIntervallMin = (int)(liveChart.AxisX[0].MinValue + 2);
            int newIntervallMax = (int)(liveChart.AxisX[0].MaxValue + 2);
            if (newIntervallMin + jahreForLiveChart.Min() >= jahreForLiveChart.Min() && newIntervallMax + jahreForLiveChart.Min() <= jahreForLiveChart.Max())
            {
                liveChart.AxisX[0].MinValue += 2;
                liveChart.AxisX[0].MaxValue += 2;
            }
            else
            {
                liveChart.AxisX[0].MinValue = jahreForLiveChart.Count() -2;
                liveChart.AxisX[0].MaxValue = jahreForLiveChart.Count();
            }
        }

        private void CustomZoomOnClick(object sender, EventArgs e)
        {
            int minValue = Int32.Parse(minIntervall.Text) - jahreForLiveChart.Min();
            int maxValue = Int32.Parse(maxIntervall.Text) - jahreForLiveChart.Min();

            liveChart.AxisX[0].MinValue = minValue;
            liveChart.AxisX[0].MaxValue = maxValue;

            minIntervall.Clear();
            maxIntervall.Clear();
        }

        private void CustomZoomOutClick(object sender, RoutedEventArgs e)
        {
            liveChart.AxisX[0].MinValue = 0;
            liveChart.AxisX[0].MaxValue = jahreForLiveChart.Count();
        }

        #endregion

        #region Krebsmeldung

        public MainWindow(Krebsmeldung neueKrebsmeldung, bool erstellen)
        {

            InitializeComponent();


            lblException.Content = "";
            if (erstellen)
            {
                //Bestätigungs-Fenster
                DatabaseMethods.InsertNewMeldung(neueKrebsmeldung, constring);
            }
            else
            {
                if (neueKrebsmeldung != null)
                {
                    cbKrebsart.Text = $"{neueKrebsmeldung.ICD10Code} - {neueKrebsmeldung.Krebsart}";
                    cbGeschlecht.Text = neueKrebsmeldung.Geschlecht;
                    cbBundesland.Text = neueKrebsmeldung.Bundesland;
                    nudJahr.Value = neueKrebsmeldung.Jahr;
                    nudAnzahl.Value = neueKrebsmeldung.Anzahl;
                }
            }
            tiKrebsmeldung.IsSelected = true;
            DataContext = this;

        }

        private void bNeueKrebsmeldung_Click(object sender, RoutedEventArgs e)
        {
            lblException.Content = "";
            if (cbKrebsart.Text.Equals("") || cbGeschlecht.Text.Equals("") || cbBundesland.Text.Equals(""))
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
                    Anzahl = nudAnzahl.Value,
                    Jahr = nudJahr.Value
                };
                Window KrebsmeldungConfirm = new KrebsmeldungConfirm(neueKrebsmeldung);
                KrebsmeldungConfirm.Show();
                FillCharts();
                this.Close();
            }
        }

        private void FillComboBoxes()
        {
            cbKrebsart.ItemsSource = DatabaseMethods.GetDataFromDatabase_ICD10(constring);
            cbGeschlecht.ItemsSource = DatabaseMethods.GetDataFromDatabase_Geschlecht(constring);
            cbBundesland.ItemsSource = DatabaseMethods.GetDataFromDatabase_Bundesland(constring);
        }


        #endregion

        private void xmlLaden_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml("\\Dateien\\Pfade.xml");

                XmlNodeList nodeList = xml.GetElementsByTagName("Lili");
                foreach (XmlNode personalNode in nodeList)
                {
                    foreach (XmlNode targetNode in personalNode.ChildNodes)
                    {
                        if (targetNode.Name == "ConStringPfad")
                        {
                            constring = targetNode.InnerText;
                        }
                        else if (targetNode.Name == "RestICD10Pfad")
                        {
                            path_rest_icd10 = targetNode.InnerText;
                        }
                    }
                }
            }
        }
    }
}
