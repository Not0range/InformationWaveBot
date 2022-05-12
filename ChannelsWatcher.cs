using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformationWaves
{
    internal class ChannelsWatcher : IDisposable
    {
        /// <summary>
        /// Путь к файлу списка допустимых каналов по умолчанию
        /// </summary>
        const string DEFAULT_CHANNELS_FILEPATH = "channels.txt";
        /// <summary>
        /// Событие, возникающее при изменении списка допустимых каналов
        /// </summary>
        public event ChannelsUpdateHandler ChannelsUpdate;
        /// <summary>
        /// Путь к файлу списка допустимых каналов
        /// </summary>
        string channelsFilePath;
        /// <summary>
        /// Наблюдатель за изменениями в файловой системе
        /// </summary>
        FileSystemWatcher watcher;

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public ChannelsWatcher()
        {
            channelsFilePath = Environment.GetEnvironmentVariable("channels_file_path");
            if (string.IsNullOrEmpty(channelsFilePath))
                channelsFilePath = DEFAULT_CHANNELS_FILEPATH;
            if (!File.Exists(channelsFilePath))
                File.Create(channelsFilePath).Close();

            string fullPath = Path.GetFullPath(channelsFilePath);
            string pathToFolder = fullPath.Remove(fullPath.LastIndexOf(Path.DirectorySeparatorChar, 
                fullPath.Length - (Path.EndsInDirectorySeparator(fullPath) ? 2 : 1)));
            watcher = new FileSystemWatcher(pathToFolder, Path.GetFileName(channelsFilePath));
            watcher.Changed += Watcher_Changed;
            watcher.Renamed += Watcher_Renamed;
            watcher.Deleted += Watcher_Deleted;
            watcher.Error += Watcher_Error;
            watcher.EnableRaisingEvents = true;
        }
        /// <summary>
        /// Обработчик возникающих ошибок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine(e.GetException());//todo
        }
        /// <summary>
        /// Обработчик удаления файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(channelsFilePath))
                File.Create(channelsFilePath).Close();
            ChannelsUpdate?.Invoke(GetChannels());
        }
        /// <summary>
        /// Обработчик переименования файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (!File.Exists(channelsFilePath))
                File.Create(channelsFilePath).Close();
            ChannelsUpdate?.Invoke(GetChannels());
        }
        /// <summary>
        /// Обработчик изменения файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
                return;
            ChannelsUpdate?.Invoke(GetChannels());
        }

        public void Dispose()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Changed -= Watcher_Changed;
            watcher.Renamed -= Watcher_Renamed;
            watcher.Deleted -= Watcher_Deleted;
            watcher.Error -= Watcher_Error;
            watcher.Dispose();
        }
        /// <summary>
        /// Чтение списка каналов из файла
        /// </summary>
        /// <returns>Массив строк, содержащий username каналов</returns>
        public string[] GetChannels()
        {
            Console.WriteLine("Read allowed channels list file...");//todo
            var strs = File.ReadAllLines(channelsFilePath).Where(t => !string.IsNullOrWhiteSpace(t));
            if (strs.Count() == 0)
                return Array.Empty<string>();
            var channels = strs.Select(t => t.Trim()).Where(t => !t.StartsWith('#'));
            return channels.ToArray();
        }

        /// <summary>
        /// Делегат обработки изменения списка каналов
        /// </summary>
        /// <param name="channels">Новый массив каналов</param>
        public delegate void ChannelsUpdateHandler(string[] channels);
    }
}
