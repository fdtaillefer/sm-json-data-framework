using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.SubObjects
{
    public class RawAdjacentRunway : AbstractRawObjectLogicalElement
    {
        public int FromNode { get; set; }

        public IList<int> InRoomPath { get; set; } = new List<int>();

        public decimal UsedTiles { get; set; }

        public ISet<PhysicsEnum> Physics { get; set; }

        public int UseFrames { get; set; } = 0;

        public bool OverrideRunwayRequirements { get; set; } = false;

        public override AbstractLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(ObjectLogicalElementTypeEnum.AdjacentRunway, out Type type))
            {
                AdjacentRunway adjacentRunway = (AdjacentRunway)Activator.CreateInstance(type);
                adjacentRunway.FromNodeId = FromNode;
                adjacentRunway.OverrideRunwayRequirements = OverrideRunwayRequirements;
                adjacentRunway.UsedTiles = UsedTiles;
                adjacentRunway.UseFrames = UseFrames;
                if(InRoomPath != null)
                {
                    adjacentRunway.InRoomPath = new List<int>(InRoomPath);
                }
                if (Physics != null)
                {
                    adjacentRunway.Physics = new HashSet<PhysicsEnum>(Physics);
                }
                return adjacentRunway;
            }
            else
            {
                throw new Exception($"Could not obtain a logical element type for AdjacentRunway.");
            }
        }
    }
}
