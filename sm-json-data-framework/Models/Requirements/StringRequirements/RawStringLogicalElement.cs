using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Requirements.StringRequirements
{
    public class RawStringLogicalElement: AbstractStringLogicalElement
    {
        public RawStringLogicalElement(string stringValue)
        {
            StringValue = stringValue;
        }

        public string StringValue { get; set; }
    }
}
