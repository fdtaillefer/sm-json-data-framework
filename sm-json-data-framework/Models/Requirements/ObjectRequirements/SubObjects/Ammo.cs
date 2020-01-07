using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Requirements.ObjectRequirements.SubObjects
{
    public class Ammo : AbstractObjectLogicalElement
    {
        public AmmoEnum Type { get; set; }
        
        public int Count { get; set; }
    }
}
