using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames in lava.
    /// </summary>
    public class LavaFrames : AbstractDamageNumericalValueLogicalElement<UnfinalizedLavaFrames, LavaFrames>
    {
        public LavaFrames(UnfinalizedLavaFrames innerElement, Action<LavaFrames> mappingsInsertionCallback) 
            : base(innerElement, mappingsInsertionCallback)
        {

        }
    }

    public class UnfinalizedLavaFrames : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedLavaFrames, LavaFrames>
    {
        private decimal LavaLeniencyMultiplier { get; set; } = LogicalOptions.DefaultFrameLeniencyMultiplier;

        public UnfinalizedLavaFrames()
        {

        }

        public UnfinalizedLavaFrames(int frames): base (frames)
        {
            
        }

        protected override LavaFrames CreateFinalizedElement(UnfinalizedLavaFrames sourceElement, Action<LavaFrames> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new LavaFrames(sourceElement, mappingsInsertionCallback);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            LavaLeniencyMultiplier = logicalOptions?.LavaLeniencyMultiplier ?? LogicalOptions.DefaultFrameLeniencyMultiplier;

            return false;
        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int baseDamage = model.Rules.CalculateLavaDamage(inGameState, Value) * times;
            return (int)(baseDamage * LavaLeniencyMultiplier);
        }

        public override IEnumerable<UnfinalizedItem> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetLavaDamageReducingItems(model, inGameState);
        }
    }
}
