using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Requirements.ObjectRequirements.Strings
{
    public abstract class AbstractObjectLogicalElementWithString : AbstractObjectLogicalElement
    {
        public string Value { get; set; }
    }
}
