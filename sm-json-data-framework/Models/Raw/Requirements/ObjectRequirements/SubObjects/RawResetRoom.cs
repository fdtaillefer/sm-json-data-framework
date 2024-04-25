using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.SubObjects
{
    public class RawResetRoom: AbstractRawObjectLogicalElement
    {
        public IEnumerable<int> Nodes { get; set; } = Enumerable.Empty<int>();

        public IEnumerable<int> NodesToAvoid { get; set; } = Enumerable.Empty<int>();

        public IEnumerable<string> ObstaclesToAvoid { get; set; } = Enumerable.Empty<string>();

        public bool MustStayPut { get; set; } = false;

        public override AbstractLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(ObjectLogicalElementTypeEnum.ResetRoom, out Type type))
            {
                ResetRoom resetRoom = (ResetRoom)Activator.CreateInstance(type);
                resetRoom.NodeIds = new List<int>(Nodes);
                resetRoom.NodeIdsToAvoid = new List<int>(NodesToAvoid);
                resetRoom.ObstaclesIdsToAvoid = new List<string>(ObstaclesToAvoid);
                resetRoom.MustStayPut = MustStayPut;
                return resetRoom;
            }
            else
            {
                throw new Exception($"Could not obtain a logical element type for ResetRoom.");
            }
        }
    }
}
