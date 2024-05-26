using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
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
        public Ammo(UnfinalizedAmmo sourceElement, Action<Ammo> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {
            AmmoType = sourceElement.AmmoType;
            Count = sourceElement.Count;
        }

        /// <summary>
        /// The type of ammo that is being consumed by this.
        /// </summary>
        public AmmoEnum AmmoType { get; }

        /// <summary>
        /// The amount of ammo that is being consumed by this.
        /// </summary>
        public int Count { get; }

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

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }

        protected override bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            int? maxPossibleAmmo = AppliedLogicalOptions.MaxPossibleAmount(AmmoType.GetConsumableResourceEnum());
            if(maxPossibleAmmo == null)
            {
                return false;
            }
            // This becomes impossible if the required ammo is more than the max ammo we can ever get
            return Count > maxPossibleAmmo.Value;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // A count of 0 makes no sense, but it *would* be logically always
            return Count == 0;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // A count of 0 makes no sense, but it *would* always be free
            return Count == 0;
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

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            // No properties need to be handled here
            return Enumerable.Empty<string>();
        }
    }
}
