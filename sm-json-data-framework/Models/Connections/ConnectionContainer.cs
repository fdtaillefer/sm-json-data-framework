﻿using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Connections
{
    public class ConnectionContainer
    {
        public IEnumerable<JsonConnection> Connections { get; set; }
    }
}
