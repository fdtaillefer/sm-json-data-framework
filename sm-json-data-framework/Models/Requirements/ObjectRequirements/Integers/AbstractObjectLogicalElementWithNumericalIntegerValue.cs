using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public abstract class AbstractObjectLogicalElementWithNumericalIntegerValue : AbstractObjectLogicalElementWithInteger
    {
        public AbstractObjectLogicalElementWithNumericalIntegerValue()
        {

        }

        public AbstractObjectLogicalElementWithNumericalIntegerValue(int value): base(value)
        {
            
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            // If the value is just a numerical value, it doesn't need to match up to anything
            return Enumerable.Empty<string>();
        }
    }
}
