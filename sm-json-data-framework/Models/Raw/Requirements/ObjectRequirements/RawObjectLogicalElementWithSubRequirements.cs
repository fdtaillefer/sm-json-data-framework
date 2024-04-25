using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements
{
    public class RawObjectLogicalElementWithSubRequirements: AbstractRawObjectLogicalElement
    {
        public string PropertyName { get; set; }

        public RawLogicalRequirements LogicalRequirements { get; set; }

        public override AbstractLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            LogicalRequirements convertedRequirements = LogicalRequirements.ToLogicalRequirements(knowledgeBase);
            // Convert property name to logicalElementEnum
            ObjectLogicalElementTypeEnum elementTypeEnum
                = (ObjectLogicalElementTypeEnum)Enum.Parse(typeof(ObjectLogicalElementTypeEnum), PropertyName, true);
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(elementTypeEnum, out Type type))
            {
                return (AbstractObjectLogicalElementWithSubRequirements)Activator.CreateInstance(type, convertedRequirements);
            } else
            {
                throw new Exception($"The identifier {PropertyName} could not be matched to a sub-requirements logical element type to instantiate.");
            }
        }
    }
}
