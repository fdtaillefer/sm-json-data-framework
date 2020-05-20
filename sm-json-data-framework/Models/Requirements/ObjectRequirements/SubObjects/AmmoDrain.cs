using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    /// <summary>
    /// A logical element that removes a fixed amount of a specific ammo from Samus, without requiring her to lose the full amount if she runs out.
    /// </summary>
    public class AmmoDrain : AbstractObjectLogicalElement
    {
        [JsonPropertyName("type")]
        public AmmoEnum AmmoType { get; set; }

        public int Count { get; set; }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            // No properties need to be handled here
            return Enumerable.Empty<string>();
        }

        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            int currentAmmo = inGameState.GetCurrentAmount(AmmoType.GetConsumableResourceEnum());
            int ammoCost = Math.Min(currentAmmo, Count);

            if (inGameState.IsResourceAvailable(model, AmmoType.GetConsumableResourceEnum(), ammoCost))
            {
                var resultingState = inGameState.Clone();
                resultingState.ApplyConsumeResource(model, AmmoType.GetConsumableResourceEnum(), ammoCost);
                return new ExecutionResult(resultingState);
            }
            else
            {
                return null;
            }
        }
    }
}
