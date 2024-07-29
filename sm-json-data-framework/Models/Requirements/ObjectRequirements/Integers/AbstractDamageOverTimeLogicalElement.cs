using sm_json_data_framework.InGameStates;
using sm_json_data_framework.InGameStates.EnergyManagement;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// An abstract class for logical elements that have to do with spending a number of frames subjected to a damage-over-time effect.
    /// </summary>
    public abstract class AbstractDamageOverTimeLogicalElement<SourceType, ConcreteType> : AbstractDamageNumericalValueLogicalElement<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedDamageOverTimeLogicalElement<SourceType, ConcreteType>
        where ConcreteType : AbstractDamageOverTimeLogicalElement<SourceType, ConcreteType>
    {
        protected AbstractDamageOverTimeLogicalElement(int value) : base(value)
        {
            
        }

        protected AbstractDamageOverTimeLogicalElement(SourceType sourceElement, Action<ConcreteType> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {
            
        }

        /// <summary>
        /// The number of frames that Samus must be subjected to the DoT effect.
        /// </summary>
        public int Frames => Value;

        /// <summary>
        /// The damage over time effect that is relevant for this logical element.
        /// </summary>
        protected abstract DamageOverTimeEnum DotEnum { get; }

        /// <summary>
        /// The leniency multiplier to apply to the number of frames, based on the applied logical options.
        /// </summary>
        protected abstract decimal LeniencyMultiplier { get; }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculateDamageOverTime(inGameState, (int)(Frames * LeniencyMultiplier), DotEnum) * times;
        }

        public override int CalculateBestCastDamage(SuperMetroidRules rules)
        {
            return (int)(rules.CalculateBestCaseDamageOverTime(Frames, DotEnum, AppliedLogicalOptions.RemovedItems) * LeniencyMultiplier);
        }

        public override int CalculateWorstCastDamage(SuperMetroidRules rules)
        {
            return (int)(rules.CalculateWorstCaseDamageOverTime(Frames, DotEnum, AppliedLogicalOptions.StartConditions.StartingInventory) * LeniencyMultiplier);
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetDamageOverTimeReducingItems(model, inGameState, DotEnum);
        }
    }

    public abstract class AbstractUnfinalizedDamageOverTimeLogicalElement<ConcreteType, TargetType> : AbstractUnfinalizedDamageNumericalValueLogicalElement<ConcreteType, TargetType>
        where ConcreteType : AbstractUnfinalizedDamageOverTimeLogicalElement<ConcreteType, TargetType>
        where TargetType : AbstractDamageOverTimeLogicalElement<ConcreteType, TargetType>
    {
        public AbstractUnfinalizedDamageOverTimeLogicalElement()
        {

        }

        public AbstractUnfinalizedDamageOverTimeLogicalElement(int value) : base(value)
        {

        }
    }
}
