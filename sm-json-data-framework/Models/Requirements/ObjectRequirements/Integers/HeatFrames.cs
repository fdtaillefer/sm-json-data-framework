using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames in a heated room.
    /// </summary>
    public class HeatFrames : AbstractDamageNumericalValueLogicalElement<UnfinalizedHeatFrames, HeatFrames>
    {
        /// <summary>
        /// A multiplier to apply to heat frame requirements as a leniency, as per applied logical options.
        /// </summary>
        private decimal HeatLeniencyMultiplier => AppliedLogicalOptions.HeatLeniencyMultiplier;

        public HeatFrames(int numberOfFrames) : base(numberOfFrames)
        {

        }

        public HeatFrames(UnfinalizedHeatFrames sourceElement, Action<HeatFrames> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int baseDamage = model.Rules.CalculateHeatDamage(inGameState, Value) * times;
            return (int)(baseDamage * HeatLeniencyMultiplier);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetHeatDamageReducingItems(model, inGameState);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedHeatFrames : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedHeatFrames, HeatFrames>
    {
        public UnfinalizedHeatFrames()
        {

        }

        public UnfinalizedHeatFrames(int frames): base(frames)
        {
            
        }

        protected override HeatFrames CreateFinalizedElement(UnfinalizedHeatFrames sourceElement, Action<HeatFrames> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new HeatFrames(sourceElement, mappingsInsertionCallback);
        }
    }
}
