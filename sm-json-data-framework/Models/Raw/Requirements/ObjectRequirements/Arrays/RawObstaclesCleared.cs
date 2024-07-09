using sm_json_data_framework.Models.Requirements.ObjectRequirements.Arrays;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.Arrays
{
    public class RawObstaclesCleared : AbstractRawObjectLogicalElementWithArray<string>
    {
        public RawObstaclesCleared(IList<string> items) : base(items)
        {

        }

        public override IUnfinalizedLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            return new UnfinalizedObstaclesCleared(new List<string>(Value));
        }
    }
}
