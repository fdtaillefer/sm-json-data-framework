using sm_json_data_framework.Converters;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements
{
    /// <summary>
    /// A container class for a series of logical elements.
    /// </summary>
    public class LogicalRequirements
    {
        public LogicalRequirements()
        {

        }

        public LogicalRequirements(IEnumerable<AbstractLogicalElement> logicalElements)
        {
            LogicalElements = LogicalElements.Concat(logicalElements);
        }

        public IEnumerable<AbstractLogicalElement> LogicalElements { get; set; } = Enumerable.Empty<AbstractLogicalElement>();

        /// <summary>
        /// Goes through all logical elements within this LogicalRequirements (and all LogicalRequirements within any of them),
        /// attempting to replace any RawStringLogicalElement by a more appropriate logical element, using the provided StringLogicalElementConverter.
        /// </summary>
        /// <param name="stringElementConverter">A StringLogicalElementConverter that can attempt to convert a raw string into a more specific logical element</param>
        /// <returns>All strings that couldn't be resolved to a more specific logical element</returns>
        public IEnumerable<string> ReplaceRawStringElements(StringLogicalElementConverter stringElementConverter)
        {
            List<string> unresolvedRawStrings = new List<string>();

            // For any logical element in this LogicalRequirements that has sub-LogicalRequirements, do a recursive call
            foreach(AbstractObjectLogicalElementWithSubRequirements logicalElement in LogicalElements.OfType<AbstractObjectLogicalElementWithSubRequirements>())
            {
                IEnumerable<string> unresolvedSubRawStrings = logicalElement.LogicalRequirements.ReplaceRawStringElements(stringElementConverter);
                unresolvedRawStrings.AddRange(unresolvedSubRawStrings);
            }

            // Build a new list of resolved logical elements to replace the current one
            List<AbstractLogicalElement> newElements = new List<AbstractLogicalElement>();

            // For any raw string logical element we contain directly, attempt to resolve it and substitute the new logical element
            // Anything else just stays the same
            foreach(AbstractLogicalElement logicalElement in LogicalElements)
            {
                // If this is a raw string, get the converter to convert it to hopefully something better
                if (logicalElement is RawStringLogicalElement rawStringElement)
                {
                    AbstractLogicalElement newLogicalElement = stringElementConverter.CreateLogicalElement(rawStringElement.StringValue);
                    // if we still have a raw string, add the string to the list of unresolved strings we'll return
                    if (newLogicalElement is RawStringLogicalElement)
                    {
                        unresolvedRawStrings.Add(rawStringElement.StringValue);
                        newElements.Add(logicalElement);
                    }
                    // If we successfully resolved the string, substitute the new element
                    else
                    {
                        newElements.Add(newLogicalElement);
                    }
                }
                else
                {
                    newElements.Add(logicalElement);
                }
            }
            LogicalElements = newElements;

            return unresolvedRawStrings.Distinct();
        }

        /// <summary>
        /// Goes through all logical elements within this LogicalRequirements (and all LogicalRequirements within any of them),
        /// attempting to initialize any property that is an object referenced by another property(which is its identifier).
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this LogicalRequirements is, or null if it's not in a room</param>
        /// <returns>A sequence of strings describing references that could not be initialized properly.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            foreach(AbstractLogicalElement logicalElement in LogicalElements)
            {
                unhandled.AddRange(logicalElement.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled;
        }

        /// <summary>
        /// Attempts to fulfill the requirements of this LogicalRequirements by the provided in-game state. If successful, returns a new InGameState instance to
        /// represent the in-game state after performing the LogicalRequirements. If unsuccessful, return nulls.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to evaluate</param>
        /// <param name="and">If true, attempts to fulfill all logical elements in this requirements. 
        /// If false, attempst to fulfill at least one logical element.</param>
        /// <param name="times">The number of consecutive times that this should be fulfilled. Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="usePreviousRoom">If true, uses the last known room state at the previous room instead of the current room to answer
        /// (whenever in-room state is relevant).</param>
        /// <returns>A new InGameState representing the state after fulfillment if successful, or null otherwise</returns>
        public InGameState AttemptFulfill(SuperMetroidModel model, InGameState inGameState, int times = 1, bool and = true, bool usePreviousRoom = false)
        {
            if (and)
            {
                return model.ApplyAnd(inGameState, LogicalElements, (le, igs) => le.AttemptFulfill(model, igs, times: times, usePreviousRoom: usePreviousRoom));
            }
            else
            {
                return model.ApplyOr(inGameState, LogicalElements, (le, igs) => le.AttemptFulfill(model, igs, times: times, usePreviousRoom: usePreviousRoom));
            }
        }
    }
}
