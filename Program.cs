using System;
using System.Threading.Tasks;

namespace InformationWaves
{
    class Program
    {
        public static void Main(params string[] args)
        {
            var key = Environment.GetEnvironmentVariable("api_key");
            if (args.Length > 0)
                new TelegramWorker(args[0]).Start().Wait();
            else if(!string.IsNullOrEmpty(key))
                new TelegramWorker(key).Start().Wait();
            else
            {
                Console.WriteLine("Enter key");
                key = Console.ReadLine();
                if (!string.IsNullOrEmpty(key))
                    new TelegramWorker(key).Start().Wait();
                else
                {
                    Console.WriteLine("Key must be set");
                    Environment.Exit(1);
                }
            }
        }
    }
}