using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// A logical element which requires Samus to spend some frames in acid.
    /// </summary>
    public class AcidFrames : AbstractDamageNumericalValueLogicalElement<UnfinalizedAcidFrames, AcidFrames>
    {
        /// <summary>
        /// A multiplier to apply to acid frame requirements as a leniency, as per applied logical options.
        /// </summary>
        private decimal AcidLeniencyMultiplier => AppliedLogicalOptions.AcidLeniencyMultiplier;

        public AcidFrames(int numberOfFrames): base(numberOfFrames)
        {

        }

        public AcidFrames(UnfinalizedAcidFrames sourceElement, Action<AcidFrames> mappingsInsertionCallback) : base(sourceElement, mappingsInsertionCallback)
        {

        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int baseDamage = model.Rules.CalculateAcidDamage(inGameState, Value) * times;
            return (int)(baseDamage * AcidLeniencyMultiplier);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetAcidDamageReducingItems(model, inGameState);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
        }
    }

    public class UnfinalizedAcidFrames : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedAcidFrames, AcidFrames>
    {
        public UnfinalizedAcidFrames()
        {

        }

        public UnfinalizedAcidFrames(int frames) : base(frames)
        {
            
        }

        protected override AcidFrames CreateFinalizedElement(UnfinalizedAcidFrames sourceElement, Action<AcidFrames> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new AcidFrames(sourceElement, mappingsInsertionCallback);
        }
    }
}
