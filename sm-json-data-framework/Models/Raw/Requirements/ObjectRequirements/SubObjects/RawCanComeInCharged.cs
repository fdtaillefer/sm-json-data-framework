using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
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
    public class RawCanComeInCharged: AbstractRawObjectLogicalElement
    {
        public int FromNode { get; set; }

        public IList<int> InRoomPath { get; set; } = new List<int>();

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        public override AbstractLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(ObjectLogicalElementTypeEnum.CanComeInCharged, out Type type))
            {
                CanComeInCharged canComeInCharged = (CanComeInCharged)Activator.CreateInstance(type);
                canComeInCharged.FromNodeId = FromNode;
                canComeInCharged.FramesRemaining = FramesRemaining;
                canComeInCharged.ShinesparkFrames = ShinesparkFrames;
                if (InRoomPath != null)
                {
                    canComeInCharged.InRoomPath = new List<int>(InRoomPath);
                }
                return canComeInCharged;
            }
            else
            {
                throw new Exception($"Could not obtain a logical element type for CanComeInCharged.");
            }
        }
    }
}
