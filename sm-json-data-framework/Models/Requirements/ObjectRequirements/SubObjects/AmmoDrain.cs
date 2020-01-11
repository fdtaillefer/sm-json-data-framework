using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    public class AmmoDrain : AbstractObjectLogicalElement
    {
        public AmmoEnum Type { get; set; }

        public int Count { get; set; }
    }
}
