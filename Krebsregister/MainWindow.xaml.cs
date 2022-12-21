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

        const string constring = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\lilia\\source\\repos\\stadlbauera\\PRE-Krebsregister\\Krebsregister\\Krebsregister_Database.mdf;Integrated Security=True";
        const string path_rest_icd10 = "C:\\Users\\lilia\\Source\\Repos\\stadlbauera\\PRE-Krebsregister\\Krebsregister\\CSV-Dateien\\restlicheICD10Codes.csv";

        public MainWindow()
        {
            PieChart();
            BarChart();
            AreaChart();
            NegativStackChart();
            GeoMap();



            DataContext = this;
            InitializeComponent();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //FillDatabase();
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


        #region Database

        public void FillDatabase()
        {
            //Allgemein Databaseconnection erstellen und öffnen
            SqlConnection connection = new SqlConnection(constring);
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            //Daten, die bereits in die Tabellen existieren löschen
            DropTables("Eintrag", connection);
            DropTables("Geschlecht", connection);
            DropTables("Bundesland", connection);
            DropTables("ICD10", connection);


            //Geschlechtfields ist ein List mit die Zeilen der CSV-Dateien
            var Geschlechtfields = ReadInCSV_Web("https://data.statistik.gv.at/data/OGD_krebs_ext_KREBS_1_C-KRE_GESCHLECHT-0.csv");
            for (int i = 0; i < Geschlechtfields.Count; i++)
            {
                string[] currentFields = Geschlechtfields[i];
                string[] FieldsIDgesplittet = currentFields[0].Split("-"); //erste Feld der currentFields wird gesplittet, damit wir nur den eigentlichen GeschlechtsID haben (GESCHLECHT-1 -> GESCHLECHT; 1)
                SqlCommand cmd = new SqlCommand("insert into Geschlecht (GeschlechtID, Geschlecht) values (@GeschlechtID, @Geschlecht)", connection);
                cmd.Parameters.AddWithValue("@GeschlechtID", FieldsIDgesplittet[1]);    //1
                cmd.Parameters.AddWithValue("@Geschlecht", currentFields[1]);
                cmd.ExecuteNonQuery();
            }

            var Bundeslandfields = ReadInCSV_Web("https://data.statistik.gv.at/data/OGD_krebs_ext_KREBS_1_C-BUNDESLAND-0.csv");
            for (int i = 0; i < Bundeslandfields.Count; i++)
            {
                string[] currentFields = Bundeslandfields[i];
                string[] FieldsIDgesplittet = currentFields[0].Split("-");     //BUNDESLAND-1 -> BUNDESLAND; 1
                SqlCommand cmd = new SqlCommand("insert into Bundesland (BundeslandID, Name) values (@BundeslandID, @Name)", connection);
                cmd.Parameters.AddWithValue("@BundeslandID", FieldsIDgesplittet[1]);    //1
                cmd.Parameters.AddWithValue("@Name", currentFields[1]);
                cmd.ExecuteNonQuery();
            }


            var ICD10fields = ReadInCSV_Web("https://data.statistik.gv.at/data/OGD_krebs_ext_KREBS_1_C-TUM_ICD10_3ST-0.csv");
            for (int i = 0; i < ICD10fields.Count; i++)
            {
                string[] currentFields = ICD10fields[i];

                string[] FieldsIDgesplittet = currentFields[0].Split("-");  //TUM_ICD10_3ST-D10 -> TUM_ICD10_3ST; D10

                SqlCommand cmd = new SqlCommand("insert into ICD10 (ICD10ID, ICD10Code, Bezeichnung) values (@ICD10ID, @ICD10Code, @Bezeichnung)", connection);


                cmd.Parameters.AddWithValue("@ICD10ID", i + 1);     //ICD10ID ist ein durchgezählte Nummer

                cmd.Parameters.AddWithValue("@ICD10Code", FieldsIDgesplittet[1]);   //D10

                string[] bezeichnung_current = currentFields[1].Split("> ");    //<D10> Gutartige Neubildung des Mundes und des Pharynx -> <D10; Gutartige Neubildung des Mundes und des Pharynx

                cmd.Parameters.AddWithValue("@Bezeichnung", bezeichnung_current[1]);    //Gutartige Neubildung des Mundes und des Pharynx
                cmd.ExecuteNonQuery();
            }

            int currentCountID = ICD10fields.Count() + 1;
            var RestICD10fields = ReadInCSV_Lokal(path_rest_icd10);
            for (int i = 0; i < RestICD10fields.Count; i++)
            {
                string[] currentFields = RestICD10fields[i];

                string[] FieldsIDgesplittet = currentFields[0].Split("-");  //TUM_ICD10_3ST-D10 -> TUM_ICD10_3ST; D10

                SqlCommand cmd = new SqlCommand("insert into ICD10 (ICD10ID, ICD10Code, Bezeichnung) values (@ICD10ID, @ICD10Code, @Bezeichnung)", connection);


                cmd.Parameters.AddWithValue("@ICD10ID", currentCountID);     //ICD10ID ist ein durchgezählte Nummer

                cmd.Parameters.AddWithValue("@ICD10Code", FieldsIDgesplittet[1]);   //D10

                string[] bezeichnung_current = currentFields[1].Split("> ");    //<D10> Gutartige Neubildung des Mundes und des Pharynx -> <D10; Gutartige Neubildung des Mundes und des Pharynx

                cmd.Parameters.AddWithValue("@Bezeichnung", bezeichnung_current[1]);    //Gutartige Neubildung des Mundes und des Pharynx
                cmd.ExecuteNonQuery();
                currentCountID++;
            }


            var Eintragfields = ReadInCSV_Web("https://data.statistik.gv.at/data/OGD_krebs_ext_KREBS_1.csv");
            for (int i = 0; i < Eintragfields.Count; i++)
            {
                string[] currentFields = Eintragfields[i];
                SqlCommand cmd = new SqlCommand("insert into Eintrag (EintragID, Berichtsjahr, AnzahlMeldungen, ICD10ID, GeschlechtID, BundeslandID) values (@EintragID, @Berichtsjahr, @AnzahlMeldungen, @ICD10ID, @GeschlechtID, @BundeslandID)", connection);

                cmd.Parameters.AddWithValue("@EintragID", i + 1);       //EintragID ist ein durchgezählte Nummer

                //Im nächsten Abschnitt wird ein SELECT-Anweisung auf die bereits existierte ICD10-Tabelle gemacht, damit mal den durchgezählten ICD10ID bekommt und als FK einsetzten kann
                int icd10ID_current = -1;
                string icd10Code_current = (currentFields[0].Split("-"))[1];    //TUM_ICD10_3ST-C00 -> TUM_ICD10_3ST; C00
                SqlCommand cmd_select_icd10 = new SqlCommand("select ICD10ID from ICD10 where ICD10Code=@currentCode", connection);
                cmd_select_icd10.Parameters.AddWithValue("@currentCode", icd10Code_current);
                SqlDataReader sqldr = cmd_select_icd10.ExecuteReader();

                while (sqldr.Read())
                { icd10ID_current = sqldr.GetInt32("ICD10ID"); }    //ID auslesen
                sqldr.Close();

                cmd.Parameters.AddWithValue("@ICD10ID", icd10ID_current);


                int berichtsjahr = int.Parse((currentFields[1].Split("-"))[1]);    //BERJ-1999 -> BERJ; 1999
                cmd.Parameters.AddWithValue("@Berichtsjahr", berichtsjahr);

                int bundesland = int.Parse((currentFields[2].Split("-"))[1]);      //BUNDESLAND-1 -> BUNDESLAND; 1
                cmd.Parameters.AddWithValue("@BundeslandID", bundesland);

                int geschlecht = int.Parse((currentFields[3].Split("-")[1]));       //GESCHLECHT-1 -> GESCHLECHT; 1
                cmd.Parameters.AddWithValue("@GeschlechtID", geschlecht);

                cmd.Parameters.AddWithValue("@AnzahlMeldungen", int.Parse(currentFields[4]));
                cmd.ExecuteNonQuery();



            }

            connection.Close();
        }

        private List<string[]> ReadInCSV_Web(string webPath)
        {
            List<string[]> fieldsList = new List<string[]>();
            WebRequest request = WebRequest.Create(webPath);
            request.Method = "GET";
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);

            reader.ReadLine();

            while (!reader.EndOfStream)
            {
                string[] fields = reader.ReadLine().Split(";");
                fieldsList.Add(fields);
            }
            reader.Close();
            response.Close();
            return fieldsList;
        }

        private List<string[]> ReadInCSV_Lokal(string path)
        {
            List<string[]> fieldsList = new List<string[]>();

            StreamReader reader = new StreamReader(path);
            reader.ReadLine();
            while (!reader.EndOfStream)
            {
                string[] fields = reader.ReadLine().Split(";");
                fieldsList.Add(fields);
            }
            reader.Close();
            return fieldsList;
        }

        private void DropTables(string tablename, SqlConnection connection)
        {
            SqlCommand sqlc = new SqlCommand($"DELETE FROM {tablename}", connection);
            sqlc.ExecuteNonQuery();
        }


        #endregion //Database

        private void bNeueKrebsmeldung_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
