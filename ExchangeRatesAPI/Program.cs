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

        private static void UploadToDB(Format data)
        {
            bool exists;

            string createString = "CREATE TABLE Rates( Currency char(3),  rate varchar(10) NOT NULL, PRIMARY KEY (currency))";
            string checkString = "select case when exists((select* from information_schema.tables where table_name = 'Rates')) then 1 else 0 end";
            string clearString = "TRUNCATE TABLE Rates";
            string uploadString;

            SqlConnection connection = new SqlConnection(connectionString);

            SqlCommand checkCommand = new SqlCommand(checkString, connection);
            SqlCommand uploadCommand;
            
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            exists = (int)checkCommand.ExecuteScalar() == 1;

            if (!exists)
            {
                SqlCommand createCommand = new SqlCommand(createString, connection);
                createCommand.ExecuteNonQuery();
            }
            else
            {
                SqlCommand clearCommand = new SqlCommand(clearString, connection);
                clearCommand.ExecuteNonQuery();
                
            }
               
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

        private static double ReadChosenFromDB(string currency)
        {
            bool exists;
            double result = -1;

            string readString = $"SELECT rate FROM Rates WHERE Currency = '{currency}'";
            string checkString = $"select case when exists((select * from rates where Currency = '{currency}')) then 1 else 0 end";

            SqlConnection connection = new SqlConnection(connectionString);

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            SqlCommand readCommand = new SqlCommand(readString, connection);
            SqlCommand checkCommand = new SqlCommand(checkString, connection);

            exists = (int)checkCommand.ExecuteScalar() == 1;

            if (exists)
            {
                var response = readCommand.ExecuteReader();
                response.Read();
                result = Convert.ToDouble(response.GetString(0));
            }

            connection.Close();

            return result;
        }

        private static void UpdateDB(Format data)
        {
            string updateString;
            SqlCommand updateCommand;
            SqlConnection connection = new SqlConnection(connectionString);

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            foreach (string key in data.Rates.Keys)
            {
                updateString = $"UPDATE rates set rate = '{data.Rates[key]}' WHERE currency = '{key}'";
                updateCommand = new SqlCommand(updateString, connection);
                updateCommand.ExecuteNonQuery();
            }

            connection.Close();
        }

        private static void DeleteTab()
        {
            string createString = $"drop table rates;";
            SqlConnection connection = new SqlConnection(connectionString);

            SqlCommand createCommand = new SqlCommand(createString, connection);
            if (connection.State != System.Data.ConnectionState.Open)
                createCommand.Connection.Open();

            createCommand.ExecuteNonQuery();

            connection.Close();
        }
        static async Task Main(string[] args)
        {
            var response = await GetDataFromGit();

            UploadToDB(response);
            
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

                            foreach (string key in result.Keys)
                            {
                                Console.WriteLine($"{key} : {result[key]}");
                            }

                            break;

                        case 2:

                            Console.Write("\nName the currency you want to exchange: ");

                            string currency = Console.ReadLine().ToUpper();

                            double value = ReadChosenFromDB(currency);

                            Console.WriteLine($"1 EUR = {value} {currency}");

                            if (value >= 0)
                            {
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
