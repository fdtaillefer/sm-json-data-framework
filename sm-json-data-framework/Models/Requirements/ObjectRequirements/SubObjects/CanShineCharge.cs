using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    /// <summary>
    /// A logical element which requires Samus to charge a blue suit and possibly use it to shinespark (if it has any shinespark frames).
    /// </summary>
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

        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            bool mustShinespark = ShinesparkFrames > 0;

            // Always need the SpeedBooster to charge a bluesuit
            if (!inGameState.HasSpeedBooster())
            {
                return null;
            }

            // If a shinespark is needed, the tech must be allowed
            if (mustShinespark && !model.CanShinespark())
            {
                return null;
            }

            // The runway must be long enough to charge
            if (model.Rules.CalculateEffectiveRunwayLength(this, model.LogicalOptions.TilesSavedWithStutter) < model.LogicalOptions.TilesToShineCharge)
            {
                return null;
            }

            // If we have enough energy for the shinespark, consume it and return the result
            int energyCost = model.Rules.CalculateEnergyNeededForShinespark(ShinesparkFrames);
            if (inGameState.IsResourceAvailable(model, ConsumableResourceEnum.ENERGY, energyCost))
            {
                InGameState resultingState = inGameState.Clone();
                resultingState.ApplyConsumeResource(model, ConsumableResourceEnum.ENERGY, energyCost);
                ExecutionResult result = new ExecutionResult(resultingState);
                result.AddItemsInvolved(new Item[] { model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME] });
                return result;
            }
            // If we don't have enough for the shinespark, we cannot do this
            else
            {
                return null;
            }
        }
    }
}
