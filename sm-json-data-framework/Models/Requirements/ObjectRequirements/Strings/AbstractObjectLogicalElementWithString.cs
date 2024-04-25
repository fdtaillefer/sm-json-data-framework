using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings
{
    public abstract class AbstractObjectLogicalElementWithString : AbstractObjectLogicalElement
    {
        public AbstractObjectLogicalElementWithString()
        {

        }

        public AbstractObjectLogicalElementWithString(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}
