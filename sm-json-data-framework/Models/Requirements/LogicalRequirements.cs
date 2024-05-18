using sm_json_data_framework.Converters;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements
{
    /// <summary>
    /// A container class for a series of logical elements.
    /// </summary>
    public class LogicalRequirements : AbstractModelElement<UnfinalizedLogicalRequirements, LogicalRequirements>, IExecutable
    {
        internal class NeverRequirements
        {
            public static readonly LogicalRequirements Instance = new LogicalRequirements(new List<ILogicalElement> { new NeverLogicalElement() }.AsReadOnly());
        }

        internal class AlwaysRequirements
        {
            public static readonly LogicalRequirements Instance = new LogicalRequirements(new List<ILogicalElement>().AsReadOnly());
        }

        private LogicalRequirements(IReadOnlyList<ILogicalElement> logicalElements)
        {
            LogicalElements = logicalElements;
        }

        public LogicalRequirements(UnfinalizedLogicalRequirements innerElement, Action<LogicalRequirements> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            LogicalElements = innerElement.LogicalElements.Select(unfinalized => unfinalized.FinalizeUntypedLogicalElement(mappings)).ToList().AsReadOnly();
        }

        public IReadOnlyList<ILogicalElement> LogicalElements { get; }

        /// <summary>
        /// Returns whether this set of logical requirements in its base state is logically impossible to fully complete
        /// (due to having a mandatory <see cref="NeverLogicalElement"/>).
        /// This does not tell whether the logical element should be replaced by a never, because that depends on map layout and logical options, 
        /// which are not available here.
        /// </summary>
        /// <returns></returns>
        public bool IsNever()
        {
            return LogicalElements.Where(element => element.IsNever()).Any();
        }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // If logical options make these logical requirements impossible, don't bother trying
            if (UselessByLogicalOptions)
            {
                return null;
            }
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

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            bool anyUselessLogicalElement = false;
            foreach (ILogicalElement logicalElement in LogicalElements)
            {
                logicalElement.ApplyLogicalOptions(logicalOptions);
                if (logicalElement.UselessByLogicalOptions)
                {
                    anyUselessLogicalElement = true;
                }
            }

            if (logicalOptions == null)
            {
                return false;
            }
            else
            {
                // We're implicitly an And, so we become impossible/useless as soon as any sub element is
                return anyUselessLogicalElement;
            }
        }
    }

    public class UnfinalizedLogicalRequirements : AbstractUnfinalizedModelElement<UnfinalizedLogicalRequirements, LogicalRequirements>
    {
        internal class UnfinalizedNeverRequirements
        {
            public static readonly UnfinalizedLogicalRequirements Instance = new UnfinalizedLogicalRequirements(new IUnfinalizedLogicalElement[] { new UnfinalizedNeverLogicalElement() });
        }

        internal class UnfinalizedAlwaysRequirements
        {
            public static readonly UnfinalizedLogicalRequirements Instance = new UnfinalizedLogicalRequirements();
        }

        public UnfinalizedLogicalRequirements()
        {

        }

        public UnfinalizedLogicalRequirements(IEnumerable<IUnfinalizedLogicalElement> logicalElements)
        {
            LogicalElements = LogicalElements.Concat(logicalElements).ToList();
        }

        protected override LogicalRequirements CreateFinalizedElement(UnfinalizedLogicalRequirements sourceElement, Action<LogicalRequirements> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new LogicalRequirements(sourceElement, mappingsInsertionCallback, mappings);
        }

        public IList<IUnfinalizedLogicalElement> LogicalElements { get; private set; } = new List<IUnfinalizedLogicalElement>();

        /// <summary>
        /// Returns whether this set of logical requirements in its base state is logically impossible to fully complete
        /// (due to having a mandatory <see cref="UnfinalizedNeverLogicalElement"/>).
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
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            foreach(IUnfinalizedLogicalElement logicalElement in LogicalElements)
            {
                unhandled.AddRange(logicalElement.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled;
        }

        /// <summary>
        /// Returns an instance of LogicalRequirements whose execution never succeeds.
        /// </summary>
        /// <returns></returns>
        public static UnfinalizedLogicalRequirements Never()
        {
            return UnfinalizedNeverRequirements.Instance;
        }
    }
}
