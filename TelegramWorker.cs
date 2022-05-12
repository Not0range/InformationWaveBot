using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InformationWaves
{
    /// <summary>
    /// Класс для работы с Telegram API
    /// </summary>
    public class TelegramWorker : IDisposable
    {
        /// <summary>
        /// Основной URL для работы API
        /// </summary>
        const string TG = "https://api.telegram.org/bot";
        /// <summary>
        /// Максимальное время ожидания обновления
        /// </summary>
        const int TIMEOUT = 30;

        /// <summary>
        /// Перечень принимаемых обновлений
        /// </summary>
        string[] AllowedUpdates { get; } = new string[]
        {
            "message",
            "channel_post"
        };
        /// <summary>
        /// Список каналов, с которых обрабатываются обновления
        /// </summary>
        public string[] allowedChannels { get; private set; }
        /// <summary>
        /// Наблюдатель за изменениями файла списка допустимых каналов
        /// </summary>
        ChannelsWatcher watcher;

        /// <summary>
        /// Ключ для работы с API
        /// </summary>
        readonly string key;
        /// <summary>
        /// Объект-сигнал, для остановки работы
        /// </summary>
        readonly CancellationTokenSource cancellationSource;
        /// <summary>
        /// Синхронизирующий объект; не позволяет изменить перечень допустимых каналов в момент обработки обновлений
        /// </summary>
        readonly Mutex mutex;

        /// <summary>
        /// Смещение обновлений
        /// </summary>
        long offset = 0;

        /// <summary>
        /// Конструктор но умолчанию
        /// </summary>
        public TelegramWorker()
        {
            key = Environment.GetEnvironmentVariable("api_key");
            if (string.IsNullOrWhiteSpace(key))
                new ArgumentNullException(null, "В переменных окружения api_key не задан ключ.");
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("watch_channels_change")))
            {
                watcher = new ChannelsWatcher();
                allowedChannels = watcher.GetChannels();
                watcher.ChannelsUpdate += UpdateAllowedChannels;
            }
            else
                allowedChannels = Array.Empty<string>();

            cancellationSource = new CancellationTokenSource();
            mutex = new Mutex();
        }

        /// <summary>
        /// Деструктор
        /// </summary>
        ~TelegramWorker()
        {
            Dispose();
        }

        /// <summary>
        /// Запуск асинхронного процесса приёма обновлений с сервера и последующая их обработка
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            var client = new HttpClient();

            try
            {
                while (!cancellationSource.IsCancellationRequested)
                {
                    try
                    {
                        var response = await GetUpdates(client);

                        if (response != null && response.result.Any())
                        {
                            OffsetHandle(response);
                            await ChannelPostHandle(response, client);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.StatusCode == HttpStatusCode.Unauthorized)
                            throw new ApplicationException("Ключ задан неверно", ex);
                        Console.WriteLine(ex.Message);//TODO
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Завершение работы...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}. Завершение работы...");
                Dispose();
            }
            finally
            {
                client.Dispose();
            }
        }

        /// <summary>
        /// Остановка сервиса
        /// </summary>
        public void Stop()
        {
            watcher.Dispose();
            cancellationSource.Cancel();
            mutex.Close();
        }

        /// <summary>
        /// Заменяет список допустимых каналов
        /// </summary>
        /// <param name="channels">Массив, содержащий новый список допустимых каналов</param>
        public void UpdateAllowedChannels(string[] channels)
        {
            mutex.WaitOne();
            allowedChannels = channels;
            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Запрос на получение обновлений
        /// </summary>
        /// <param name="client">Веб-клиент выполнения запросов</param>
        /// <returns>Задача, представляющая асинхронную операцию получения данных с удалённого сервера</returns>
        private async Task<Telegram.Response<Telegram.Update[]>> GetUpdates(HttpClient client)
        {
            var msg = await client.PostAsync($"{TG}{key}/getUpdates",
                            JsonContent.Create(new
                            {
                                offset,
                                timeout = TIMEOUT,
                                allowed_updates = JsonSerializer.Serialize(AllowedUpdates)
                            }), cancellationSource.Token);
            return await JsonSerializer.DeserializeAsync<Telegram.Response<Telegram.Update[]>>
                (msg.EnsureSuccessStatusCode().Content.ReadAsStream());
        }

        /// <summary>
        /// Обработчик смещения обновлений
        /// </summary>
        /// <param name="response">Ответ, полученный от удалённого сервера</param>
        /// <exception cref="ArgumentNullException"></exception>
        private void OffsetHandle(Telegram.Response<Telegram.Update[]> response)
        {
            if (response == null || !response.result.Any())
                throw new ArgumentNullException(nameof(response));

            var max = response.result.Max(t => t.update_id);
            if (max >= offset)
                offset = max + 1;
        }

        /// <summary>
        /// Обработчик сообщений каналов
        /// </summary>
        /// <param name="response">Ответ, полученный от удалённого сервера</param>
        /// <param name="client">Веб-клиент выполения запросов</param>
        /// <returns>Задача, представляющая асинхронную операцию обработки сообщений каналов</returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task ChannelPostHandle(Telegram.Response<Telegram.Update[]> response, HttpClient client)
        {
            if (response == null || !response.result.Any())
                throw new ArgumentNullException(nameof(response));

            mutex.WaitOne();
            foreach (var msg in response.result.Where(t => t.channel_post != null)
                .Select(t => t.channel_post))
            {
                if (allowedChannels.Length > 0 && !allowedChannels.Any(t => t.Equals(msg.chat.username,
                    StringComparison.OrdinalIgnoreCase)) || string.IsNullOrWhiteSpace(msg.text))
                    continue;

                var words = Regex.Split(msg.text, @"(\p{P}|\s)+").Where(t => !string.IsNullOrWhiteSpace(t));

                var ctx = new Entities.InformationWaveContext();
                try
                {
                    ctx.Add(new Entities.Social
                    {
                        Name = msg.chat.title,
                        Group = msg.chat.title,
                        Text = msg.text,
                        Date = msg.date.ToString(),
                        Link = $"https://t.me/{msg.chat.username}/{msg.message_id}",
                        Author = msg.author_signature ?? msg.chat.title,
                        AuthorId = msg.chat.username,
                        IsComment = false,
                        DateView = DateTimeOffset.FromUnixTimeSeconds(msg.date).DateTime,
                        Annotation = words.First(),
                        Keywords = string.Join(", ", words),
                    });
                    await ctx.SaveChangesAsync();
                    Console.WriteLine($"Added {msg.text} from {msg.chat.title}");//todo;
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine(ex.Message);//todo
                }
                finally
                {
                    await ctx.DisposeAsync();
                }
            }
            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Запрос на поулчение полных данных о чате
        /// </summary>
        /// <param name="client">Веб-клиент выполения запросов</param>
        /// <param name="chat">Объект чата, для которого необходимо получить информацию</param>
        /// <returns>Задача, представляющая асинхронную операцию получения полной информации о чате</returns>
        /// <exception cref="NullReferenceException"></exception>
        private async Task GetChatInfo(HttpClient client, Telegram.Chat chat)
        {
            var msg = await client.PostAsync($"{TG}{key}/getChat",
                                JsonContent.Create(new
                                {
                                    chat_id = chat.id
                                }), cancellationSource.Token);
            var c = await JsonSerializer.DeserializeAsync<Telegram.Response<Telegram.Chat>>
                (msg.EnsureSuccessStatusCode().Content.ReadAsStream());

            if (c == null || c.result == null ) 
                throw new NullReferenceException("Сервер ничего не вернул");

            chat.invite_link = c.result.invite_link;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
