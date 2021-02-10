using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;


namespace ExchangeRatesAPI
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static async Task GetDataFromGit()
        {

        }
        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome\nType '1' to see list of exchange rates\n" +
                                       "Or   '2' to convert chosen curency\n");
            int choice = Convert.ToInt32(Console.ReadLine());

            double[] values = { 4D, 5D, 1D, 1D, 8D };

            string[] names = { "USD", "PLN", "BGP", "RPT", "WED" };

            switch (choice)
            {
                case 1:

                    Console.WriteLine("\nBase: EUR");

                    for (int i = 0; i < 5; i++)
                    {
                        Console.WriteLine(names[i] + ": " + values[i]);
                    }

                    break;

                case 2:

                    Console.Write("\nName the curency you want to exchange: ");

                    string curency = Console.ReadLine();
                    int index = 0;
                    bool found = false;

                    foreach(string element in names)
                    {
                        if(curency == element)
                        {
                            found = true;
                            break;
                        }
                        else
                        {
                            index++;
                        }
                    }
                    
                    if (found)
                    {
                        Console.WriteLine(names[index] + ": " + values[index]);

                        Console.Write("Type in how much to convert: ");
                        double amount = Convert.ToDouble(Console.ReadLine());

                        Console.WriteLine("Result: " + (amount * values[index]));
                    }
                    else
                    {
                        Console.WriteLine("Invalid name of currency");
                    }

                    break;

                default:

                    Console.WriteLine("Invalid command");

                    break;
                
            }
        }
    }
}
