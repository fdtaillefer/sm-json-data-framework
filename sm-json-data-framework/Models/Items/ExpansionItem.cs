using sm_json_data_framework.Models.Raw.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Items
{
    /// <summary>
    /// Represents an item that expands the available max amount of a resource.
    /// </summary>
    public class ExpansionItem : InGameItem
    {
        public RechargeableResourceEnum Resource { get; set; }

        public int ResourceAmount { get; set; }

        public ExpansionItem()
        {

        }

        public ExpansionItem(RawExpansionItem item): base(item)
        {
            Resource = item.Resource;
            ResourceAmount = item.ResourceAmount;
        }
    }
}
