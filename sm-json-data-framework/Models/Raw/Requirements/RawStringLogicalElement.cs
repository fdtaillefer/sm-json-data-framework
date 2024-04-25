using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Techs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements
{
    public class RawStringLogicalElement : AbstractRawLogicalElement
    {
        public string Value { get; set; }

        public override AbstractLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            // Figure out what this is
            if (Equals(Value, "never"))
            {
                Type neverType = knowledgeBase.StringLogicalElementTypes[StringLogicalElementTypeEnum.Never];
                NeverLogicalElement logicalElement = (NeverLogicalElement) Activator.CreateInstance(neverType);
                return logicalElement;
            }
            // If the string is the name of a helper that's in the knowledgeBase, return an appropriate logical element
            else if (knowledgeBase.Helpers.TryGetValue(Value, out Helper helper))
            {
                Type helperType = knowledgeBase.StringLogicalElementTypes[StringLogicalElementTypeEnum.Helper];
                HelperLogicalElement logicalElement = (HelperLogicalElement)Activator.CreateInstance(helperType, helper);
                return logicalElement;
            }
            // If the string is the name of a tech that's in the knowledgeBase, return an appropriate logical element
            else if (knowledgeBase.Techs.TryGetValue(Value, out Tech tech))
            {
                Type techType = knowledgeBase.StringLogicalElementTypes[StringLogicalElementTypeEnum.Tech];
                TechLogicalElement logicalElement = (TechLogicalElement)Activator.CreateInstance(techType, tech);
                return logicalElement;
            }
            // If the string is the name of an item that's in the knowledgeBase, return an appropriate logical element
            else if (knowledgeBase.Items.TryGetValue(Value, out Item item))
            {
                Type itemType = knowledgeBase.StringLogicalElementTypes[StringLogicalElementTypeEnum.Item];
                ItemLogicalElement logicalElement = (ItemLogicalElement)Activator.CreateInstance(itemType, item);
                return logicalElement;
            }
            // If the string is the name of a game flag that's already in the model...
            else if (knowledgeBase.GameFlags.TryGetValue(Value, out GameFlag gameFlag))
            {
                Type gameFlagType = knowledgeBase.StringLogicalElementTypes[StringLogicalElementTypeEnum.Gameflag];
                GameFlagLogicalElement logicalElement = (GameFlagLogicalElement)Activator.CreateInstance(gameFlagType, item);
                return logicalElement;
            }
            // If the string matched nothing that's in the model maybe it's referencing something that's
            // not been read and put in the model yet.
            // Return an uninterpreted string logical element. This will need to be replaced before initialization is done.
            else if (knowledgeBase.AllowUninterpretedStringLogicalElements)
            {
                return new UninterpretedStringLogicalElement(Value);
            }
            else
            {
                throw new JsonException($"Logical element string {Value} could not be matched to anything.");
            }
        }
    }
}
