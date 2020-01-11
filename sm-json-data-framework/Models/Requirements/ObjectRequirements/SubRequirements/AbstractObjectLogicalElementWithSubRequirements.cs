using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements
{
    public class AbstractObjectLogicalElementWithSubRequirements : AbstractObjectLogicalElement
    {
        public LogicalRequirements LogicalRequirements { get; set; }
    }
}
