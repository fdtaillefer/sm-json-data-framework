﻿using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames grappled to one of Draygon's turrets.
    /// </summary>
    public class DraygonElectricityFrames : AbstractDamageNumericalValueLogicalElement<UnfinalizedDraygonElectricityFrames, DraygonElectricityFrames>
    {
        public DraygonElectricityFrames(UnfinalizedDraygonElectricityFrames sourceElement, Action<DraygonElectricityFrames> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        /// <summary>
        /// The number of frames that Samus must spend grappled to a Draygon turret.
        /// </summary>
        public int Frames => Value;

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateElectricityGrappleDamage(inGameState, Frames) * times;
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetElectricityGrappleDamageReducingItems(model, inGameState);
        }

        public override int CalculateBestCastDamage(SuperMetroidRules rules)
        {
            return rules.CalculateBestCaseElectricityGrappleDamage(Frames, AppliedLogicalOptions.RemovedItems);
        }

        public override int CalculateWorstCastDamage(SuperMetroidRules rules)
        {
            return rules.CalculateWorstCaseElectricityGrappleDamage(Frames, AppliedLogicalOptions.StartConditions.StartingInventory);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedDraygonElectricityFrames : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedDraygonElectricityFrames, DraygonElectricityFrames>
    {
        public UnfinalizedDraygonElectricityFrames()
        {

        }

        public UnfinalizedDraygonElectricityFrames(int frames) : base(frames)
        {

        }

        protected override DraygonElectricityFrames CreateFinalizedElement(UnfinalizedDraygonElectricityFrames sourceElement, Action<DraygonElectricityFrames> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new DraygonElectricityFrames(sourceElement, mappingsInsertionCallback);
        }
    }
}
