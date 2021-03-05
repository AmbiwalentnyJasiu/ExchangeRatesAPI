using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;



namespace ExchangeRatesAPI
{
    class Program
    {
        const string url = "https://api.exchangeratesapi.io/latest";
        private static readonly HttpClient client = new HttpClient();

        const string connectionString = "Server=LAPTOP-8AROAO4O\\MSSQLSERVERDEVJP;" +
                                        "Database=ExchangeRatesAPI;" +
                                        "User Id=sa;" +
                                        "Password=Janush04072018;";

        private static async Task<Format> GetDataFromGit()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage httpResponse = await client.GetAsync(url);

            httpResponse.EnsureSuccessStatusCode();

            var result = await httpResponse.Content.ReadAsStreamAsync();
            var resultList = await JsonSerializer.DeserializeAsync<Format>(result);

            return resultList;

        }

        private static bool CheckIfTableExists(string name)
        {
            bool exists;

            string checkString = $"select case when exists((select* from information_schema.tables " +
                                 $"where table_name = '{name}')) then 1 else 0 end";
            SqlConnection connection = new SqlConnection(connectionString);
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            SqlCommand checkCommand = new SqlCommand(checkString, connection);

            exists = (int)checkCommand.ExecuteScalar() == 1;

            connection.Close();

            return exists;

        }

        private static bool CheckIfElementExists(string name, string currency)
        {
            bool exists;

            string checkString = $"select case when exists((select * from {name} " +
                                 $"where Currency = '{currency}')) then 1 else 0 end";
            SqlConnection connection = new SqlConnection(connectionString);
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            SqlCommand checkCommand = new SqlCommand(checkString, connection);

            exists = (int)checkCommand.ExecuteScalar() == 1;

            connection.Close();

            return exists;

        }

        private static void UploadToDB(Format data)
        {
            string createString = "CREATE TABLE Rates( Currency char(3),  rate varchar(10) NOT NULL, PRIMARY KEY (currency))";
            string uploadString;

            SqlConnection connection = new SqlConnection(connectionString);

            SqlCommand uploadCommand;
            
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            SqlCommand createCommand = new SqlCommand(createString, connection);
            createCommand.ExecuteNonQuery();
           
               
            foreach(string key in data.Rates.Keys)
            {
                uploadString = $"INSERT INTO rates (Currency, rate) VALUES ('{key}' ,  '{data.Rates[key]}')";
                uploadCommand = new SqlCommand(uploadString, connection);
                uploadCommand.ExecuteNonQuery();
            }

            connection.Close();
        }

        private static Dictionary<string,double> ReadAllFromDB()
        {
            bool exists = CheckIfTableExists("Rates");

            if (exists)
            {
                Dictionary<string, double> result = new Dictionary<string, double>();
                string readString = "SELECT * FROM Rates";

                SqlConnection connection = new SqlConnection(connectionString);

                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                SqlCommand readCommand = new SqlCommand(readString, connection);

                var response = readCommand.ExecuteReader();

                while (response.Read())
                {
                    result.Add(response.GetString(0), Convert.ToDouble(response.GetString(1)));
                }

                connection.Close();

                return result;
            }
            else return null;
        }

        private static double ReadChosenFromDB(string currency)
        {
            bool elementExists = CheckIfElementExists("Rates", currency);

            string readString = $"SELECT rate FROM Rates WHERE Currency = '{currency}'";
            if (elementExists)
            {
                SqlConnection connection = new SqlConnection(connectionString);

                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                SqlCommand readCommand = new SqlCommand(readString, connection);

                
                var response = readCommand.ExecuteReader();
                response.Read();
                double result = Convert.ToDouble(response.GetString(0));
                
                connection.Close();
                return result;
            }
            else return -1;
        }

        private static void UpdateDB(Format data)
        {
            bool tableExists = CheckIfTableExists("Rates");
            bool elementExists;

            if (tableExists)
            {
                string updateString;
                SqlCommand updateCommand;
                SqlConnection connection = new SqlConnection(connectionString);

                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                foreach (string key in data.Rates.Keys)
                {
                    elementExists = CheckIfElementExists("Rates", key);
                    if (elementExists)
                    {
                        updateString = $"UPDATE rates set rate = '{data.Rates[key]}' WHERE currency = '{key}'";
                    }
                    else
                    {
                        updateString = $"INSERT INTO rates (Currency, rate) VALUES ('{key}' ,  '{data.Rates[key]}')";
                    }

                    updateCommand = new SqlCommand(updateString, connection);
                    updateCommand.ExecuteNonQuery();
                }

                connection.Close();
            }
            else
            {
                UploadToDB(data);
            }
        }

        static async Task Main(string[] args)
        {            
            int choice;

            Console.WriteLine("Welcome");

            do
            {
                Console.WriteLine("\nType '1' to see list of exchange rates\n" +
                                  "Or   '2' to convert chosen curency\n" +
                                  "Or   '3' to update database\n" +
                                  "Or   '4' to exit");


                if (int.TryParse(Console.ReadLine(), out choice))
                {

                    switch (choice)
                    {
                        case 1:

                            var result = ReadAllFromDB();

                            if (result != null)
                            {
                                foreach (string key in result.Keys)
                                {
                                    Console.WriteLine($"{key} : {result[key]}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Table does not exist, Upload data first!");
                            }

                            break;

                        case 2:

                            if (!CheckIfTableExists("Rates"))
                            {
                                Console.WriteLine("Table does not exist, Upload data first");
                                break;
                            }
                            Console.Write("\nName the currency you want to exchange: ");

                            string currency = Console.ReadLine().ToUpper();

                            double value = ReadChosenFromDB(currency);

                            if (value > 0)
                            {
                                Console.WriteLine($"1 EUR = {value} {currency}");
                                
                                Console.Write($"Input the amount of {currency} to convert: ");

                                if (double.TryParse(Console.ReadLine(), out double amount))
                                {
                                    Console.WriteLine($"\nResult: {amount / value} EUR");
                                }
                                else
                                {
                                    Console.WriteLine("Input correct number");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid name of currency");
                            }                           

                            break;

                        case 3:

                            Console.WriteLine("Updating DataBase...");
                            UpdateDB(await GetDataFromGit());
                            break;

                        case 4:

                            Console.WriteLine("Exiting...");

                            break;

                        default:

                            Console.WriteLine("Invalid command");

                            break;

                    }
                }
                else
                {
                    Console.WriteLine("Input correct number!");
                }
            } while (choice != 4);
        }
    }    
}
