using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    public class CanShineCharge : AbstractObjectLogicalElement, IRunway
    {
        [JsonIgnore]
        public int Length { get => UsedTiles; }

        [JsonIgnore]
        public int EndingUpTiles { get => 0; }

        public int UsedTiles { get; set; }

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        [JsonPropertyName("openEnd")]
        public int OpenEnds { get; set; } = 0;

        public int ShinesparkFrames { get; set; }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            // No properties need to be handled here
            return Enumerable.Empty<string>();
        }

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, bool usePreviousRoom = false)
        {
            bool mustShinespark = ShinesparkFrames > 0;
            // Must have SpeedBooster and must be able to charge in the current runway
            // If a shinespark is involved, the shinespark tech must be enabled and must have the energy for the Shinespark (spent energy + 29)
            return inGameState.HasSpeedBooster()
                && model.LogicalOptions.TilesToShineCharge >= model.Rules.CalculateEffectiveRunwayLength(this)
                && (!mustShinespark || 
                    (model.CanShinespark() && inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, model.Rules.CalculateEnergyNeededForShinespark(inGameState, ShinesparkFrames))));
        }
    }
}
