using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
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
        private decimal LavaLeniencyMultiplier { get; set; } = LogicalOptions.DefaultFrameLeniencyMultiplier;

        public LavaPhysicsFrames(UnfinalizedLavaPhysicsFrames innerElement, Action<LavaPhysicsFrames> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {

        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int baseDamage = model.Rules.CalculateLavaPhysicsDamage(inGameState, Value) * times;
            return (int)(baseDamage * LavaLeniencyMultiplier);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetLavaPhysicsDamageReducingItems(model, inGameState);
        }

        public override void ApplyLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            LavaLeniencyMultiplier = logicalOptions?.LavaLeniencyMultiplier ?? LogicalOptions.DefaultFrameLeniencyMultiplier;
            base.ApplyLogicalOptions(logicalOptions);
        }
    }

    public class UnfinalizedLavaPhysicsFrames : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedLavaPhysicsFrames, LavaPhysicsFrames>
    {
        /// <summary>
        /// A multiplier to apply to lava frame requirements as a leniency, as per applied logical options.
        /// </summary>
        private decimal LavaLeniencyMultiplier { get; set; } = LogicalOptions.DefaultFrameLeniencyMultiplier;

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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            LavaLeniencyMultiplier = logicalOptions?.LavaLeniencyMultiplier ?? LogicalOptions.DefaultFrameLeniencyMultiplier;
            
            return false;
        }

        public override int CalculateDamage(UnfinalizedSuperMetroidModel model, ReadOnlyUnfinalizedInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int baseDamage = model.Rules.CalculateLavaPhysicsDamage(inGameState, Value) * times;
            return (int)(baseDamage * LavaLeniencyMultiplier);
        }

        public override IEnumerable<UnfinalizedItem> GetDamageReducingItems(UnfinalizedSuperMetroidModel model, ReadOnlyUnfinalizedInGameState inGameState)
        {
            return model.Rules.GetLavaPhysicsDamageReducingItems(model, inGameState);
        }
    }
}
