using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformationWaves.Telegram
{
    internal class Message
    {
        public int messasge_id { get; set; }

        public User from { get; set; }

        public int date { get; set; }

        public Chat chat { get; set; }

        public string text { get; set; }
}
}
