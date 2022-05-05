using System;
using System.Threading.Tasks;

namespace InformationWaves
{
    class Program
    {
        public static void Main(params string[] args)
        {
            string key = null;
            if (args.Length > 0)
                key = Environment.GetEnvironmentVariable("api_key");
            if(string.IsNullOrEmpty(key))
            {
                Console.WriteLine("Enter key");
                key = Console.ReadLine();
                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("Key must be set");
                    Environment.Exit(1);
                }
            }

            var worker = new TelegramWorker(key);
            ConsoleCancelEventHandler handler =
                (object sender, ConsoleCancelEventArgs e) =>
                {
                    e.Cancel = true;
                    if (e.SpecialKey == ConsoleSpecialKey.ControlC)
                        worker.Stop();
                };
            Console.CancelKeyPress += handler;
            worker.Start().Wait();
            Console.CancelKeyPress -= handler;
        }
    }
}