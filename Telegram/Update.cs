using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformationWaves.Telegram
{
    /// <summary>
    /// Струдктура проиходящих обновлений (событий). Например, добавление в группу, канал, отправка сообщений другими пользователями
    /// </summary>
    internal class Update
    {
        public int update_id { get; set; }

        public Message message { get; set; }
    }
}
