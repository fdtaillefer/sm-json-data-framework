﻿using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
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
        public HibashiHits(UnfinalizedHibashiHits innerElement, Action<HibashiHits> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {

        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateEnvironmentalDamage(inGameState, model.Rules.HibashiDamage) * Value * times;
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetEnvironmentalDamageReducingItems(model, inGameState);
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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }
        public override int CalculateDamage(UnfinalizedSuperMetroidModel model, ReadOnlyUnfinalizedInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateEnvironmentalDamage(inGameState, model.Rules.HibashiDamage) * Value * times;
        }

        public override IEnumerable<UnfinalizedItem> GetDamageReducingItems(UnfinalizedSuperMetroidModel model, ReadOnlyUnfinalizedInGameState inGameState)
        {
            return model.Rules.GetEnvironmentalDamageReducingItems(model, inGameState);
        }
    }
}
