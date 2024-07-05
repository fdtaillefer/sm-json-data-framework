using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Arrays;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.Arrays
{
    public class RawResourceCapacityLogicalElement : AbstractRawObjectLogicalElementWithArray<RawResourceCapacityLogicalElementItem>
    {
        public RawResourceCapacityLogicalElement(IList<RawResourceCapacityLogicalElementItem> items):base(items)
        {

        }

        public override IUnfinalizedLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            return new UnfinalizedResourceCapacityLogicalElement(Value.Select(rawResourceCapacityItem => new UnfinalizedResourceCapacityLogicalElementItem(rawResourceCapacityItem, knowledgeBase)).ToList());
        }
    }
}
