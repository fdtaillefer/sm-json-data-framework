using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Techs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements
{
    public class RawStringLogicalElement : AbstractRawLogicalElement
    {
        public string Value { get; }

        public RawStringLogicalElement(string value)
        {
            Value = value;
        }

        public override IUnfinalizedLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            // Figure out what this is
            if (Equals(Value, "never"))
            {
                Type neverType = knowledgeBase.StringLogicalElementTypes[StringLogicalElementTypeEnum.Never];
                UnfinalizedNeverLogicalElement logicalElement = (UnfinalizedNeverLogicalElement) Activator.CreateInstance(neverType);
                return logicalElement;
            }
            // If the string is the name of a helper that's in the knowledgeBase, return an appropriate logical element
            else if (knowledgeBase.Helpers.TryGetValue(Value, out UnfinalizedHelper helper))
            {
                Type helperType = knowledgeBase.StringLogicalElementTypes[StringLogicalElementTypeEnum.Helper];
                UnfinalizedHelperLogicalElement logicalElement = (UnfinalizedHelperLogicalElement)Activator.CreateInstance(helperType, helper);
                return logicalElement;
            }
            // If the string is the name of a tech that's in the knowledgeBase, return an appropriate logical element
            else if (knowledgeBase.Techs.TryGetValue(Value, out UnfinalizedTech tech))
            {
                Type techType = knowledgeBase.StringLogicalElementTypes[StringLogicalElementTypeEnum.Tech];
                UnfinalizedTechLogicalElement logicalElement = (UnfinalizedTechLogicalElement)Activator.CreateInstance(techType, tech);
                return logicalElement;
            }
            // If the string is the name of an item that's in the knowledgeBase, return an appropriate logical element
            else if (knowledgeBase.Items.TryGetValue(Value, out UnfinalizedItem item))
            {
                Type itemType = knowledgeBase.StringLogicalElementTypes[StringLogicalElementTypeEnum.Item];
                UnfinalizedItemLogicalElement logicalElement = (UnfinalizedItemLogicalElement)Activator.CreateInstance(itemType, item);
                return logicalElement;
            }
            // If the string is the name of a game flag that's already in the model...
            else if (knowledgeBase.GameFlags.TryGetValue(Value, out UnfinalizedGameFlag gameFlag))
            {
                Type gameFlagType = knowledgeBase.StringLogicalElementTypes[StringLogicalElementTypeEnum.Gameflag];
                UnfinalizedGameFlagLogicalElement logicalElement = (UnfinalizedGameFlagLogicalElement)Activator.CreateInstance(gameFlagType, gameFlag);
                return logicalElement;
            }
            // Since all raw elements have already been read and are in the knowledge base, having no match by now means
            // we will not be able to interpret this logical element. Return an error.
            else
            {
                throw new JsonException($"Logical element string {Value} could not be matched to anything.");
            }
        }
    }
}
