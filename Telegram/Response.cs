using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformationWaves.Telegram
{
    /// <summary>
    /// Структура, получаемая в ответ на запрос к серверу Telegram
    /// </summary>
    /// <typeparam name="T">Тип данных, возвращаемых в запросе (как правило, Update[])</typeparam>
    internal class Response<T>
    {
        public bool ok { get; set; }

        public T result { get; set; }
    }
}
