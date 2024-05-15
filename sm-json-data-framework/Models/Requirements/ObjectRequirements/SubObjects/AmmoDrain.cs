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
        private UnfinalizedAmmoDrain InnerElement {get;set;}

        public AmmoDrain(UnfinalizedAmmoDrain innerElement, Action<AmmoDrain> mappingsInsertionCallback) : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
        }

        /// <summary>
        /// The type of ammo that is being drained by this.
        /// </summary>
        public AmmoEnum AmmoType { get { return InnerElement.AmmoType; } }

        /// <summary>
        /// The amount of ammo that is being drained by this.
        /// </summary>
        public int Count { get { return InnerElement.Count; } }

        public override bool IsNever()
        {
            return false;
        }

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
    }

    public class UnfinalizedAmmoDrain : AbstractUnfinalizedObjectLogicalElement<UnfinalizedAmmoDrain, AmmoDrain>
    {
        [JsonPropertyName("type")]
        public AmmoEnum AmmoType { get; set; }

        public int Count { get; set; }

        protected override AmmoDrain CreateFinalizedElement(UnfinalizedAmmoDrain sourceElement, Action<AmmoDrain> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new AmmoDrain(sourceElement, mappingsInsertionCallback);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }

        public override bool IsNever()
        {
            return false;
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            // No properties need to be handled here
            return Enumerable.Empty<string>();
        }

        protected override UnfinalizedExecutionResult ExecuteUseful(UnfinalizedSuperMetroidModel model, ReadOnlyUnfinalizedInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int currentAmmo = inGameState.Resources.GetAmount(AmmoType.GetConsumableResourceEnum());
            int ammoCost = Math.Min(currentAmmo, Count);

            if (inGameState.IsResourceAvailable(AmmoType.GetConsumableResourceEnum(), ammoCost))
            {
                var resultingState = inGameState.Clone();
                resultingState.ApplyConsumeResource(AmmoType.GetConsumableResourceEnum(), ammoCost);
                return new UnfinalizedExecutionResult(resultingState);
            }
            else
            {
                return null;
            }
        }
    }
}
