using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Items
{
    public class RawExpansionItem: RawInGameItem
    {
        public RechargeableResourceEnum Resource { get; set; }

        public int ResourceAmount { get; set; }
    }
}
