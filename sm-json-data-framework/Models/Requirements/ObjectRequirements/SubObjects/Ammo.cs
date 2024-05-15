using sm_json_data_framework.Models.InGameStates;
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
        public AmmoEnum AmmoType { get { return InnerElement.AmmoType; } }

        /// <summary>
        /// The amount of ammo that is being consumed by this.
        /// </summary>
        public int Count { get { return InnerElement.Count; } }

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
    }

    public class UnfinalizedAmmo : AbstractUnfinalizedObjectLogicalElement<UnfinalizedAmmo, Ammo>
    {
        [JsonPropertyName("type")]
        public AmmoEnum AmmoType { get; set; }
        
        public int Count { get; set; }

        protected override Ammo CreateFinalizedElement(UnfinalizedAmmo sourceElement, Action<Ammo> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Ammo(sourceElement, mappingsInsertionCallback);
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
            int ammoCost = Count * times;
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
