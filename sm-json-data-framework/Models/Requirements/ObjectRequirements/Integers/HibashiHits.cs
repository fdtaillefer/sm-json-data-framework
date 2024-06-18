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
    /// A logical element which requires Samus to take a number of hits from the Norfair flame jets (known as Hibashi).
    /// </summary>
    public class HibashiHits : AbstractDamageNumericalValueLogicalElement<UnfinalizedHibashiHits, HibashiHits>
    {
        public HibashiHits(UnfinalizedHibashiHits sourceElement, Action<HibashiHits> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        /// <summary>
        /// The number of hibashi hits that Samus must take.
        /// </summary>
        public int Hits => Value;

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateEnvironmentalDamage(inGameState, model.Rules.HibashiDamage) * Hits * times;
        }

        public override int CalculateBestCastDamage(SuperMetroidRules rules)
        {
            return rules.CalculateBestCaseEnvironmentalDamage(rules.HibashiDamage * Hits, AppliedLogicalOptions.RemovedItems);
        }

        public override int CalculateWorstCastDamage(SuperMetroidRules rules)
        {
            return rules.CalculateWorstCaseEnvironmentalDamage(rules.HibashiDamage * Hits, AppliedLogicalOptions.StartConditions.StartingInventory);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetEnvironmentalDamageReducingItems(model, inGameState);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedHibashiHits : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedHibashiHits, HibashiHits>
    {
        public UnfinalizedHibashiHits()
        {

        }

        public UnfinalizedHibashiHits(int hits) : base(hits)
        {

        }

        protected override HibashiHits CreateFinalizedElement(UnfinalizedHibashiHits sourceElement, Action<HibashiHits> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new HibashiHits(sourceElement, mappingsInsertionCallback);
        }
    }
}
