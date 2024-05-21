﻿using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers
{
    /// <summary>
    /// An abstract superclass for the many <see cref="AbstractObjectLogicalElementWithNumericalIntegerValue{SourceType, ConcreteType}"/> that happen to inflict damage on Samus.
    /// </summary>
    public abstract class AbstractDamageNumericalValueLogicalElement<SourceType, ConcreteType>: AbstractObjectLogicalElementWithNumericalIntegerValue<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedDamageNumericalValueLogicalElement<SourceType, ConcreteType>
        where ConcreteType : AbstractDamageNumericalValueLogicalElement<SourceType, ConcreteType>
    {
        protected AbstractDamageNumericalValueLogicalElement (int value): base(value)
        {

        }

        protected AbstractDamageNumericalValueLogicalElement(SourceType innerElement, Action<ConcreteType> mappingsInsertionCallback)
            : base(innerElement, mappingsInsertionCallback)
        {
            
        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int damage = CalculateDamage(model, inGameState, times: times, previousRoomCount: previousRoomCount);
            if (inGameState.IsResourceAvailable(ConsumableResourceEnum.Energy, damage))
            {
                var resultingState = inGameState.Clone();
                resultingState.ApplyConsumeResource(ConsumableResourceEnum.Energy, damage);
                ExecutionResult result = new ExecutionResult(resultingState);
                result.AddDamageReducingItemsInvolved(GetDamageReducingItems(model, inGameState));
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Calculates the amount of damage that fulfilling this logical element will inflict on Samus.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="times">The number of consecutive times that Samus will take this damage.</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns>The calculated amount of damage</returns>
        public abstract int CalculateDamage(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0);

        /// <summary>
        /// Returns the enumeration of items that are responsible for reducing incurred damage, 
        /// given the execution described by the provided parameters.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state that execution would start with.</param>
        /// <returns></returns>
        public abstract IEnumerable<Item> GetDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState);

        protected override bool CalculateLogicallyNever()
        {
            // A damage element could become impossible if the minimum damage it can inflict is more than the max energy we can ever get,
            // but max energy is not available in logical options.
            return false;
        }

        protected override bool CalculateLogicallyAlways()
        {
            // This could be always if it ends up being 0 damage suitless, but that would be defined by the rules, which aren't available here
            return false;
        }

        protected override bool CalculateLogicallyFree()
        {
            // This could be free if it ends up being 0 damage suitless, but that would be defined by the rules, which aren't available here
            return false;
        }
    }

    public abstract class AbstractUnfinalizedDamageNumericalValueLogicalElement<ConcreteType, TargetType>: AbstractUnfinalizedObjectLogicalElementWithNumericalIntegerValue<ConcreteType, TargetType>
        where ConcreteType : AbstractUnfinalizedDamageNumericalValueLogicalElement<ConcreteType, TargetType>
        where TargetType : AbstractDamageNumericalValueLogicalElement<ConcreteType, TargetType>
    {
        public AbstractUnfinalizedDamageNumericalValueLogicalElement()
        {

        }

        public AbstractUnfinalizedDamageNumericalValueLogicalElement(int value) : base(value)
        {

        }
    }
}
