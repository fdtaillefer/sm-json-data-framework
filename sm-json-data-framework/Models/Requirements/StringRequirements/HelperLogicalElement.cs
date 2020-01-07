using sm_json_data_parser.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Requirements.StringRequirements
{
    public class HelperLogicalElement : AbstractStringLogicalElement
    {
        private Helper Helper { get; set; }

        public HelperLogicalElement(Helper helper)
        {
            Helper = helper;
        }
    }
}
