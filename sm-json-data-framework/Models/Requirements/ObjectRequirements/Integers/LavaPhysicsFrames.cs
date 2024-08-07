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
    /// A logical element which requires Samus to spend some frames in lava while under lava physics (i.e. with Gravity Suit turned off even if available).
    /// </summary>
    public class LavaPhysicsFrames : AbstractDamageNumericalValueLogicalElement<UnfinalizedLavaPhysicsFrames, LavaPhysicsFrames>
    {
        /// <summary>
        /// A multiplier to apply to lava frame requirements as a leniency, as per applied logical options.
        /// </summary>
        public decimal LavaLeniencyMultiplier => AppliedLogicalOptions.LavaLeniencyMultiplier;

        public LavaPhysicsFrames(UnfinalizedLavaPhysicsFrames sourceElement, Action<LavaPhysicsFrames> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        /// <summary>
        /// The number of frames that Samus must spend while under lava physics.
        /// </summary>
        public int Frames => Value;

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateLavaPhysicsDamage(inGameState, (int)(Frames * LavaLeniencyMultiplier)) * times;
        }

        public override int CalculateBestCastDamage(SuperMetroidRules rules)
        {
            return (int)(rules.CalculateBestCaseLavaPhysicsDamage(Frames, AppliedLogicalOptions.RemovedItems) * LavaLeniencyMultiplier);
        }

        public override int CalculateWorstCastDamage(SuperMetroidRules rules)
        {
            return (int)(rules.CalculateWorstCaseLavaPhysicsDamage(Frames, AppliedLogicalOptions.StartConditions.StartingInventory) * LavaLeniencyMultiplier);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetLavaPhysicsDamageReducingItems(model, inGameState);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedLavaPhysicsFrames : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedLavaPhysicsFrames, LavaPhysicsFrames>
    {

        public UnfinalizedLavaPhysicsFrames()
        {

        }

        public UnfinalizedLavaPhysicsFrames(int frames) : base(frames)
        {

        }

        protected override LavaPhysicsFrames CreateFinalizedElement(UnfinalizedLavaPhysicsFrames sourceElement, Action<LavaPhysicsFrames> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new LavaPhysicsFrames(sourceElement, mappingsInsertionCallback);
        }
    }
}
