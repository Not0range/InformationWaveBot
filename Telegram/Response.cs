﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InformationWaves.Telegram
{
    internal class Response<T>
    {
        public bool ok { get; set; }

        public T result { get; set; }
    }
}
