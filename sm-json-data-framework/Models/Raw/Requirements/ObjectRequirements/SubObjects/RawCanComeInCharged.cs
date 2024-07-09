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

        public int ExcessShinesparkFrames { get; set; }

        public override IUnfinalizedLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(ObjectLogicalElementTypeEnum.CanComeInCharged, out Type type))
            {
                UnfinalizedCanComeInCharged canComeInCharged = (UnfinalizedCanComeInCharged)Activator.CreateInstance(type);
                canComeInCharged.FromNodeId = FromNode;
                canComeInCharged.FramesRemaining = FramesRemaining;
                canComeInCharged.ShinesparkFrames = ShinesparkFrames;
                canComeInCharged.ExcessShinesparkFrames = ExcessShinesparkFrames;
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
