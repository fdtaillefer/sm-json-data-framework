using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to take a number of hits from the weaker spikes (mostly found in Brinstar).
    /// </summary>
    public class ThornHits : AbstractDamageNumericalValueLogicalElement<UnfinalizedThornHits, ThornHits>
    {
        public ThornHits(UnfinalizedThornHits innerElement, Action<ThornHits> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {

        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateEnvironmentalDamage(inGameState, model.Rules.ThornDamage) * Value * times;
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetEnvironmentalDamageReducingItems(model, inGameState);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedThornHits : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedThornHits, ThornHits>
    {
        public UnfinalizedThornHits()
        {

        }

        public UnfinalizedThornHits(int hits) : base(hits)
        {

        }

        protected override ThornHits CreateFinalizedElement(UnfinalizedThornHits sourceElement, Action<ThornHits> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new ThornHits(sourceElement, mappingsInsertionCallback);
        }
    }
}
