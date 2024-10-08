﻿using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Requirements
{
    /// <summary>
    /// A container class for a series of logical elements.
    /// </summary>
    public class LogicalRequirements : AbstractModelElement<UnfinalizedLogicalRequirements, LogicalRequirements>, IExecutable, ILogicalExecutionPreProcessable
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
            return LogicalElements.ExecuteAll(model, inGameState, times: times, previousRoomCount: previousRoomCount);
        }

        /// <summary>
        /// Attempts to execute one logical element inside this LogicalRequirements (the cheapest one) 
        /// based on the provided in-game state (which will not be altered), by fulfilling its execution requirements.
        /// However, this execution will also succeed if there are no logical elemnts (Hence the "OrAll" part).
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to use for execution. This will NOT be altered by this method.</param>
        /// <param name="times">The number of consecutive times that this should be executed.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns>An ExecutionResult with details about successful execution (and a resulting InGameState that will never be the provided inGameState instance),
        /// or null if execution failed.</returns>
        public ExecutionResult ExecuteOneOrAll(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if(!LogicalElements.Any())
            {
                // Clone the InGameState to fulfill method contract
                return new ExecutionResult(inGameState.Clone());
            }
            (_, ExecutionResult result) = LogicalElements.ExecuteBest(model, inGameState, times: times, previousRoomCount: previousRoomCount);
            return result;
        }

        /// <summary>
        /// Returns the nth (based on index) logical element of type T found within this LogicalRequirements 
        /// (and which also optionally respects the provided predicate), if found. Return null otherwise.
        /// </summary>
        /// <typeparam name="T">The type of logical element to find</typeparam>
        /// <param name="index">The 0-based index of the element to return, among those of type T which respect the predicate (if provided)</param>
        /// <param name="predicate">An optional preidcate which, if provided, will filter the T instances that are found.</param>
        /// <returns></returns>
        public T LogicalElement<T>(int index, Func<T, bool> predicate = null) where T: ILogicalElement
        {
            IEnumerable<T> elements = LogicalElementsTyped<T>();
            if (predicate != null)
            {
                elements = elements.Where(predicate);
            }

            return elements
                .Skip(index)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns all logical elements of type T within this LogicalRequirements which respect the provided predicate.
        /// </summary>
        /// <typeparam name="T">The type of logical elements to find</typeparam>
        /// <param name="predicate">The predicate to filter by</param>
        /// <returns></returns>
        public IEnumerable<T> LogicalElementsWhere<T>(Func<T, bool> predicate) where T : ILogicalElement
        {
            return LogicalElementsTyped<T>()
                .Where(predicate);
        }

        /// <summary>
        /// Returns all logical elements of type T within this LogicalRequirements.
        /// </summary>
        /// <typeparam name="T">The type of logical elements to find</typeparam>
        /// <returns></returns>
        public IEnumerable<T> LogicalElementsTyped<T>() where T : ILogicalElement
        {
            return LogicalElements
                .OfType<T>();
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            foreach (ILogicalElement logicalElement in LogicalElements)
            {
                logicalElement.ApplyLogicalOptions(logicalOptions, model);
            }
        }

        protected override void UpdateLogicalProperties(SuperMetroidModel  model)
        {
            base.UpdateLogicalProperties(model);
            LogicallyNever = CalculateLogicallyNever(model);
            LogicallyOrNever = CalculateLogicallyOrNever(model);
            LogicallyAlways = CalculateLogicallyAlways(model);
            LogicallyOrAlways = CalculateLogicallyOrAlways(model);
            LogicallyFree = CalculateLogicallyFree(model);
            LogicallyOrFree = CalculateLogicallyOrFree(model);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidModel model)
        {
            // Logical requirements are always relevant, even when free or impossible
            return true;
        }

        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidModel model)
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
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyOrNever(SuperMetroidModel model)
        {
            // An empty Or makes little sense - interpret it as being possible to fulfill
            if(!LogicalElements.Any())
            {
                return false;
            }

            // If we have any child logical elements, an Or is impossible if all children are impossible
            return LogicalElements.All(element => element.LogicallyNever);
        }

        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways(SuperMetroidModel model)
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
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyOrAlways(SuperMetroidModel model)
        {
            // An empty Or makes little sense - interpret it as always being possible to fulfill
            if (!LogicalElements.Any())
            {
                return true;
            }

            // If we have any child logical elements, an Or is "always" if at least one child is
            return LogicalElements.Any(element => element.LogicallyAlways);
        }

        public bool LogicallyFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyFree"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyFree(SuperMetroidModel model)
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
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyOrFree(SuperMetroidModel model)
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
        /// Returns the nth (based on index) logical element of type T found within this UnfinalizedLogicalRequirements 
        /// (and which also optionally respects the provided predicate), if found. Return null otherwise.
        /// </summary>
        /// <typeparam name="T">The type of logical element to find</typeparam>
        /// <param name="index">The 0-based index of the element to return, among those of type T which respect the predicate (if provided)</param>
        /// <param name="predicate">An optional preidcate which, if provided, will filter the T instances that are found.</param>
        /// <returns></returns>
        public T LogicalElement<T>(int index, Func<T, bool> predicate = null) where T : IUnfinalizedLogicalElement
        {
            IEnumerable<T> elements = LogicalElementsTyped<T>();
            if (predicate != null)
            {
                elements = elements.Where(predicate);
            }

            return elements
                .Skip(index)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns all logical elements of type T within this UnfinalizedLogicalRequirements which respect the provided predicate.
        /// </summary>
        /// <typeparam name="T">The type of logical elements to find</typeparam>
        /// <param name="predicate">The predicate to filter by</param>
        /// <returns></returns>
        public IEnumerable<T> LogicalElementsWhere<T>(Func<T, bool> predicate) where T : IUnfinalizedLogicalElement
        {
            return LogicalElementsTyped<T>()
                .Where(predicate);
        }

        /// <summary>
        /// Returns all logical elements of type T within this UnfinalizedLogicalRequirements.
        /// </summary>
        /// <typeparam name="T">The type of logical elements to find</typeparam>
        /// <returns></returns>
        public IEnumerable<T> LogicalElementsTyped<T>() where T : IUnfinalizedLogicalElement
        {
            return LogicalElements
                .OfType<T>();
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
