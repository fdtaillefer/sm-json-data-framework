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
    public class LogicalRequirements: IExecutable
    {
        internal class NeverRequirements
        {
            public static readonly LogicalRequirements Instance = new LogicalRequirements(new AbstractLogicalElement[] { new NeverLogicalElement() });
        }

        internal class AlwaysRequirements
        {
            public static readonly LogicalRequirements Instance = new LogicalRequirements();
        }

        public LogicalRequirements()
        {

        }

        public LogicalRequirements(IEnumerable<AbstractLogicalElement> logicalElements)
        {
            LogicalElements = LogicalElements.Concat(logicalElements).ToList();
        }

        public IList<AbstractLogicalElement> LogicalElements { get; private set; } = new List<AbstractLogicalElement>();

        /// <summary>
        /// Returns whether this set of logical requirements in its current state is logically impossible to fully complete
        /// (due to having a mandatory <see cref="NeverLogicalElement"/>).
        /// This does not tell whether the logical element should be replaced by a never, because that depends on map layout and logical options, 
        /// which are not available here.
        /// </summary>
        /// <returns></returns>
        public bool IsNever()
        {
            return LogicalElements.Where(element => element.IsNever()).Any();
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

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            return model.ExecuteAll(LogicalElements, inGameState, times: times, previousRoomCount: previousRoomCount);
        }

        /// <summary>
        /// Attempts to execute one logical element inside this LogicalRequirements (the cheapest one) 
        /// based on the provided in-game state (which will not be altered), by fulfilling its execution requirements.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to use for execution. This will NOT be altered by this method.</param>
        /// <param name="times">The number of consecutive times that this should be executed.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns></returns>
        public ExecutionResult ExecuteOne(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            (_, ExecutionResult result) = model.ExecuteBest(LogicalElements, inGameState, times: times, previousRoomCount: previousRoomCount);
            return result;
        }

        /// <summary>
        /// Returns an instance of LogicalRequirements whose execution never succeeds.
        /// </summary>
        /// <returns></returns>
        public static LogicalRequirements Never()
        {
            return NeverRequirements.Instance;
        }
    }
}
