namespace Datenbank1;
using System;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Ocsp;

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


        Request("CREATE DATABASE IF NOT EXISTS todoapplication; USE todoapplication; CREATE TABLE IF NOT EXISTS Liste(id INT AUTO_INCREMENT PRIMARY KEY,Name TEXT); CREATE TABLE IF NOT EXISTS aufgabe ( id INT AUTO_INCREMENT PRIMARY KEY, beschreibung TEXT, priorität INT CHECK (priorität BETWEEN 0 AND 5), listen_id INT REFERENCES liste(id) );");
        Console.WriteLine("PROLQS-Datenbankanwendung");
        string activeUser = "";
        LoginToDo();
        void LoginToDo(){
            Console.WriteLine("AnmeldeFenster");
            while (true){
                Console.WriteLine("1. Anmelden");
                Console.WriteLine("2. Registrieren");
                Console.WriteLine("3. Beenden");
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
                            Request("SELECT liste.name, aufgabe.beschreibung, aufgabe.priorität FROM `aufgabe` JOIN  `liste` ON liste.id = aufgabe.listen_id WHERE isCompleted = 0 AND aufgabe.user_email = '"+activeUser+"' ORDER BY priorität DESC;");
                            break;
                        case "2":
                            Request("SELECT liste.name, aufgabe.beschreibung, aufgabe.priorität FROM `aufgabe` JOIN  `liste` ON liste.id = aufgabe.listen_id WHERE isCompleted = 0 AND aufgabe.user_email = '"+activeUser+"' ORDER BY priorität ASC;");
                            break;
                    }
                }
                void TasksfromList(){
                    Request("SELECT * FROM `liste`");
                    Console.WriteLine("\nID der Liste die ausgegeben wird: ");
                    string ?list = Console.ReadLine();
                    Console.WriteLine("\nListe    Beschreibung    Priorität");
                    Request("SELECT liste.name, aufgabe.beschreibung, aufgabe.priorität FROM `aufgabe` JOIN  `liste` ON liste.id = aufgabe.listen_id WHERE listen_ID = "+list+" AND isCompleted = 0 AND aufgabe.user_email = '"+activeUser+"';");
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
                    Request("SELECT liste.name, aufgabe.beschreibung, aufgabe.priorität FROM `aufgabe` JOIN  `liste` ON liste.id = aufgabe.listen_id WHERE isCompleted = 1 AND aufgabe.user_email = '"+activeUser+"'");
                }

                void RestoreTask(){
                    Console.WriteLine("\nID     Liste    Beschreibung    Priorität");
                    Request("SELECT aufgabe.id, liste.name, aufgabe.beschreibung, aufgabe.priorität FROM `aufgabe` JOIN  `liste` ON liste.id = aufgabe.listen_id WHERE isCompleted = 1 AND aufgabe.user_email = '"+activeUser+"'");
                    Console.WriteLine("ID der wiederherzustellenden Aufgabe: ");
                    string ?task = Console.ReadLine();
                    Request("UPDATE aufgabe SET isCompleted = 0 WHERE id = "+task+";");
                }
        }
}
