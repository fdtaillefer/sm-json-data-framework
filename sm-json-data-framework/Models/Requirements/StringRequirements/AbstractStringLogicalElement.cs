using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.StringRequirements
{
    /// <summary>
    /// And abstract superclass for logical elements that have an inner string value.
    /// </summary>
    public abstract class AbstractStringLogicalElement : AbstractLogicalElement
    {
        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            // String logical elements don't have properties
            return Enumerable.Empty<string>();
        }
    }
}
