using System;
using System.Threading.Tasks;

namespace InformationWaves
{
    class Program
    {
        public static void Main(params string[] args)
        {
            var worker = new TelegramWorker();

            ConsoleCancelEventHandler consoleCancelHandler =
                (object sender, ConsoleCancelEventArgs e) =>
                {
                    e.Cancel = true;
                    if (e.SpecialKey == ConsoleSpecialKey.ControlC)
                        worker.Stop();
                };

            Console.CancelKeyPress += consoleCancelHandler;
            worker.Start().Wait();
            Console.CancelKeyPress -= consoleCancelHandler;
        }
    }
}