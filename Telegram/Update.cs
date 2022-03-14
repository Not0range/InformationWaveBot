using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformationWaves.Telegram
{
    internal class Update
    {
        public int update_id { get; set; }

        public Message message { get; set; }
    }
}
