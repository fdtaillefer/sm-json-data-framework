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
        public HeatFrames(UnfinalizedHeatFrames innerElement, Action<HeatFrames> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {

        }
    }

    public class UnfinalizedHeatFrames : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedHeatFrames, HeatFrames>
    {
        private decimal HeatLeniencyMultiplier { get; set; } = LogicalOptions.DefaultFrameLeniencyMultiplier;

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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            HeatLeniencyMultiplier = logicalOptions?.HeatLeniencyMultiplier ?? LogicalOptions.DefaultFrameLeniencyMultiplier;

            return false;
        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int baseDamage = model.Rules.CalculateHeatDamage(inGameState, Value) * times;
            return (int)(baseDamage * HeatLeniencyMultiplier);
        }

        public override IEnumerable<UnfinalizedItem> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetHeatDamageReducingItems(model, inGameState);
        }
    }
}
