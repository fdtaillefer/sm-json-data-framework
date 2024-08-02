using sm_json_data_framework.EnergyManagement;
using sm_json_data_framework.InGameStates;
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
    public class LavaPhysicsFrames : AbstractDamageOverTimeLogicalElement<UnfinalizedLavaPhysicsFrames, LavaPhysicsFrames>
    {
        /// <summary>
        /// A multiplier to apply to lava frame requirements as a leniency, as per applied logical options.
        /// </summary>
        public decimal LavaLeniencyMultiplier => AppliedLogicalOptions.LavaLeniencyMultiplier;

        protected override DamageOverTimeEnum DotEnum => DamageOverTimeEnum.LavaPhysics;

        protected override decimal LeniencyMultiplier => LavaLeniencyMultiplier;

        public LavaPhysicsFrames(UnfinalizedLavaPhysicsFrames sourceElement, Action<LavaPhysicsFrames> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedLavaPhysicsFrames : AbstractUnfinalizedDamageOverTimeLogicalElement<UnfinalizedLavaPhysicsFrames, LavaPhysicsFrames>
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
