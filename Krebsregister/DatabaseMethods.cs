using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;

namespace Krebsregister
{
    internal class DatabaseMethods
    {


        //Vali
        //static string constring = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Valentina\\Source\\Repos\\PRE-Krebsregister\\Krebsregister\\Krebsregister_Database.mdf;Integrated Security=True";
        //static string path_rest_icd10 = "C:\\Users\\Valentina\\Source\\Repos\\PRE-Krebsregister\\Krebsregister\\CSV-Dateien\\restlicheICD10Codes.csv";
        //Anna
        //static string constring = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\Markus Stadlbauer\\Documents\\Schule\\5. Klasse\\PRE\\Projekt\\Krebsregister\\Krebsregister\\Krebsregister_Database.mdf\";Integrated Security=True";
        //static string path_rest_icd10 = "C:\\Users\\Markus Stadlbauer\\Documents\\Schule\\5. Klasse\\PRE\\Projekt\\Krebsregister\\Krebsregister\\CSV-Dateien\\restlicheICD10Codes.csv";
        //Lili
        static string path_rest_icd10 = "C:\\Users\\lilia\\Source\\Repos\\stadlbauera\\PRE-Krebsregister\\Krebsregister\\CSV-Dateien\\restlicheICD10Codes.csv";
        static string constring = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\lilia\\source\\repos\\stadlbauera\\PRE-Krebsregister\\Krebsregister\\Krebsregister_Database.mdf;Integrated Security=True";

        #region erstellen und befüllen Datenbank
        public static void FillDatabase()
        {
            //Allgemein Databaseconnection erstellen und öffnen
            SqlConnection connection = new SqlConnection(constring);
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }


            //Geschlechtfields ist ein List mit die Zeilen der CSV-Dateien
            var Geschlechtfields = ReadInCSV_Web("https://data.statistik.gv.at/data/OGD_krebs_ext_KREBS_1_C-KRE_GESCHLECHT-0.csv");
            for (int i = 0; i < Geschlechtfields.Count; i++)
            {
                string[] currentFields = Geschlechtfields[i];
                string[] FieldsIDgesplittet = currentFields[0].Split("-"); //erste Feld der currentFields wird gesplittet, damit wir nur den eigentlichen GeschlechtsID haben (GESCHLECHT-1 -> GESCHLECHT; 1)
                SqlCommand cmd = new SqlCommand("INSERT INTO Geschlecht (GeschlechtID, Geschlecht) SELECT @GeschlechtID, @Geschlecht WHERE NOT EXISTS (SELECT GeschlechtID, Geschlecht FROM Geschlecht WHERE GeschlechtID = @GeschlechtID)", connection);
                cmd.Parameters.AddWithValue("@GeschlechtID", FieldsIDgesplittet[1]);    //1
                cmd.Parameters.AddWithValue("@Geschlecht", currentFields[1]);
                cmd.ExecuteNonQuery();
            }

            var Bundeslandfields = ReadInCSV_Web("https://data.statistik.gv.at/data/OGD_krebs_ext_KREBS_1_C-BUNDESLAND-0.csv");
            for (int i = 0; i < Bundeslandfields.Count; i++)
            {
                string[] currentFields = Bundeslandfields[i];
                string[] FieldsIDgesplittet = currentFields[0].Split("-");     //BUNDESLAND-1 -> BUNDESLAND; 1
                SqlCommand cmd = new SqlCommand("INSERT INTO Bundesland (BundeslandID, Name) SELECT @BundeslandID, @Name WHERE NOT EXISTS (SELECT BundeslandID, Name FROM Bundesland WHERE BundeslandID = @BundeslandID)", connection);
                cmd.Parameters.AddWithValue("@BundeslandID", FieldsIDgesplittet[1]);    //1
                cmd.Parameters.AddWithValue("@Name", currentFields[1]);
                cmd.ExecuteNonQuery();
            }


            var ICD10fields = ReadInCSV_Web("https://data.statistik.gv.at/data/OGD_krebs_ext_KREBS_1_C-TUM_ICD10_3ST-0.csv");
            for (int i = 0; i < ICD10fields.Count; i++)
            {
                string[] currentFields = ICD10fields[i];

                string[] FieldsIDgesplittet = currentFields[0].Split("-");  //TUM_ICD10_3ST-D10 -> TUM_ICD10_3ST; D10

                SqlCommand cmd = new SqlCommand("INSERT INTO ICD10 (ICD10ID, ICD10Code, Bezeichnung) SELECT @ICD10ID, @ICD10Code, @Bezeichnung WHERE NOT EXISTS (SELECT ICD10ID, ICD10Code, Bezeichnung FROM ICD10 WHERE ICD10ID = @ICD10ID)", connection);


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

                SqlCommand cmd = new SqlCommand("INSERT INTO ICD10 (ICD10ID, ICD10Code, Bezeichnung) SELECT @ICD10ID, @ICD10Code, @Bezeichnung WHERE NOT EXISTS (SELECT ICD10ID, ICD10Code, Bezeichnung FROM ICD10 WHERE ICD10ID = @ICD10ID)", connection);


                cmd.Parameters.AddWithValue("@ICD10ID", currentCountID);     //ICD10ID ist ein durchgezählte Nummer

                cmd.Parameters.AddWithValue("@ICD10Code", FieldsIDgesplittet[1]);   //D10

                string[] bezeichnung_current = currentFields[1].Split("> ");    //<D10> Gutartige Neubildung des Mundes und des Pharynx -> <D10; Gutartige Neubildung des Mundes und des Pharynx

                cmd.Parameters.AddWithValue("@Bezeichnung", bezeichnung_current[1]);    //Gutartige Neubildung des Mundes und des Pharynx
                cmd.ExecuteNonQuery();
                currentCountID++;
            }


