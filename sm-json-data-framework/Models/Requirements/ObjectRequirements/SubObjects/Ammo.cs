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
    /// A logical element which requires Samus spend a fixed amount of a specific ammo type.
    /// </summary>
    public class Ammo : AbstractObjectLogicalElement<UnfinalizedAmmo, Ammo>
    {
        private UnfinalizedAmmo InnerElement { get; set; }

        public Ammo(UnfinalizedAmmo innerElement, Action<Ammo> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
        }

        /// <summary>
        /// The type of ammo that is being consumed by this.
        /// </summary>
        public AmmoEnum AmmoType => InnerElement.AmmoType;

        /// <summary>
        /// The amount of ammo that is being consumed by this.
        /// </summary>
        public int Count => InnerElement.Count;

        public override bool IsNever()
        {
            return false;
        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int ammoCost = Count * times;
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

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }

        protected override bool CalculateLogicallyNever()
        {
            // This could become impossible if the required ammo is more than the max ammo we can ever get,
            // but max ammo is not available in logical options.
            return false;
        }
    }

    public class UnfinalizedAmmo : AbstractUnfinalizedObjectLogicalElement<UnfinalizedAmmo, Ammo>
    {
        public AmmoEnum AmmoType { get; set; }
        
        public int Count { get; set; }

        protected override Ammo CreateFinalizedElement(UnfinalizedAmmo sourceElement, Action<Ammo> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Ammo(sourceElement, mappingsInsertionCallback);
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
    }
}
