﻿using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    /// <summary>
    /// A logical element which requires Samus spend a fixed amount of a specific ammo type.
    /// </summary>
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

        public override bool IsFulfilled(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            return inGameState.IsResourceAvailable(AmmoType.GetConsumableResourceEnum(), Count * times);
        }

        public override InGameState AttemptFulfill(SuperMetroidModel model, InGameState inGameState, int times = 1, bool usePreviousRoom = false)
        {
            int ammoCost = Count * times;
            if (inGameState.IsResourceAvailable(AmmoType.GetConsumableResourceEnum(), ammoCost))
            {
                inGameState = inGameState.Clone();
                inGameState.ConsumeResource(AmmoType.GetConsumableResourceEnum(), ammoCost);
                return inGameState;
            }
            else
            {
                return null;
            }
        }
    }
}
