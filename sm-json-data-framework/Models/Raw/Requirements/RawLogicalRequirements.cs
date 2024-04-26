using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements
{
    public class RawLogicalRequirements
    {
        public RawLogicalRequirements()
        {

        }

        public RawLogicalRequirements(IEnumerable<AbstractRawLogicalElement> logicalElements)
        {
            LogicalElements = LogicalElements.Concat(logicalElements);
        }

        public IEnumerable<AbstractRawLogicalElement> LogicalElements { get; private set; } = Enumerable.Empty<AbstractRawLogicalElement>();

        public LogicalRequirements ToLogicalRequirements(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            return new LogicalRequirements(LogicalElements.Select(element => element.ToLogicalElement(knowledgeBase)));
        }
    }
}
