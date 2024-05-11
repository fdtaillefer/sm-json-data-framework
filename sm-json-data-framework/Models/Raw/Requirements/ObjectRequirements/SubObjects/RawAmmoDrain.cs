using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.SubObjects
{
    public class RawAmmoDrain : AbstractRawObjectLogicalElement
    {
        public AmmoEnum Type { get; set; }

        public int Count { get; set; }

        public override IUnfinalizedLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(ObjectLogicalElementTypeEnum.AmmoDrain, out Type type))
            {
                UnfinalizedAmmoDrain ammoDrain = (UnfinalizedAmmoDrain)Activator.CreateInstance(type);
                ammoDrain.AmmoType = Type;
                ammoDrain.Count = Count;
                return ammoDrain;
            }
            else
            {
                throw new Exception($"Could not obtain a logical element type for AmmoDrain.");
            }
        }
    }
}
