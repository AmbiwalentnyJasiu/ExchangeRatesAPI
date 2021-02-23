using System;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;



namespace ExchangeRatesAPI
{
    class Program
    {
        const string url = "https://api.exchangeratesapi.io/latest";
        private static readonly HttpClient client = new HttpClient();

        private static async Task<Format> GetDataFromGit()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage streamResponse = await client.GetAsync(url);

            streamResponse.EnsureSuccessStatusCode();

            var result = await streamResponse.Content.ReadAsStreamAsync();
            var resultList = await JsonSerializer.DeserializeAsync<Format>(result);

            return resultList;
        }
        static async Task Main(string[] args)
        {
            var response = await GetDataFromGit();
            int choice;

            Console.WriteLine("Welcome");

            do
            {
                Console.WriteLine("\nType '1' to see list of exchange rates\n" +
                                  "Or   '2' to convert chosen curency\n" +
                                  "Or   '3' to exit");

                choice = Convert.ToInt32(Console.ReadLine());

                switch (choice)
                {
                    case 1:

                        foreach (string key in response.Rates.Keys)
                        {
                            Console.WriteLine(key + " : " + response.Rates[key]);
                        }

                        Console.WriteLine("\nBase: " + response.Base);

                        Console.WriteLine("\nLast updated:" + response.Date);

                        break;

                    case 2:

                        Console.Write("\nName the currency you want to exchange: ");

                        string currency = Console.ReadLine();
                        bool notFound = true;

                        foreach (string key in response.Rates.Keys)
                        {
                            if (currency == key)
                            {
                                notFound = false;

                                Console.Write("Input the amount of " + currency + " to convert: ");
                                double amount = Convert.ToDouble(Console.ReadLine());

                                Console.WriteLine("\nResult: " + (amount / response.Rates[key]) + " EUR");
                                break;
                            }
                        }

                        if (notFound)
                        {
                            Console.WriteLine("Invalid name of currency");
                        }

                        break;

                    case 3:

                        Console.WriteLine("Exiting...");

                        break;

                    default:

                        Console.WriteLine("Invalid command");

                        break;

                }
            } while (choice != 3);
        }
    }
}
