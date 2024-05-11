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
    public class RawCanShineCharge: AbstractRawObjectLogicalElement
    {
        public int UsedTiles { get; set; }

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        public int OpenEnd { get; set; } = 0;

        public int ShinesparkFrames { get; set; }

        public override IUnfinalizedLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(ObjectLogicalElementTypeEnum.CanShineCharge, out Type type))
            {
                UnfinalizedCanShineCharge canShineCharge = (UnfinalizedCanShineCharge)Activator.CreateInstance(type);
                canShineCharge.UsedTiles = UsedTiles;
                canShineCharge.GentleUpTiles = GentleUpTiles;
                canShineCharge.GentleDownTiles = GentleDownTiles;
                canShineCharge.SteepUpTiles = SteepUpTiles;
                canShineCharge.SteepDownTiles = SteepDownTiles;
                canShineCharge.StartingDownTiles = StartingDownTiles;
                canShineCharge.OpenEnds = OpenEnd;
                canShineCharge.ShinesparkFrames = ShinesparkFrames;
                return canShineCharge;
            }
            else
            {
                throw new Exception($"Could not obtain a logical element type for CanShineCharge.");
            }
        }
    }
}
