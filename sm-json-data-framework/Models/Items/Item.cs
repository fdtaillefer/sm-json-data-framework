using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Items
{
    /// <summary>
    /// Represents an item, regardless of whether this is explicitly an item in the game or just implicitly an item.
    /// </summary>
    public class Item
    {
        public string Name { get; set; }

        public Item() { }

        public Item (string name)
        {
            Name = name;
        }
    }
}
