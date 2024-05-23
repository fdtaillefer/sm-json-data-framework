using sm_json_data_framework.Converters;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
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

        public LogicalRequirements(UnfinalizedLogicalRequirements sourceElement, Action<LogicalRequirements> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            LogicalElements = sourceElement.LogicalElements.Select(unfinalized => unfinalized.FinalizeUntypedLogicalElement(mappings)).ToList().AsReadOnly();
        }

        public IReadOnlyList<ILogicalElement> LogicalElements { get; }

        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            // If logical options make these logical requirements impossible, don't bother trying
            if (LogicallyNever)
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

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            foreach (ILogicalElement logicalElement in LogicalElements)
            {
                logicalElement.ApplyLogicalOptions(logicalOptions, rules);
            }
        }

        protected override void UpdateLogicalProperties(SuperMetroidRules rules)
        {
            base.UpdateLogicalProperties(rules);
            LogicallyNever = CalculateLogicallyNever(rules);
            LogicallyOrNever = CalculateLogicallyOrNever(rules);
            LogicallyAlways = CalculateLogicallyAlways(rules);
            LogicallyOrAlways = CalculateLogicallyOrAlways(rules);
            LogicallyFree = CalculateLogicallyFree(rules);
            LogicallyOrFree = CalculateLogicallyOrFree(rules);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // Logical requirements are always relevant, even when free or impossible
            return true;
        }

        /// <summary>
        /// If true, then it is known that this logical element given the current logical options can never ever be executed.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // Since executing logical requirements means executing all logical elements, this becomes impossible if any child is impossible
            return LogicalElements.Any(element =>  element.LogicallyNever);
        }

        /// <summary>
        /// If true, then it is known that this logical element given the current logical options can never ever be executed, 
        /// regardless of in-game state, even if it were interpreted as a logical Or.
        /// </summary>
        public bool LogicallyOrNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyOrNever"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyOrNever(SuperMetroidRules rules)
        {
            // An empty Or makes little sense - interpret it as being possible to fulfill
            if(!LogicalElements.Any())
            {
                return false;
            }

            // If we have any child logical elements, an Or is impossible if all children are impossible
            return LogicalElements.All(element => element.LogicallyNever);
        }

        /// <summary>
        /// If true, this this can always be fulfilled, regardless of in-game state, given the current logical options.
        /// </summary>
        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // Since executing logical requirements means executing all logical elements,
            // this only becomes "always" if all child elements also are (but also if empty)
            return LogicalElements.All(element => element.LogicallyAlways);
        }

        /// <summary>
        /// If true, then it is known that this logical element given the current logical options could always be executed, 
        /// regardless of in-game state, if it were interpreted as a logical Or.
        /// </summary>
        public bool LogicallyOrAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyOrAlways"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyOrAlways(SuperMetroidRules rules)
        {
            // An empty Or makes little sense - interpret it as always being possible to fulfill
            if (!LogicalElements.Any())
            {
                return true;
            }

            // If we have any child logical elements, an Or is "always" if at least one child is
            return LogicalElements.Any(element => element.LogicallyAlways);
        }

        /// <summary>
        /// If true, not only can this these requirements always be executed given the current logical options, regardless of in-game state,
        /// but that fulfillment is also guaranteed to cost no resources.
        /// </summary>
        public bool LogicallyFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyFree"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // Since executing logical requirements means executing all logical elements,
            // this only becomes free if all child elements also are (but also if empty)
            return LogicalElements.All(element => element.LogicallyFree);
        }

        /// <summary>
        /// If true, then these requirements can always be executed for free given the current logical options, regardless of in-game state,
        /// when interpreted as a logical Or. A free execution means not only always possible, but also with on resource cost.
        /// </summary>
        public bool LogicallyOrFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyOrFree"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyOrFree(SuperMetroidRules rules)
        {
            // An empty Or makes little sense - interpret it as always being free
            if (!LogicalElements.Any())
            {
                return true;
            }

            // If we have any child logical elements, an Or is free if at least one child is
            return LogicalElements.Any(element => element.LogicallyFree);
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
