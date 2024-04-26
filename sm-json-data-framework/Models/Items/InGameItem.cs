using sm_json_data_framework.Models.Raw.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Items
{
    /// <summary>
    /// Represents an item that is explicitly identified as such in the game.
    /// </summary>
    public class InGameItem : Item
    {
        public string Data { get; set; }

        public InGameItem()
        {

        }

        public InGameItem(RawInGameItem item): base(item.Name)
        {
            Data = item.Data;
        }
    }
}
