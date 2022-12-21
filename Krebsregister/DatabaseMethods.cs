using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Krebsregister
{
    internal class DatabaseMethods
    {

        static string constring = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\lilia\\source\\repos\\stadlbauera\\PRE-Krebsregister\\Krebsregister\\Krebsregister_Database.mdf;Integrated Security=True";
        static string path_rest_icd10 = "C:\\Users\\lilia\\Source\\Repos\\stadlbauera\\PRE-Krebsregister\\Krebsregister\\CSV-Dateien\\restlicheICD10Codes.csv";



        public static void FillDatabase()
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
                SqlCommand cmd = new SqlCommand($"insert into Geschlecht (GeschlechtID, Geschlecht) values (@GeschlechtID, @Geschlecht)", connection);
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

        public static void InsertNewMeldung(Krebsmeldung km)
        {
            //Allgemein Databaseconnection erstellen und öffnen
            SqlConnection connection = new SqlConnection(constring);
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            //latest ID => MAX(id)
        }


        private static List<string[]> ReadInCSV_Web(string webPath)
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

        private static List<string[]> ReadInCSV_Lokal(string path)
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

        private static void DropTables(string tablename, SqlConnection connection)
        {
            SqlCommand sqlc = new SqlCommand($"DELETE FROM {tablename}", connection);
            sqlc.ExecuteNonQuery();
        }

    }
}
