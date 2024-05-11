using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements
{
    public class RawObjectLogicalElementWithString: AbstractRawObjectLogicalElement
    {
        public string PropertyName { get; }
        public string Value { get; }

        public RawObjectLogicalElementWithString(string propertyName, string value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        public override IUnfinalizedLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {

            // Convert property name to logicalElementEnum
            ObjectLogicalElementTypeEnum elementTypeEnum
                = (ObjectLogicalElementTypeEnum)Enum.Parse(typeof(ObjectLogicalElementTypeEnum), PropertyName, true);
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(elementTypeEnum, out Type type))
            {
                IUnfinalizedLogicalElement logicalElement =
                    (IUnfinalizedLogicalElement)Activator.CreateInstance(type, Value);
                return logicalElement;
            }
            else
            {
                throw new Exception($"The identifier {PropertyName} could not be matched to a string object logical element type to instantiate.");
            }
        }
    }
}
