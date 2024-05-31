using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Security.AccessControl;
using System.Text;

namespace sm_json_data_framework.Models.Requirements
{
    /// <summary>
    /// The abstract base class for all logical elements. 
    /// A logical element is a building block of the logica that decides whether Samus is able to do somethign in the game.
    /// </summary>
    /// <typeparam name="SourceType">The unfinalized type that finalizes into this type</typeparam>
    /// <typeparam name="ConcreteType">The self-type of the concrete sub-type</typeparam>
    public abstract class AbstractLogicalElement<SourceType, ConcreteType> : AbstractModelElement<SourceType, ConcreteType>, ILogicalElement
        where ConcreteType: AbstractLogicalElement<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedLogicalElement<SourceType, ConcreteType>
    {
        public AbstractLogicalElement()
        {

        }

        public AbstractLogicalElement(SourceType sourceElement, Action<ConcreteType> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {
            
        }

        // Inherited from IExecutable
        public ExecutionResult Execute(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            if (LogicallyNever)
            {
                return null;
            }
            return ExecuteUseful(model, inGameState, times, previousRoomCount);
        }

        /// <summary>
        /// Has the same purpose as <see cref="Execute(SuperMetroidModel, ReadOnlyInGameState, int, int)"/>, but will only be called if this logical element
        /// has not be rendered impossible by logical options, meaning implementations don't need to test for it.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="inGameState">The in-game state to use for execution. This will NOT be altered by this method.</param>
        /// <param name="times">The number of consecutive times that this should be executed.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <returns>An ExecutionResult describing the execution if successful, or null otherwise.</returns>
        protected abstract ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0);

        protected override void UpdateLogicalProperties(SuperMetroidRules rules)
        {
            base.UpdateLogicalProperties(rules);
            LogicallyNever = CalculateLogicallyNever(rules);
            LogicallyAlways = CalculateLogicallyAlways(rules);
            LogicallyFree = CalculateLogicallyFree(rules);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // There's always situations where a logical element (even one that's logically free or logically never) has some relevance,
            // such as adding data to an ExecutionResult or affecting a containing And/Or.
            return true;
        }

        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected abstract bool CalculateLogicallyNever(SuperMetroidRules rules);

        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected abstract bool CalculateLogicallyAlways(SuperMetroidRules rules);

        public bool LogicallyFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyFree"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected abstract bool CalculateLogicallyFree(SuperMetroidRules rules);
    }

    /// <summary>
    /// The untyped interface portion of <see cref="AbstractLogicalElement{SourceType, ConcreteType}"/>.
    /// </summary>
    public interface ILogicalElement : IExecutable, IModelElement, ILogicalExecutionPreProcessable
    {

    }

    public abstract class AbstractUnfinalizedLogicalElement<ConcreteType, TargetType> : AbstractUnfinalizedModelElement<ConcreteType, TargetType>, IUnfinalizedLogicalElement
        where ConcreteType : AbstractUnfinalizedLogicalElement<ConcreteType, TargetType>
        where TargetType : AbstractLogicalElement<ConcreteType, TargetType>
    {
        public ILogicalElement FinalizeUntypedLogicalElement(ModelFinalizationMappings mappings)
        {
            return Finalize(mappings);
        }

        public abstract IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room);
    }

    /// <summary>
    /// The untyped interface portion of <see cref="AbstractUnfinalizedLogicalElement{ConcreteType, TargetType}"/>.
    /// </summary>
    public interface IUnfinalizedLogicalElement: IUnfinalizedModelElement
    {
        /// <summary>
        /// An untyped version of <see cref="AbstractUnfinalizedModelElement{ConcreteType, TargetType}.Finalize(ModelFinalizationMappings)"/>, specifically for
        /// a <see cref="AbstractUnfinalizedLogicalElement{ConcreteType, TargetType}"/>.
        /// </summary>
        /// <param name="mappings">A model containing mappings between unfinalized instances and corresponding finalized instances</param>
        /// <returns></returns>
        public ILogicalElement FinalizeUntypedLogicalElement(ModelFinalizationMappings mappings);

        /// <summary>
        /// If this logical element contains any properties that are an object referenced by another property(which is its identifier), initializes them.
        /// Also delegates to any sub-logical elements.
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this logical element is, or null if it's not in a room</param>
        /// <returns>A sequence of strings describing references that could not be initialized properly.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room);
    }
}
