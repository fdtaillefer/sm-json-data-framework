﻿using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Options;
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
    public class AmmoDrain : AbstractObjectLogicalElement<UnfinalizedAmmoDrain, AmmoDrain>
    {
        public AmmoDrain(UnfinalizedAmmoDrain innerElement, Action<AmmoDrain> mappingsInsertionCallback) : base(innerElement, mappingsInsertionCallback)
        {
            AmmoType = innerElement.AmmoType;
            Count = innerElement.Count;
        }

        /// <summary>
        /// The type of ammo that is being drained by this.
        /// </summary>
        public AmmoEnum AmmoType { get; }

        /// <summary>
        /// The amount of ammo that is being drained by this.
        /// </summary>
        public int Count { get; }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int currentAmmo = inGameState.Resources.GetAmount(AmmoType.GetConsumableResourceEnum());
            int ammoCost = Math.Min(currentAmmo, Count);

            if (inGameState.IsResourceAvailable(AmmoType.GetConsumableResourceEnum(), ammoCost))
            {
                var resultingState = inGameState.Clone();
                resultingState.ApplyConsumeResource(AmmoType.GetConsumableResourceEnum(), ammoCost);
                return new ExecutionResult(resultingState);
            }
            else
            {
                return null;
            }
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
        }

        protected override bool CalculateLogicallyNever()
        {
            // This is always possible, by definition
            return false;
        }

        protected override bool CalculateLogicallyAlways()
        {
            // This is always possible, by definition - though it's not always free
            return true;
        }

        protected override bool CalculateLogicallyFree()
        {
            // While always possible, this can cost resources
            return false;
        }
    }

    public class UnfinalizedAmmoDrain : AbstractUnfinalizedObjectLogicalElement<UnfinalizedAmmoDrain, AmmoDrain>
    {
        public AmmoEnum AmmoType { get; set; }

        public int Count { get; set; }

        protected override AmmoDrain CreateFinalizedElement(UnfinalizedAmmoDrain sourceElement, Action<AmmoDrain> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new AmmoDrain(sourceElement, mappingsInsertionCallback);
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            // No properties need to be handled here
            return Enumerable.Empty<string>();
        }
    }
}
