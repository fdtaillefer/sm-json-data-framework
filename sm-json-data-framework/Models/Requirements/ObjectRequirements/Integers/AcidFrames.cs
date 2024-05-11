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
        public AcidFrames(UnfinalizedAcidFrames innerElement, Action<AcidFrames> mappingsInsertionCallback) : base(innerElement, mappingsInsertionCallback)
        {

        }
    }

    public class UnfinalizedAcidFrames : AbstractUnfinalizedDamageNumericalValueLogicalElement<UnfinalizedAcidFrames, AcidFrames>
    {
        private decimal AcidLeniencyMultiplier { get; set; } = LogicalOptions.DefaultFrameLeniencyMultiplier;

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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            AcidLeniencyMultiplier = logicalOptions?.AcidLeniencyMultiplier ?? LogicalOptions.DefaultFrameLeniencyMultiplier;

            return false;
        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int baseDamage = model.Rules.CalculateAcidDamage(inGameState, Value) * times;
            return (int)(baseDamage * AcidLeniencyMultiplier);
        }

        public override IEnumerable<UnfinalizedItem> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetAcidDamageReducingItems(model, inGameState);
        }
    }
}
