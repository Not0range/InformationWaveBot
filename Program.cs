using System;
using System.Threading.Tasks;

namespace InformationWaves
{
    class Program
    {
        const string CHANNEL_FILENAME = "channels.txt";
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

            if (!File.Exists(CHANNEL_FILENAME))
                File.Create(CHANNEL_FILENAME).Close();

            var worker = new TelegramWorker(key);
            ReadChannelList(worker, false);

            var watcher = new FileSystemWatcher(Environment.CurrentDirectory, CHANNEL_FILENAME);
            watcher.EnableRaisingEvents = true;
            FileSystemEventHandler changedHandler =
                (object sender, FileSystemEventArgs e) =>
                {
                    if (e.ChangeType != WatcherChangeTypes.Changed)
                        return;
                    ReadChannelList(worker);
                };
            FileSystemEventHandler removeHandler =
                (object sender, FileSystemEventArgs e) =>
                {
                    if (!File.Exists(CHANNEL_FILENAME))
                        File.Create(CHANNEL_FILENAME).Close();
                    ReadChannelList(worker);
                };
            RenamedEventHandler renameHandler =
                (object sender, RenamedEventArgs e) =>
                {
                    if (!File.Exists(CHANNEL_FILENAME))
                        File.Create(CHANNEL_FILENAME).Close();
                    ReadChannelList(worker);
                };
            ErrorEventHandler errorHandler =
                (object sender, ErrorEventArgs e) =>
                {
                    Console.WriteLine(e.GetException());//todo
                };

            watcher.Changed += changedHandler;
            watcher.Deleted += removeHandler;
            watcher.Renamed += renameHandler;
            watcher.Error += errorHandler;

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

            watcher.Changed -= changedHandler;
            watcher.Deleted -= removeHandler;
            watcher.Renamed -= renameHandler;
            watcher.Dispose();
        }

        static void ReadChannelList(TelegramWorker worker, bool blockMutex = true)
        {
            Console.WriteLine("Read allowed channels list file...");//todo
            var strs = File.ReadAllLines(CHANNEL_FILENAME).Where(t => !string.IsNullOrWhiteSpace(t));
            if (strs.Count() == 0)
            {
                worker.UpdateAllowedChannels(Array.Empty<string>(), blockMutex);
                return;
            }
            var channels = strs.Select(t => t.Trim()).Where(t => !t.StartsWith('#'));
            worker.UpdateAllowedChannels(channels.ToArray(), blockMutex);
        }
    }
}