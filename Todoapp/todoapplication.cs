namespace Datenbank1;
using System;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IO;
using System.Text;
using System.Drawing.Printing;

public class ConsolApplication{
    public static void Main(string[] args){
        string connectionString = "server=localhost;userid=tandll;pwd=Kennwort1;database=todoapplication";
        void Request(string queryString){
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(queryString, connection);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                
                    while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write(reader[i] + " ");
                            }

                            Console.WriteLine();
                        }
                

                reader.Close();
            }
        }


        string RequestVariable(string queryString){
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(queryString, connection);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();

                string output = "";
                    while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                output += reader[i].ToString();
                            }
                        }
                reader.Close();
                return output;
            }
        }

        string structure = "CREATE DATABASE IF NOT EXISTS todoapplication; USE todoapplication; CREATE TABLE IF NOT EXISTS Liste(id INT AUTO_INCREMENT PRIMARY KEY,Name TEXT); CREATE TABLE IF NOT EXISTS aufgabe ( id INT AUTO_INCREMENT PRIMARY KEY, beschreibung TEXT, priorität INT CHECK (priorität BETWEEN 0 AND 5), listen_id INT REFERENCES liste(id) );";
        Request(structure);
        Console.WriteLine("PROLQS-Datenbankanwendung");
        string ?activeUser = "";
        LoginToDo();
        void LoginToDo(){
            Console.WriteLine("AnmeldeFenster");
            while (true){
                Console.WriteLine("1. Anmelden");
                Console.WriteLine("2. Registrieren");
                Console.WriteLine("3. Backup");
                Console.WriteLine("4. Restore last Backup");
                Console.WriteLine("5. Beenden");
                Console.Write("Auswahl: ");
                string ?input = Console.ReadLine();
                switch (input){
                    case "1":
                        Login();
                        break;
                    case "2":
                        SignIn();
                        break;
                    case "3":
                        BackupAll();
                        break;
                    case "4":
                        RestoreData();
                        break;
                    case "5":
                        return;
                }
            }
        }

        void Login(){
            Console.WriteLine("Email-Adresse:");
            string ?email = Console.ReadLine();
            Console.WriteLine("Password:");
            string ?password = Console.ReadLine();
            string realpassword = RequestVariable("SELECT password FROM `user` WHERE email ='"+email+"';");
            if (realpassword == password){
                Console.WriteLine("Login Successful");
                activeUser = email;
                MainConsole();
            }
            else{
                Console.WriteLine("Wrong Username or Password");
                return;}
            
        }

        void SignIn(){
            Console.WriteLine("Email-Adresse:");
            string ?email = Console.ReadLine();
            Console.WriteLine("Password:");
            string ?password = Console.ReadLine();
            Request("INSERT INTO `user`(`email`, `password`) VALUES ('"+email+"','"+password+"');");
            MainConsole();
        }

        // Methode zur Durchführung des Backups aller Tabellen
        void BackupAll(){  
            // Ruft die Backup-Daten für jede Tabelle ab und schreibt sie in eine Datei
            // Für jede Tabelle wird die BackupTable-Methode aufgerufen und das Ergebnis in eine Textdatei gespeichert.
            File.WriteAllText("..\\Backups\\ToDoAufgabeBackup.txt", BackupTable("Aufgabe"));
            File.WriteAllText("..\\Backups\\ToDoListeBackup.txt", BackupTable("Liste"));
            File.WriteAllText("..\\Backups\\ToDoUserBackup.txt", BackupTable("User"));

            // Gibt eine Erfolgsmeldung aus, nachdem das Backup durchgeführt wurde
            Console.WriteLine("Backup was successful");            
            }

            // Methode zum Erstellen eines Backups einer spezifischen Tabelle
            string BackupTable(string table){
            // Verbindet sich mit der MySQL-Datenbank
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // Öffnet die Verbindung zur Datenbank
                connection.Open();
                
                // StringBuilder wird verwendet, um die Backup-Daten in einer einzigen Zeichenkette zu speichern
                StringBuilder backupData = new StringBuilder();

                // Erzeugt eine SQL-Abfrage, um alle Daten aus der angegebenen Tabelle abzurufen
                string query = $"SELECT * FROM {table}";
                MySqlCommand command = new MySqlCommand(query, connection);

                // Führt die Abfrage aus und liest die Daten aus
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    // Durchläuft jede Zeile (Datensatz) in der Tabelle
                    while (reader.Read())
                    {
                        // Durchläuft alle Spalten in der aktuellen Zeile
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            // Fügt den Wert der aktuellen Spalte zum Backup-Datenstring hinzu
                            backupData.Append(reader[i].ToString());

                            // Fügt ein Komma zwischen den Spaltenwerten hinzu, wenn es sich nicht um die letzte Spalte handelt
                            if (i < reader.FieldCount - 1)
                                backupData.Append(",");
                        }

                        // Fügt einen Zeilenumbruch nach jeder Zeile (Datensatz) hinzu
                        backupData.AppendLine();
                    }
                }
                // Gibt die generierten Backup-Daten als Text zurück
                return backupData.ToString();
            }
        }

        void RestoreData(){
            // Ruft die Restore-Methoden für jede Tabelle auf   
            RestoreDataList();
            RestoreDataUser();
            RestoreDataAufgabe();
        }

        // Methode zur Wiederherstellung der 'Liste'-Tabelle aus der Backup-Datei
        void RestoreDataList(){
            // Löscht alle vorhandenen Daten in der 'Liste'-Tabelle
            Request("DELETE FROM Liste");

            // Pfad zur Backup-Datei für die 'Liste'-Tabelle
            string filePath = "..\\Backups\\ToDoListeBackup.txt";
            // Liest alle Zeilen der Datei in ein Array
            string[] lines = File.ReadAllLines(filePath);

            // Durchläuft jede Zeile in der Datei
            foreach (string line in lines)
            {
                // Teilt die Zeile in Werte auf, getrennt durch Kommas
                string[] values = line.Split(',');

                // Prüft, ob die Zeile zwei Werte enthält (id und Name)
                if (values.Length == 2)
                {
                    // Erzeugt eine SQL-Abfrage zum Einfügen der Daten in die Tabelle
                    string query = $"INSERT INTO Liste (id, Name) VALUES ({values[0]}, '{values[1]}')";
                    // Führt die Abfrage in der Datenbank aus
                    Request(query);
                }
            }
            Console.WriteLine("Liste table restored successfully.");
        }

        // Methode zur Wiederherstellung der 'User'-Tabelle aus der Backup-Datei
        void RestoreDataUser(){
            // Löscht alle vorhandenen Daten in der 'User'-Tabelle
            Request("DELETE FROM `User`");
            // Pfad zur Backup-Datei für die 'User'-Tabelle
            string filePath = "..\\Backups\\ToDoUserBackup.txt";
            // Liest alle Zeilen der Datei in ein Array
            string[] lines = File.ReadAllLines(filePath);
            // Durchläuft jede Zeile in der Datei
            foreach (string line in lines)
            {
                // Teilt die Zeile in Werte auf, getrennt durch Kommas
                string[] values = line.Split(',');
                // Prüft, ob mindestens zwei Werte vorhanden sind (email und password)
                if (values.Length == 2)
                {
                    // Erzeugt eine SQL-Abfrage zum Einfügen der Daten in die Tabelle
                    string query = $"INSERT INTO `User` (email, password) VALUES ('{values[0]}', '{values[1]}')";
                    // Führt die Abfrage in der Datenbank aus
                    Request(query);
                }
            }
            Console.WriteLine("User table restored successfully.");
        }

        // Methode zur Wiederherstellung der 'Aufgabe'-Tabelle aus der Backup-Datei
        void RestoreDataAufgabe(){
            // Löscht alle vorhandenen Daten in der 'Aufgabe'-Tabelle
            Request("DELETE FROM Aufgabe");
            // Pfad zur Backup-Datei für die 'Aufgabe'-Tabelle
            string filePath = "..\\Backups\\ToDoAufgabeBackup.txt";
            // Liest alle Zeilen der Datei in ein Array
            string[] lines = File.ReadAllLines(filePath);
            // Durchläuft jede Zeile in der Datei
            foreach (string line in lines)
            {
                // Teilt die Zeile in Werte auf, getrennt durch Kommas
                string[] values = line.Split(',');
                // Prüft, ob die Zeile sechs Werte enthält (id, beschreibung, priorität, listen_id, user_email, isCompleted)
                if(values.Length == 6){
                    // Erzeugt eine SQL-Abfrage zum Einfügen der Daten in die Tabelle
                    string query = $"INSERT INTO Aufgabe (id, beschreibung, priorität, listen_id, user_email, isCompleted) VALUES ({values[0]}, '{values[1]}', {values[2]}, {values[3]}, '{values[4]}', {values[5]})";
                    // Führt die Abfrage in der Datenbank aus
                    Request(query);
                }

            }
            
            Console.WriteLine("Aufgabe table restored successfully.");
        }



















        
        
        
        //VIEW 1: CREATE VIEW selectUnchecked AS SELECT liste.name, aufgabe.beschreibung, aufgabe.priorität, aufgabe.user_email FROM `aufgabe` JOIN `liste` ON liste.id = aufgabe.listen_id WHERE isCompleted = 0;
        //VIEW 2: CREATE VIEW selectchecked AS SELECT liste.name, aufgabe.beschreibung, aufgabe.priorität, aufgabe.user_email FROM `aufgabe` JOIN `liste` ON liste.id = aufgabe.listen_id WHERE isCompleted = 1;
        
        void MainConsole(){
            Console.WriteLine("Active user: "+activeUser);
                    while (true)
                    {
                        Console.WriteLine("\nWählen Sie eine Option:");
                        Console.WriteLine("1. Anlegen einer neuen Liste");
                        Console.WriteLine("2. Anlegen einer neuen Aufgabe");
                        Console.WriteLine("3. Ausgabe der Aufgaben sortiert nach Priorität");
                        Console.WriteLine("4. Ausgabe aller Aufgaben einer Liste");
                        Console.WriteLine("5. Aufgabe als erledigt markieren");
                        Console.WriteLine("6. Ausgabe aller Aufgaben die erledigt sind");
                        Console.WriteLine("7. Aufgabe die erledigt ist wieder zurückholen");
                        Console.WriteLine("8. Ausloggen");
        
                        Console.Write("Auswahl: ");
                        string ?input = Console.ReadLine();
            
                        switch (input)
                        {
                            case "1":
                                AddNewList();
                                break;
                            case "2":
                                AddNewTask();
                                break;
                            case "3":
                                TaskOutput();
                                break;
                            case "4":
                                TasksfromList();
                                break;
                            case "5":
                                CheckTask();
                                break;
                            case "6":
                                FinishedTasks();
                                break;
                            case "7":
                                RestoreTask();
                                break;
                            case "8":
                                return;
                        }
                    }
                }   

                void AddNewList(){
                    Console.WriteLine("Name der neuen Liste: ");
                    string ?name = Console.ReadLine();
                    Request("INSERT INTO liste (Name) VALUES ('"+name+"');");
                }
                void AddNewTask(){
                    Request("SELECT * FROM `liste`");
                    Console.WriteLine("Liste der neuen Aufgabe(ID): ");
                    string ?list = Console.ReadLine();
                    Console.WriteLine("Beschreibung der neuen Aufgabe: ");
                    string ?name = Console.ReadLine();
                    Console.WriteLine("Priorität der Aufgabe (0(niedrig)- 5(hoch)):");
                    string ?priorität = Console.ReadLine();   
                    Request("INSERT INTO aufgabe (beschreibung,priorität,listen_id, user_email) VALUES ('"+name+"',"+priorität+","+list+",'"+activeUser+"');");
                    
                }
                void TaskOutput(){
                    Console.WriteLine("Aufsteigend (1) oder Absteigend(2): ");
                    string ?input = Console.ReadLine();
                    Console.WriteLine("\nListe    Beschreibung    Priorität");
                    switch (input){
                        case "1":
                        //View1 used
                            Request("SELECT name, beschreibung, priorität FROM selectunchecked WHERE user_email = '"+activeUser+"' ORDER BY priorität DESC;");
                            break;
                        case "2":
                        //View1 used
                            Request("SELECT name, beschreibung, priorität FROM selectunchecked WHERE user_email = '"+activeUser+"' ORDER BY priorität DESC;");
                            break;
                    }
                }
                void TasksfromList(){
                    Request("SELECT * FROM `liste`");
                    Console.WriteLine("\nID der Liste die ausgegeben wird: ");
                    string ?list = Console.ReadLine();
                    Console.WriteLine("\nListe    Beschreibung    Priorität");
                    //View1 used
                    Request("SELECT name, beschreibung, priorität FROM selectunchecked WHERE user_email = '"+activeUser+"' AND listen_ID = "+list+"';");
                }

                void CheckTask(){
                    Console.WriteLine("\nID     Liste    Beschreibung    Priorität");
                    Request("SELECT aufgabe.id, liste.name, aufgabe.beschreibung, aufgabe.priorität FROM `aufgabe` JOIN  `liste` ON liste.id = aufgabe.listen_id WHERE isCompleted = 0 AND aufgabe.user_email = '"+activeUser+"'");
                    Console.WriteLine("Welche Aufgabe soll abgehakt werden?(ID)");
                    string ?task = Console.ReadLine();
                    Request("UPDATE aufgabe SET isCompleted = 1 WHERE id = "+task+";");
                    
                }

                void FinishedTasks(){
                    Console.WriteLine("\nListe    Beschreibung    Priorität");
                    //View2 used
                    Request("SELECT name, beschreibung, priorität FROM selectchecked WHERE aufgabe.user_email = '"+activeUser+"'");
                }

                void RestoreTask(){
                    Console.WriteLine("\nID     Liste    Beschreibung    Priorität");
                    //View2 used
                    Request("SELECT id, name, beschreibung, priorität FROM selectchecked WHERE aufgabe.user_email = '"+activeUser+"'");
                    Console.WriteLine("ID der wiederherzustellenden Aufgabe: ");
                    string ?task = Console.ReadLine();
                    Request("UPDATE aufgabe SET isCompleted = 0 WHERE id = "+task+";");
                }
        }
}
