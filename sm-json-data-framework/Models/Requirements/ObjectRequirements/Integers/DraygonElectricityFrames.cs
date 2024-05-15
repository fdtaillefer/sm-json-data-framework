using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
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
        public DraygonElectricityFrames(UnfinalizedDraygonElectricityFrames innerElement, Action<DraygonElectricityFrames> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {

        }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateElectricityGrappleDamage(inGameState, Value) * times;
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetElectricityGrappleDamageReducingItems(model, inGameState);
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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }

        public override int CalculateDamage(UnfinalizedSuperMetroidModel model, ReadOnlyUnfinalizedInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateElectricityGrappleDamage(inGameState, Value) * times;
        }

        public override IEnumerable<UnfinalizedItem> GetDamageReducingItems(UnfinalizedSuperMetroidModel model, ReadOnlyUnfinalizedInGameState inGameState)
        {
            return model.Rules.GetElectricityGrappleDamageReducingItems(model, inGameState);
        }
    }
}
