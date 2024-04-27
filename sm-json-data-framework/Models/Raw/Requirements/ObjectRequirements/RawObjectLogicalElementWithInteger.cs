using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements
{
    public class RawObjectLogicalElementWithInteger : AbstractRawObjectLogicalElement
    {
        public string PropertyName { get; }
        public int Value { get; }

        public RawObjectLogicalElementWithInteger(string propertyName, int value) {
            PropertyName = propertyName;
            Value = value;
        }

        public override AbstractLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            
            // Convert property name to logicalElementEnum
            ObjectLogicalElementTypeEnum elementTypeEnum
                = (ObjectLogicalElementTypeEnum)Enum.Parse(typeof(ObjectLogicalElementTypeEnum), PropertyName, true);
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(elementTypeEnum, out Type type))
            {
                AbstractObjectLogicalElementWithInteger logicalElement =
                    (AbstractObjectLogicalElementWithInteger)Activator.CreateInstance(type, Value);
                return logicalElement;
            }
            else
            {
                throw new Exception($"The identifier {PropertyName} could not be matched to an integer object logical element type to instantiate.");
            }
        }
    }
}
