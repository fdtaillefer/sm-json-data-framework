using sm_json_data_parser.Models.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Requirements.StringRequirements
{

    public class ItemLogicalElement : AbstractStringLogicalElement
    {
        private Item Item { get; set; }

        public ItemLogicalElement(Item item)
        {
            Item = item;
        }
    }
}