            var Eintragfields = ReadInCSV_Web("https://data.statistik.gv.at/data/OGD_krebs_ext_KREBS_1.csv");
            for (int i = 0; i < 100; i++)           //i < Eintragfields.Count
            {
                string[] currentFields = Eintragfields[i];
                SqlCommand cmd = new SqlCommand("INSERT INTO Eintrag (EintragID, Berichtsjahr, AnzahlMeldungen, ICD10ID, GeschlechtID, BundeslandID) SELECT @EintragID, @Berichtsjahr, @AnzahlMeldungen, @ICD10ID, @GeschlechtID, @BundeslandID WHERE NOT EXISTS (SELECT EintragID, Berichtsjahr, AnzahlMeldungen, ICD10ID, GeschlechtID, BundeslandID FROM Eintrag WHERE EintragID = @EintragID)", connection);

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

        #endregion erstellen und befüllen Datenbank


        #region neue Krebsmeldung einfügen
        public static void InsertNewMeldung(Krebsmeldung km)
        {
            //Allgemein Databaseconnection erstellen und öffnen
            SqlConnection connection = new SqlConnection(constring);
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            SqlCommand cmd_id = new SqlCommand("SELECT MAX(EintragID)+1 FROM Eintrag;", connection);
            SqlDataReader reader = cmd_id.ExecuteReader();
            int aktuell_id = 0;
            if(reader.Read())
            {
                aktuell_id = reader.GetInt32(0);
            }
            reader.Close();
            
            SqlCommand cmd_insert = new SqlCommand("INSERT INTO EINTRAG (EintragID, Berichtsjahr, AnzahlMeldungen) VALUES (@aktuell_id, @aktuell_jahr, @aktuell_anzahlMeldungen);", connection);
            cmd_insert.Parameters.AddWithValue("@aktuell_id", aktuell_id);
            cmd_insert.Parameters.AddWithValue("@aktuell_jahr", km.Jahr);
            cmd_insert.Parameters.AddWithValue("@aktuell_anzahlMeldungen", km.Anzahl);
            cmd_insert.ExecuteNonQuery();

            SqlCommand cmd_update = new SqlCommand("UPDATE e " +
                "SET e.ICD10ID = (SELECT ICD10ID FROM ICD10 WHERE ICD10Code = @aktuell_ICD10Code), " +
                "e.GeschlechtID = (SELECT GeschlechtID FROM Geschlecht WHERE Geschlecht = @aktuell_geschlecht), " +
                "e.BundeslandID = (SELECT BundeslandID FROM Bundesland WHERE Name = @aktuell_bundesland) " +
                "FROM Eintrag e " +
                "WHERE e.EintragID = @aktuell_id;", connection);
            cmd_update.Parameters.AddWithValue("@aktuell_id", aktuell_id);
            cmd_update.Parameters.AddWithValue("@aktuell_ICD10Code", km.ICD10Code);
            cmd_update.Parameters.AddWithValue("@aktuell_geschlecht", km.Geschlecht);
            cmd_update.Parameters.AddWithValue("@aktuell_bundesland", km.Bundesland);
            cmd_update.ExecuteNonQuery();     
            connection.Close();
        }

        #endregion neue Krebsmeldung einfügen

        #region Daten aus den Datenbank holen 

        public static List<Krebsmeldung> GetDataFromDatabase_Eintrag()
        {
            List<Krebsmeldung> krebsmeldungs = new List<Krebsmeldung>();
            SqlConnection connection = new SqlConnection(constring);
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            SqlCommand cmd = new SqlCommand("SELECT e.Berichtsjahr, e.AnzahlMeldungen, i.ICD10Code, i.Bezeichnung, g.Geschlecht, b.Name\r\nFROM Eintrag e\r\nJOIN ICD10 i ON (i.ICD10ID = e.ICD10ID)\r\nJOIN Geschlecht g ON (g.GeschlechtID = e.GeschlechtID)\r\nJOIN Bundesland b ON (b.BundeslandID = e.BundeslandID);", connection);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                krebsmeldungs.Add(new Krebsmeldung
                {
                    Jahr = reader.GetInt32(0),
                    Anzahl = reader.GetInt32(1),
                    ICD10Code = reader.GetString(2),
                    Krebsart = reader.GetString(3),
                    Geschlecht = reader.GetString(4),
                    Bundesland = reader.GetString(5)
                });
            }

            connection.Close();
            return krebsmeldungs;
        }

        public static List<string> GetDataFromDatabase_ICD10()
        {
            List<string> result = new List<string>();
            SqlConnection connection = new SqlConnection(constring);
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
            SqlCommand cmd = new SqlCommand("SELECT ICD10Code, Bezeichnung FROM ICD10", connection);
            SqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                result.Add($"{reader.GetString(0)} - {reader.GetString(1)}");
            }
            connection.Close();
            return result;
        }

        public static List<string> GetDataFromDatabase_Geschlecht()
        {
            List<string> result = new List<string>();
            SqlConnection connection = new SqlConnection(constring);
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
            SqlCommand cmd = new SqlCommand("SELECT Geschlecht FROM Geschlecht", connection);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader.GetString(0));
            }
            connection.Close();
            return result;
        }

        public static List<string> GetDataFromDatabase_Bundesland()
        {
            List<string> result = new List<string>();
            SqlConnection connection = new SqlConnection(constring);
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
            SqlCommand cmd = new SqlCommand("SELECT Name FROM Bundesland", connection);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader.GetString(0));
            }
            connection.Close();
            return result;
        }

        #endregion Daten aus den Datenbank holen

    }
}
