using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    public class Ammo : AbstractObjectLogicalElement
    {
        [JsonPropertyName("type")]
        public AmmoEnum AmmoType { get; set; }
        
        public int Count { get; set; }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            // No properties need to be handled here
            return Enumerable.Empty<string>();
        }

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, bool usePreviousRoom = false)
        {
            return inGameState.IsResourceAvailable(AmmoType.GetConsumableResourceEnum(), Count);
        }
    }
}
