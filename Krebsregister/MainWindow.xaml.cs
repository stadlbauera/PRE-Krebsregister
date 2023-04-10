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
using System.Diagnostics.Metrics;
using LiveCharts.Definitions.Charts;
using LiveCharts.Wpf.Charts.Base;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Xml;
using Path = System.IO.Path;
using System.Numerics;
using System.Reflection;
using System.Collections;

namespace Krebsregister
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Allgemein

        List<string> icd10s;
        List<string> geschlechter;
        List<string> bundeslaender;
        List<string> jahre;
        public string constring { get; set; }

        public string path_rest_icd10 { get; set; }

        public string path_xml { get; set; }

        private List<Krebsmeldung> alleKrebsmeldungen = new List<Krebsmeldung>();
        public MainWindow()
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeComponent();
            GetPaths();
            alleKrebsmeldungen = DatabaseMethods.GetDataFromDatabase_Eintrag(constring);
            //dgKrebsmeldungen.ItemsSource = alleKrebsmeldungen;

            nudJahr.MaxValue = DateTime.Now.Year;
            nudJahr.Value = DateTime.Now.Year;
            nudAnzahl.MinValue = 1;

            FillCharts(new List<string> { "C00", "C01", "C02" });

            FillComboBoxes();

            lvfilter.ItemsSource = alleKrebsmeldungen;
            lblTitleDashboardKrebsart.Content = "C00 - Bösartige Neubildung der Lippe";
        }

        private void FillComboBoxes()
        {
            NK_cbKrebsart.ItemsSource = DatabaseMethods.GetDataFromDatabase_ICD10(constring);
            NK_cbGeschlecht.ItemsSource = DatabaseMethods.GetDataFromDatabase_Geschlecht(constring);
            NK_cbBundesland.ItemsSource = DatabaseMethods.GetDataFromDatabase_Bundesland(constring);

            icd10s = DatabaseMethods.GetDataFromDatabase_ICD10(constring);
            icd10s.Insert(0, "Alle");
            ES_cboKrebsart.ItemsSource = icd10s;

            geschlechter = DatabaseMethods.GetDataFromDatabase_Geschlecht(constring);
            geschlechter.Insert(0, "Alle");
            ES_cboGeschlecht.ItemsSource = geschlechter;
            bundeslaender = DatabaseMethods.GetDataFromDatabase_Bundesland(constring);
            bundeslaender.Insert(0, "Alle");
            ES_cboBundesland.ItemsSource = bundeslaender;
             jahre = DatabaseMethods.GetDataFromDatabase_Eintrag(constring).Select(x => x.Jahr.ToString()).Distinct().ToList();
            jahre.Insert(0, "Alle");
            ES_cboBerichtsjahr.ItemsSource = jahre;

            tbVon.Text = DatabaseMethods.GetDataFromDatabase_Eintrag(constring).Select(x => x.Jahr).Distinct().Min().ToString();
            tbBis.Text = DatabaseMethods.GetDataFromDatabase_Eintrag(constring).Select(x => x.Jahr).Distinct().Max().ToString();
        }

        private void GetPaths()
        {
            string path = Assembly.GetExecutingAssembly().Location;
            while (!path.EndsWith("Krebsregister"))
            {
                path = Directory.GetParent(path).ToString();
            }

            path_xml = path + "\\Dateien\\Pfade.xml";

            XmlDocument xml = new XmlDocument();
            xml.Load(path_xml);

            XmlNodeList nodeList = xml.GetElementsByTagName("Anna");
            foreach (XmlNode personalNode in nodeList)
            {
                foreach (XmlNode targetNode in personalNode.ChildNodes)
                {
                    if (targetNode.Name == "ConStringPfad")
                    {
                        constring = targetNode.InnerText;
                    }
                }
            }
        }

        #region Charts


        public void FillPieChart<T>(PieChart chart, Dictionary<T, int> chartValuesToShow)
        {
            chart.Series.Clear();

            SeriesCollection series = new SeriesCollection();

            foreach (KeyValuePair<T, int> kv in chartValuesToShow)
            {
                series.Add(new PieSeries() { Title = $"{kv.Key}", Values = new ChartValues<int> { kv.Value } });
            }
            chart.Series = series;

            var tooltip = new DefaultTooltip
            {
                SelectionMode = TooltipSelectionMode.SharedYValues,
                IsEnabled = false

            };

            chart.DataTooltip = tooltip;
        }

        public void FillGeoMap(GeoMap geomap, List<Krebsmeldung> krebsmedlungentoShow)
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

            foreach (Krebsmeldung krebsmeldung in krebsmedlungentoShow)
            {
                if (!bundeslaenderCounter.ContainsKey(bundeslaenderIDs[krebsmeldung.Bundesland]))  bundeslaenderCounter.Add(bundeslaenderIDs[krebsmeldung.Bundesland], 0);
                bundeslaenderCounter[bundeslaenderIDs[krebsmeldung.Bundesland]] += krebsmeldung.Anzahl;
            }
            geomap.HeatMap = bundeslaenderCounter;

            //var tooltip = new DefaultTooltip
            //{
            //    SelectionMode = TooltipSelectionMode.Auto,
            //    IsEnabled = false
            //};

            //geomap.ToolTip = tooltip;
        }

        public void FillBarChart<T>(CartesianChart barChart, Dictionary<string, Dictionary<T, int>> chartValues)
        {
            SeriesCollection seriesCollection = new SeriesCollection();
            foreach (KeyValuePair<string, Dictionary<T, int>> icd10 in chartValues)
            {
                ChartValues<int> cv = new ChartValues<int>();
                foreach (KeyValuePair<T, int> jahr in icd10.Value)
                {
                    cv.Add(jahr.Value);
                }

                seriesCollection.Add(new ColumnSeries
                {
                    Title = icd10.Key.ToString(),
                    Values = cv
                });
            }
            barChart.Series = seriesCollection;
        }

        #endregion

        #endregion

        #region Menue

        private void DatenLaden_Click(object sender, RoutedEventArgs e)
        {
            DatabaseMethods.FillDatabase(constring);
        }

        private void Beenden_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Dashboard

        private void FillCharts(List<string> used_ICD10s)
        {
            FillTitles(used_ICD10s);
            List<string> icd10Beschreibung = DatabaseMethods.GetDataFromDatabase_ICD10(constring);
            lblTitleDashboardKrebsart.Content = icd10Beschreibung.Where(x => x.StartsWith(used_ICD10s[0])).ToList().FirstOrDefault();

            List<Krebsmeldung> pieChartRelevant = alleKrebsmeldungen.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals(used_ICD10s[0])).ToList();
            PieChart(pieChartRelevant);

            List<Krebsmeldung> negativStackChartRelevant = alleKrebsmeldungen.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals(used_ICD10s[0])).ToList();
            NegativStackChart(negativStackChartRelevant);


            List<Krebsmeldung> barChartRelevant = alleKrebsmeldungen.Where(krebsmeldung => ((krebsmeldung.ICD10Code.Equals(used_ICD10s[0]) || krebsmeldung.ICD10Code.Equals(used_ICD10s[1]) || krebsmeldung.ICD10Code.Equals(used_ICD10s[2])) && (krebsmeldung.Jahr >= 1983 && krebsmeldung.Jahr <= 1984))).ToList();
            BarChart(barChartRelevant);

            List<int> jahre = new List<int> { 1983, 1984, 1985, 1986, 1987, 1988, 1989 };
            List<int> anzahlVonC00 = alleKrebsmeldungen.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals(used_ICD10s[0]) && (krebsmeldung.Jahr >= 1983 && krebsmeldung.Jahr <= 1989)).ToList().MySum(jahre);
            List<int> anzahlVonC01 = alleKrebsmeldungen.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals(used_ICD10s[1]) && (krebsmeldung.Jahr >= 1983 && krebsmeldung.Jahr <= 1989)).ToList().MySum(jahre);
            List<List<int>> anzahls = new List<List<int>> { anzahlVonC00, anzahlVonC01 };
            AreaChart(anzahls);


            List<Krebsmeldung> geoHeatMapRelevant = alleKrebsmeldungen.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals(used_ICD10s[0]) && krebsmeldung.Jahr == 1994).ToList();
            FillGeoMap(geoMap, geoHeatMapRelevant);


            List<Krebsmeldung> pieChart2Relevant = alleKrebsmeldungen.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals(used_ICD10s[0]) && krebsmeldung.Bundesland.Equals("Oberösterreich")).ToList();
            PieChart2(pieChart2Relevant);


            jahreForLiveChart = alleKrebsmeldungen.Select(x => x.Jahr).Distinct().ToList();
            List<int> liveChartRelevantC00 = alleKrebsmeldungen.Where(krebsmeldung => krebsmeldung.ICD10Code.Equals(used_ICD10s[0])).ToList().MySum(jahreForLiveChart);
            LiveChart(liveChartRelevantC00, jahreForLiveChart);

            Table(alleKrebsmeldungen);
        }
        private void FillTitles(List<string> list)
        {
            lblTitleGeoHeatMap.Content = $"{list[0]} 1994 in Ö";
            lblTitleNegativStackedChart.Content = $"Vergleich Männer und Frauen: {list[0]}";
            lblTitlePieChart.Content = $"{list[0]} Verteilung auf die Bundesländer";
            lblTitleAreaChart.Content = $"{list[0]} und {list[1]} im Jahr 1983 - 1989";
            lblTitlePieChart2.Content = $"{list[0]} in Oberösterreich über die Jahre";
            //lblTitleGridView.Content = "Alle Einträge in der DB";
            lblTitleBarChart.Content = $"{list[0]}, {list[1]}, {list[2]} in den Jahren 1994 und 1995";
            lblTitleLiveChart.Content = $"{list[0]} über die Jahre";
        }

        #region Table

        private void Table(List<Krebsmeldung> list_krebsmeldung)
        {
            List<Krebsmeldung> krebsmeldungen = new List<Krebsmeldung>();
            foreach (Krebsmeldung k in list_krebsmeldung)
            {
                krebsmeldungen.Add(k);
            }
            //dgKrebsmeldungen.ItemsSource = krebsmeldungen;
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

            foreach (Krebsmeldung krebsmeldung in show)
            {
                if (!bundeslaenderCounter.ContainsKey(krebsmeldung.Bundesland)) bundeslaenderCounter.Add(krebsmeldung.Bundesland, 0);
                bundeslaenderCounter[krebsmeldung.Bundesland] += krebsmeldung.Anzahl;
            }
            FillPieChart<string>(pieChart1, bundeslaenderCounter);

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
            FillPieChart(pieChart2, berichtsJahrCounter);

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

        public string[] LabelsBC { get; set; }
        private void BarChart(List<Krebsmeldung> show)
        {
            List<string> labels = new List<string>();
            Dictionary<string, Dictionary<int, int>> chartValues = new Dictionary<string, Dictionary<int, int>>();

            foreach (Krebsmeldung krebsmeldung in show)
            {
                if (!chartValues.ContainsKey(krebsmeldung.ICD10Code))
                {
                    chartValues.Add(krebsmeldung.ICD10Code, new Dictionary<int, int>());
                }
                if (!chartValues[krebsmeldung.ICD10Code].ContainsKey(krebsmeldung.Jahr))
                {
                    labels.Add("" + krebsmeldung.Jahr);
                    chartValues[krebsmeldung.ICD10Code].Add(krebsmeldung.Jahr, 0);
                }
                chartValues[krebsmeldung.ICD10Code][krebsmeldung.Jahr] += krebsmeldung.Anzahl;
            }

            FillBarChart(barChart, chartValues);

            LabelsBC = new string[labels.Count];
            for (int i = 0; i < LabelsBC.Length; i++)
            {
                LabelsBC[i] = labels[i].ToString();
            };
        }
        #endregion

        #region LiveChart

        public List<int> jahreForLiveChart { get; set; }
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
            if (newIntervallMin + jahreForLiveChart.Min() >= jahreForLiveChart.Min() && newIntervallMax + jahreForLiveChart.Min() <= jahreForLiveChart.Max())
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
                liveChart.AxisX[0].MinValue = jahreForLiveChart.Count() - 2;
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



        #endregion

        #region Krebsmeldung

        public MainWindow(Krebsmeldung neueKrebsmeldung, bool erstellen)
        {

            InitializeComponent();

            lblException.Content = "";
            if (erstellen)
            {
                //Bestätigungs-Fenster
                GetPaths();
                DatabaseMethods.InsertNewMeldung(neueKrebsmeldung, constring);
            }
            else
            {
                if (neueKrebsmeldung != null)
                {
                    NK_cbKrebsart.Text = $"{neueKrebsmeldung.ICD10Code} - {neueKrebsmeldung.Krebsart}";
                    NK_cbGeschlecht.Text = neueKrebsmeldung.Geschlecht;
                    NK_cbBundesland.Text = neueKrebsmeldung.Bundesland;
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
            if (NK_cbKrebsart.Text.Equals("") || NK_cbGeschlecht.Text.Equals("") || NK_cbBundesland.Text.Equals(""))
            {
                lblException.Content = "Bitte füllen Sie alle Felder aus!";
            }
            else
            {
                Krebsmeldung neueKrebsmeldung = new Krebsmeldung
                {
                    Krebsart = NK_cbKrebsart.Text.Split(" - ")[1],
                    ICD10Code = NK_cbKrebsart.Text.Split(" - ")[0],
                    Geschlecht = NK_cbGeschlecht.Text,
                    Bundesland = NK_cbBundesland.Text,
                    Anzahl = nudAnzahl.Value,
                    Jahr = nudJahr.Value
                };
                Window KrebsmeldungConfirm = new KrebsmeldungConfirm(neueKrebsmeldung);
                KrebsmeldungConfirm.Show();
                alleKrebsmeldungen = DatabaseMethods.GetDataFromDatabase_Eintrag(constring);
                FillCharts(new List<string> { "C00", "C01", "C02" });
                this.Close();
            }
        }



        #endregion

        #region Erweiterte Statistik

        List<string> selectedICD10ES = new List<string>();
        List<string> selectedGeschlechtES = new List<string>();
        List<string> selectedBundeslandES = new List<string>();
        List<int> selectedJahrES = new List<int>();

        List<string> Krebsart_ES_Tabelle = new List<string>();
        List<string> Geschlecht_ES_Tabelle = new List<string>();
        List<string> Bundesland_ES_Tabelle = new List<string>();
        List<int> JAHR_ES_Tabelle = new List<int>();
        List<int> SUM_ES_Tabelle = new List<int>();
        List<int> AVG_ES_Tabelle = new List<int>();

        private void rbZeitpunkt_Checked(object sender, RoutedEventArgs e)
        {
            if (gbZeitpunkt != null) gbZeitpunkt.IsEnabled = true;
            if (gbZeitraum != null) gbZeitraum.IsEnabled = false;
        }

        private void rbZeitraum_Checked(object sender, RoutedEventArgs e)
        {
            if (gbZeitpunkt != null) gbZeitpunkt.IsEnabled = false;
            if (gbZeitraum != null) gbZeitraum.IsEnabled = true;
        }

        private void Aktualisieren(object sender, RoutedEventArgs e)
        {
            //Test-Daten
            //List<string> icd10s = new List<string>();
            //icd10s.Add("C00");
            //icd10s.Add("C01");
            //icd10s.Add("C02");
            //List<string> bundeslaender = new List<string>();
            //bundeslaender.Add("Wien");
            //bundeslaender.Add("Oberösterreich");
            //bundeslaender.Add("Niederösterreich");

            //List<string> berichtsjahre = new List<string>();
            //berichtsjahre.Add("1993");
            //berichtsjahre.Add("1994");
            //berichtsjahre.Add("1998");

            //List<string> geschelcht = new List<string>();
            //geschelcht.Add("männlich");
            //geschelcht.Add("weiblich");

            List<string> selectedicd10s = ES_lblSelectedKrebsart.Content.ToString().Split(" ")[1].Split(";").ToList();
            if (selectedicd10s[0].Equals("Alle"))
            {
                
                selectedicd10s = icd10s.Select(x => x.Split(" - ")[1]).ToList();
            }
            List<string> selectedbundeslaender = ES_lblSelectedBundesland.Content.ToString().Split(" ")[1].Split(";").ToList();
            if (selectedbundeslaender[0].Equals("Alle")) selectedbundeslaender = bundeslaender;
            List<string> selectedberichtsjahre = new List<string>();
            if (rbZeitpunkt.IsChecked == true)
            {
                selectedberichtsjahre = ES_lblSelectedZeitpunkt.Content.ToString().Split(" ")[1].Split(";").ToList();
                if (selectedberichtsjahre[0].Equals("Alle")) selectedberichtsjahre = jahre;
            }
            if (rbZeitraum.IsChecked == true)
            {
                for(int i = int.Parse(tbVon.Text); i <= int.Parse(tbBis.Text); i++)
                {
                    selectedberichtsjahre.Add(i.ToString());
                }
            }
            
            List<string> selectedgeschlecht = ES_lblSelectedGeschlecht.Content.ToString().Split(" ")[1].Split(";").ToList();
            if (selectedgeschlecht[0].Equals("Alle")) selectedgeschlecht = geschlechter;

            List<Krebsmeldung> list_krebsmeldung = DatabaseMethods.GetDataFromDatabase_Eintrag(constring);

            List<Krebsmeldung> gefilterte_krebsmeldung = list_krebsmeldung.Where(x => selectedicd10s.Contains(x.ICD10Code)).ToList()
                                                                            .Where(x => selectedbundeslaender.Contains(x.Bundesland)).ToList()
                                                                            .Where(x => selectedberichtsjahre.Contains(x.Jahr.ToString())).ToList()
                                                                            .Where(x => selectedgeschlecht.Contains(x.Geschlecht)).ToList();

            //List<Krebsmeldung> gefilterte_krebsmeldung = list_krebsmeldung.Where(x => selectedICD10ES.Contains(x.ICD10Code)).ToList()
            //                                                                .Where(x => selectedBundeslandES.Contains(x.Bundesland)).ToList()
            //                                                                .Where(x => selectedJahrES.Contains(x.Jahr)).ToList()
            //                                                                .Where(x => selectedGeschlechtES.Contains(x.Geschlecht)).ToList();
            CreateFilteredCharts(gefilterte_krebsmeldung);

            //List<Krebsmeldung> result = DatabaseMethods.ES_Cube(constring, selectedicd10s, selectedgeschlecht, selectedbundeslaender, selectedberichtsjahre);
            List<Krebsmeldung> result = DatabaseMethods.ES_ROLLUP(constring, selectedicd10s, selectedgeschlecht, selectedbundeslaender, selectedberichtsjahre);

            lvfilterNeu.ItemsSource = result;

            selectedICD10ES.Clear();
        }

        public string[] LabelsFBCBundesland { get; set; }
        public string[] LabelsFBCGeschlecht { get; set; }
        public string[] LabelsFBCICD10Code { get; set; }
        public string[] LabelsFBCJahr { get; set; }
        private void CreateFilteredCharts(List<Krebsmeldung> gefilterte_krebsmeldung)
        {
            //PieChart filteredPieChart = new PieChart();
            //filteredPieChart.LegendLocation = LegendLocation.Bottom;
            //filteredPieChart.Height = 300;
            //filteredPieChart.Width = 300;

            //filteredCharts.Children.Add(filteredPieChart);


            Dictionary<string, int> bundeslaenderCounter = new Dictionary<string, int>();

            foreach (Krebsmeldung krebsmeldung in gefilterte_krebsmeldung)
            {
                if (!bundeslaenderCounter.ContainsKey(krebsmeldung.Bundesland)) bundeslaenderCounter.Add(krebsmeldung.Bundesland, 0);
                bundeslaenderCounter[krebsmeldung.Bundesland] += krebsmeldung.Anzahl;
            }
            FillPieChart<string>(filteredPieChartBundesland, bundeslaenderCounter);

            Dictionary<string, int> geschlechtCounter = new Dictionary<string, int>();

            foreach (Krebsmeldung krebsmeldung in gefilterte_krebsmeldung)
            {
                if (!geschlechtCounter.ContainsKey(krebsmeldung.Geschlecht)) geschlechtCounter.Add(krebsmeldung.Geschlecht, 0);
                geschlechtCounter[krebsmeldung.Geschlecht] += krebsmeldung.Anzahl;
            }
            FillPieChart<string>(filteredPieChartGeschlecht, geschlechtCounter);


            Dictionary<string, int> ICD10CodeCounter = new Dictionary<string, int>();
            foreach (Krebsmeldung krebsmeldung in gefilterte_krebsmeldung)
            {
                if (!ICD10CodeCounter.ContainsKey(krebsmeldung.ICD10Code)) ICD10CodeCounter.Add(krebsmeldung.ICD10Code, 0);
                ICD10CodeCounter[krebsmeldung.ICD10Code] += krebsmeldung.Anzahl;
            }
            FillPieChart<string>(filteredPieChartICD10Code, ICD10CodeCounter);

            Dictionary<string, int> JahrCounter = new Dictionary<string, int>();
            foreach (Krebsmeldung krebsmeldung in gefilterte_krebsmeldung)
            {
                if (!JahrCounter.ContainsKey(krebsmeldung.Jahr.ToString())) JahrCounter.Add(krebsmeldung.Jahr.ToString(), 0);
                JahrCounter[krebsmeldung.Jahr.ToString()] += krebsmeldung.Anzahl;
            }
            FillPieChart<string>(filteredPieChartJahr, JahrCounter);

            FillGeoMap(filteredGeoMap, gefilterte_krebsmeldung);


            List<string> labelsBundesland = new List<string>();
            Dictionary<string, Dictionary<int, int>> chartValuesBundesland = new Dictionary<string, Dictionary<int, int>>();

            foreach (Krebsmeldung krebsmeldung in gefilterte_krebsmeldung)
            {
                if (!chartValuesBundesland.ContainsKey(krebsmeldung.Bundesland)) chartValuesBundesland.Add(krebsmeldung.Bundesland, new Dictionary<int, int>());
                if (!chartValuesBundesland[krebsmeldung.Bundesland.ToString()].ContainsKey(krebsmeldung.Jahr))
                {
                    labelsBundesland.Add("" + krebsmeldung.Jahr);
                    chartValuesBundesland[krebsmeldung.Bundesland].Add(krebsmeldung.Jahr, 0);
                }
                chartValuesBundesland[krebsmeldung.Bundesland][krebsmeldung.Jahr] += krebsmeldung.Anzahl;
            }

            FillBarChart(filteredBarChartBundesland, chartValuesBundesland);

            LabelsFBCBundesland = new string[labelsBundesland.Count];
            for (int i = 0; i < LabelsFBCBundesland.Length; i++)
            {
                LabelsFBCBundesland[i] = labelsBundesland[i].ToString();
            };


            List<string> labelsICD10Code = new List<string>();
            Dictionary<string, Dictionary<int, int>> chartValuesICD10Code = new Dictionary<string, Dictionary<int, int>>();

            foreach (Krebsmeldung krebsmeldung in gefilterte_krebsmeldung)
            {
                if (!chartValuesICD10Code.ContainsKey(krebsmeldung.ICD10Code)) chartValuesICD10Code.Add(krebsmeldung.ICD10Code, new Dictionary<int, int>());
                if (!chartValuesICD10Code[krebsmeldung.ICD10Code.ToString()].ContainsKey(krebsmeldung.Jahr))
                {
                    labelsBundesland.Add("" + krebsmeldung.Jahr);
                    chartValuesICD10Code[krebsmeldung.ICD10Code].Add(krebsmeldung.Jahr, 0);
                }
                chartValuesICD10Code[krebsmeldung.ICD10Code][krebsmeldung.Jahr] += krebsmeldung.Anzahl;
            }

            FillBarChart(filteredBarChartGeschlecht, chartValuesICD10Code);

            LabelsFBCGeschlecht = new string[labelsICD10Code.Count];
            for (int i = 0; i < LabelsFBCGeschlecht.Length; i++)
            {
                LabelsFBCGeschlecht[i] = labelsICD10Code[i].ToString();
            };

            List<string> labelsJahrBundesland = new List<string>();
            Dictionary<string, Dictionary<string, int>> chartValuesJahrBundesland = new Dictionary<string, Dictionary<string, int>>();

            foreach (Krebsmeldung krebsmeldung in gefilterte_krebsmeldung)
            {
                if (!chartValuesJahrBundesland.ContainsKey(krebsmeldung.Jahr.ToString())) chartValuesJahrBundesland.Add(krebsmeldung.Jahr.ToString(), new Dictionary<string, int>());
                if (!chartValuesJahrBundesland[krebsmeldung.Jahr.ToString()].ContainsKey(krebsmeldung.Bundesland))
                {
                    labelsBundesland.Add(krebsmeldung.Bundesland.ToString());
                    chartValuesJahrBundesland[krebsmeldung.Jahr.ToString()].Add(krebsmeldung.Bundesland, 0);
                }
                chartValuesJahrBundesland[krebsmeldung.Jahr.ToString()][krebsmeldung.Bundesland] += krebsmeldung.Anzahl;
            }

            FillBarChart(filteredBarChartJahr, chartValuesJahrBundesland);

            LabelsFBCJahr = new string[labelsJahrBundesland.Count];
            for (int i = 0; i < LabelsFBCJahr.Length; i++)
            {
                LabelsFBCJahr[i] = labelsJahrBundesland[i].ToString();
            };

           
            List<string> labelsICD10Geschlecht = new List<string>();
            Dictionary<string, Dictionary<string, int>> chartValuesICD10Geschlecht = new Dictionary<string, Dictionary<string, int>>();

            foreach (Krebsmeldung krebsmeldung in gefilterte_krebsmeldung)
            {
                if (!chartValuesICD10Geschlecht.ContainsKey(krebsmeldung.ICD10Code)) chartValuesICD10Geschlecht.Add(krebsmeldung.ICD10Code, new Dictionary<string, int>());
                if (!chartValuesICD10Geschlecht[krebsmeldung.ICD10Code].ContainsKey(krebsmeldung.Geschlecht))
                {
                    labelsICD10Geschlecht.Add(krebsmeldung.Geschlecht);
                    chartValuesICD10Geschlecht[krebsmeldung.ICD10Code].Add(krebsmeldung.Geschlecht, 0);
                }
                chartValuesICD10Geschlecht[krebsmeldung.ICD10Code][krebsmeldung.Geschlecht] += krebsmeldung.Anzahl;
            }

            FillBarChart(filteredBarChartICD10Code, chartValuesICD10Geschlecht);

            LabelsFBCICD10Code = new string[labelsICD10Geschlecht.Count];
            for (int i = 0; i < LabelsFBCJahr.Length; i++)
            {
                LabelsFBCICD10Code[i] = labelsICD10Geschlecht[i].ToString();
            };



        }
        private void FilterDashboard_Click(object sender, RoutedEventArgs e)
        {
            Window filterDWindow = new FilterDashboardWindow(DatabaseMethods.GetDataFromDatabase_ICD10(constring));
            if (filterDWindow.ShowDialog() == true)
            {
                FillCharts(FilterDashboardWindow.selectedICDs);
            }
        }

        

        //private void ES_cboKrebsart_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        //{
        //    foreach (string item in e.Selected)
        //    {
        //        selectedICD10ES.Add(item.Split(" - ")[0]);
        //        selectedICD10ES.Distinct();
        //    }
        //}

        //private void ES_cboGeschlecht_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        //{
        //    foreach (string item in e.Selected)
        //    {
        //        selectedGeschlechtES.Add(item);
        //        selectedGeschlechtES.Distinct();
        //    }
        //}

        //private void ES_cboBundesland_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        //{
        //    foreach (string item in e.Selected)
        //    {
        //        selectedBundeslandES.Add(item);
        //        selectedBundeslandES.Distinct();
        //    }
        //}

        //private void ES_cboBerichtsjahr_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        //{
        //    foreach (int item in e.Selected)
        //    {
        //        selectedJahrES.Add(item);
        //        selectedJahrES.Distinct();
        //    }
        //}

        #endregion

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //string? tabItem = ((TabItem)((TabControl)sender).SelectedItem).Header as string;
            //if(tabItem == "Erweiterte Statistik")
            //{
            //    selectedICD10ES.Clear();
            //    selectedBundeslandES.Clear();
            //    selectedGeschlechtES.Clear();
            //    selectedJahrES.Clear();
            //}
        }

        private void ES_cboKrebsart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = ES_cboKrebsart.SelectedItem.ToString().Split(" - ")[0];
            if (selected.Equals("Alle"))
            {
                ES_lblSelectedKrebsart.Content = "Ausgewählt: Alle";
            }
            else
            {
                if (ES_lblSelectedKrebsart.Content.Equals("Ausgewählt: Alle")) ES_lblSelectedKrebsart.Content = "Ausgewählt: ";
                if (ES_lblSelectedKrebsart.Content.ToString().Contains(selected))
                {
                    int length = ($"{selected};").Length;
                    int start = ES_lblSelectedKrebsart.Content.ToString().IndexOf(selected);
                    ES_lblSelectedKrebsart.Content = ES_lblSelectedKrebsart.Content.ToString().Remove(start, length);
                }
                else
                {
                    ES_lblSelectedKrebsart.Content = $"{ES_lblSelectedKrebsart.Content}{selected};";
                }
            }

        }

        private void ES_cboGeschlecht_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = ES_cboGeschlecht.SelectedItem.ToString();
            if (selected.Equals("Alle"))
            {
                ES_lblSelectedGeschlecht.Content = "Ausgewählt: Alle";
            }
            else
            {
                if (ES_lblSelectedGeschlecht.Content.Equals("Ausgewählt: Alle")) ES_lblSelectedGeschlecht.Content = "Ausgewählt: ";
                if (ES_lblSelectedGeschlecht.Content.ToString().Contains(selected))
                {
                    int length = ($"{selected};").Length;
                    int start = ES_lblSelectedGeschlecht.Content.ToString().IndexOf(selected);
                    ES_lblSelectedGeschlecht.Content = ES_lblSelectedGeschlecht.Content.ToString().Remove(start,length);
                }
                else
                {
                    ES_lblSelectedGeschlecht.Content = $"{ES_lblSelectedGeschlecht.Content}{selected};";
                }
            }
 
        }

        private void ES_cboBundesland_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = ES_cboBundesland.SelectedItem.ToString();
            if (selected.Equals("Alle"))
            {
                ES_lblSelectedBundesland.Content = "Ausgewählt: Alle";
            }
            else
            {
                if (ES_lblSelectedBundesland.Content.Equals("Ausgewählt: Alle")) ES_lblSelectedBundesland.Content = "Ausgewählt: ";
                if (ES_lblSelectedBundesland.Content.ToString().Contains(selected))
                {
                    int length = ($"{selected};").Length;
                    int start = ES_lblSelectedBundesland.Content.ToString().IndexOf(selected);
                    ES_lblSelectedBundesland.Content = ES_lblSelectedBundesland.Content.ToString().Remove(start, length);
                }
                else
                {
                    ES_lblSelectedBundesland.Content = $"{ES_lblSelectedBundesland.Content}{selected};";
                }
            }
        }

        private void ES_cboBerichtsjahr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            {
                string selected = ES_cboBerichtsjahr.SelectedItem.ToString();
                if (selected.Equals("Alle"))
                {
                    ES_lblSelectedZeitpunkt.Content = "Ausgewählt: Alle";
                }
                else
                {
                    if (ES_lblSelectedZeitpunkt.Content.Equals("Ausgewählt: Alle")) ES_lblSelectedZeitpunkt.Content = "Ausgewählt: ";
                    if (ES_lblSelectedZeitpunkt.Content.ToString().Contains(selected))
                    {
                        int length = ($"{selected};").Length;
                        int start = ES_lblSelectedZeitpunkt.Content.ToString().IndexOf(selected);
                        ES_lblSelectedZeitpunkt.Content = ES_lblSelectedZeitpunkt.Content.ToString().Remove(start, length);
                    }
                    else
                    {
                        ES_lblSelectedZeitpunkt.Content = $"{ES_lblSelectedZeitpunkt.Content}{selected};";
                    }
                }
            }
        }
    }
}
