using sm_json_data_framework.EnergyManagement;
using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    public abstract class AbstractPunctualEnvironmentDamageLogicalElement<SourceType, ConcreteType> : AbstractDamageNumericalValueLogicalElement<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedPunctualEnvironmentDamageLogicalElement<SourceType, ConcreteType>
        where ConcreteType : AbstractPunctualEnvironmentDamageLogicalElement<SourceType, ConcreteType>
    {
        protected AbstractPunctualEnvironmentDamageLogicalElement(int value) : base(value)
        {

        }

        protected AbstractPunctualEnvironmentDamageLogicalElement(SourceType sourceElement, Action<ConcreteType> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {

        }

        /// <summary>
        /// The number of hits that Samus must take.
        /// </summary>
        public int Hits => Value;

        /// <summary>
        /// The environment damage source that causes damage in this logical element.
        /// </summary>
        protected abstract PunctualEnvironmentDamageEnum EnvironmentDamageEnum { get; }

        public override int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.Rules.CalculatePunctualEnvironmentDamage(inGameState, EnvironmentDamageEnum) * Hits * times;
        }

        public override int CalculateBestCastDamage(SuperMetroidModel model)
        {
            return model.Rules.CalculatePunctualEnvironmentDamage(model.BestCaseInventory, EnvironmentDamageEnum) * Hits;
        }

        public override int CalculateWorstCastDamage(SuperMetroidModel model)
        {
            return model.Rules.CalculatePunctualEnvironmentDamage(model.WorstCaseInventory, EnvironmentDamageEnum) * Hits;
        }

        public override IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            return model.Rules.GetPunctualEnvironmentDamageReducingItems(model, inGameState, EnvironmentDamageEnum);
        }
    }

    public abstract class AbstractUnfinalizedPunctualEnvironmentDamageLogicalElement<ConcreteType, TargetType> : AbstractUnfinalizedDamageNumericalValueLogicalElement<ConcreteType, TargetType>
        where ConcreteType : AbstractUnfinalizedPunctualEnvironmentDamageLogicalElement<ConcreteType, TargetType>
        where TargetType : AbstractPunctualEnvironmentDamageLogicalElement<ConcreteType, TargetType>
    {
        public AbstractUnfinalizedPunctualEnvironmentDamageLogicalElement()
        {

        }

        public AbstractUnfinalizedPunctualEnvironmentDamageLogicalElement(int value) : base(value)
        {

        }
    }
}
