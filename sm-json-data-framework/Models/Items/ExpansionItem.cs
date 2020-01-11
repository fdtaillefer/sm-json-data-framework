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
        public ConsumableResourceEnum Resource { get; set; }

        public int ResourceAmount { get; set; }
    }
}
