using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.Arrays
{
    public class RawResourceCapacityLogicalElementItem
    {
        public RechargeableResourceEnum Type { get; set; }
        public int Count { get; set; }
    }
}
