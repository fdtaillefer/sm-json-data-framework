using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Items
{
    public class BaseResource
    {
        public ConsumableResourceEnum Resource { get; set; }

        public int MaxAmount { get; set; }
    }
}
