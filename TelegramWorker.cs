﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InformationWaves
{
    /// <summary>
    /// Класс для работы с Telegram API
    /// </summary>
    internal class TelegramWorker
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
        /// Ключ для работы с API
        /// </summary>
        string key;
        /// <summary>
        /// Объект-сигнал, для остановки работы
        /// </summary>
        CancellationTokenSource cancellationSource;

        public TelegramWorker(string key)
        {
            this.key = key;
            cancellationSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Запуск асинхронного процесса приёма обновлений с сервера и последующая обработка
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            HttpClient client = new HttpClient();
            int offset = 0;
            var ctx = new Entities.InformationWaveContext();

            while (!cancellationSource.IsCancellationRequested)
            {
                try
                {
                    var msg = await client.PostAsync($"{TG}{key}/getUpdates", 
                        JsonContent.Create(new 
                        {
                            offset = offset, 
                            timeout = TIMEOUT, 
                            allowed_updates  = JsonSerializer.Serialize(AllowedUpdates)
                        }), cancellationSource.Token);
                    var res = await JsonSerializer.DeserializeAsync<Telegram.Response<Telegram.Update[]>>
                        (msg.EnsureSuccessStatusCode().Content.ReadAsStream());

                    if (res != null)
                    {
                        foreach (var u in res.result)
                        {
                            if (u.update_id >= offset)
                                offset = u.update_id + 1;

                            ctx.Add(new Entities.Social
                            {
                                Name = u.message.from.username,
                                Group = u.message.chat.title ?? "",
                                Text = u.message.text,
                                Date = u.message.date.ToString(),
                                Link = "123",
                                Author = u.message.from.first_name,
                                AuthorId = u.message.from.username,
                                Review = false,
                                Annotation = "123",
                                Keywords = "123",
                            });
                            await ctx.SaveChangesAsync();
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(ex.Message);//TODO
                }
            }
        }

        /// <summary>
        /// Остановка сервиса
        /// </summary>
        public void Stop()
        {
            cancellationSource.Cancel();
        }
    }
}
