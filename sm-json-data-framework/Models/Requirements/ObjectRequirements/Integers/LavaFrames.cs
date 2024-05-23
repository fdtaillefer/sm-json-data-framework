using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
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
        /// <summary>
        /// A multiplier to apply to lava frame requirements as a leniency, as per applied logical options.
        /// </summary>
        private decimal LavaLeniencyMultiplier => AppliedLogicalOptions.LavaLeniencyMultiplier;

        public LavaFrames(int numberOfFrames) : base(numberOfFrames)
        {

        }

        public LavaFrames(UnfinalizedLavaFrames sourceElement, Action<LavaFrames> mappingsInsertionCallback) 
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int baseDamage = model.Rules.CalculateLavaDamage(inGameState, Value) * times;
            return (int)(baseDamage * LavaLeniencyMultiplier);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetLavaDamageReducingItems(model, inGameState);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedLavaFrames : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedLavaFrames, LavaFrames>
    {
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
    }
}
