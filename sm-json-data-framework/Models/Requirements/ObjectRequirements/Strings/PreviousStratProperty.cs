using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings
{
    /// <summary>
    /// A logical element which requires Samus to have reached the current node by a strat with a specific strat property.
    /// </summary>
    public class PreviousStratProperty : AbstractObjectLogicalElementWithStrings<UnfinalizedPreviousStratProperty, PreviousStratProperty>
    {
        private UnfinalizedPreviousStratProperty InnerElement { get; set; }

        public PreviousStratProperty(UnfinalizedPreviousStratProperty innerElement, Action<PreviousStratProperty> mappingsInsertionCallback) : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
        }

        /// <summary>
        /// Returns whether the provided InGameState fulfills this PreviousStratProperty element.
        /// </summary>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public bool IsFulfilled(ReadOnlyInGameState inGameState, int previousRoomCount = 0)
        {
            return InnerElement.IsFulfilled(inGameState, previousRoomCount);
        }
    }

    public class UnfinalizedPreviousStratProperty : AbstractUnfinalizedObjectLogicalElementWithString<UnfinalizedPreviousStratProperty, PreviousStratProperty>
    {
        public UnfinalizedPreviousStratProperty()
        {

        }

        public UnfinalizedPreviousStratProperty(string previousStrat): base(previousStrat)
        {
            
        }

        protected override PreviousStratProperty CreateFinalizedElement(UnfinalizedPreviousStratProperty sourceElement, Action<PreviousStratProperty> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new PreviousStratProperty(sourceElement, mappingsInsertionCallback);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }

        public override bool IsNever()
        {
            return false;
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            // A strat property is a free-form string so we have nothing to initialize
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Returns whether the provided InGameState fulfills this PreviousStratProperty element.
        /// </summary>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by.
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public bool IsFulfilled(ReadOnlyInGameState inGameState, int previousRoomCount = 0)
        {
            return inGameState.GetLastStrat(previousRoomCount)?.StratProperties?.Contains(Value) == true;
        }

        protected override ExecutionResult ExecuteUseful(UnfinalizedSuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if (IsFulfilled(inGameState, previousRoomCount))
            {
                // Clone the InGameState to fulfill method contract
                return new ExecutionResult(inGameState.Clone());
            }
            else
            {
                return null;
            }
        }
    }
}
