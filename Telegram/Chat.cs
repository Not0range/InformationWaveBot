using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformationWaves.Telegram
{
    internal class Chat
    {
        public long id { get; set; }

        public string type { get; set; }

        public string title { get; set; }

        public string username { get; set; }

        public string invite_link { get; set; }
    }
}
