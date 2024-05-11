using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.SubObjects
{
    public class RawAmmo: AbstractRawObjectLogicalElement
    {
        public AmmoEnum Type { get; set; }

        public int Count { get; set; }

        public override IUnfinalizedLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(ObjectLogicalElementTypeEnum.Ammo, out Type type))
            {
                UnfinalizedAmmo ammo = (UnfinalizedAmmo)Activator.CreateInstance(type);
                ammo.AmmoType = Type;
                ammo.Count = Count;
                return ammo;
            }
            else
            {
                throw new Exception($"Could not obtain a logical element type for Ammo.");
            }
        }
    }
}
