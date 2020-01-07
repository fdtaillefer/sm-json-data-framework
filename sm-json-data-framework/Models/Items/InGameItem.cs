using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Items
{
    /// <summary>
    /// Represents an item that is explicitly identified as such in the game.
    /// </summary>
    public class InGameItem : Item
    {
        public string Data { get; set; }
    }
}
