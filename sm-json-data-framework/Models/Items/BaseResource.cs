using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Items
{
    public class BaseResource
    {
        public ConsumableResourceEnum Resource { get; set; }

        public int MaxAmount { get; set; }
    }
}
