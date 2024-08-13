using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// Represents how a parent <see cref="Link"/> can lead to a given node, and how to get there.
    /// </summary>
    public class LinkTo : AbstractModelElement<UnfinalizedLinkTo, LinkTo>, ILogicalExecutionPreProcessable
    {
        public LinkTo(UnfinalizedLinkTo sourceElement, Action<LinkTo> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            TargetNode = sourceElement.TargetNode.Finalize(mappings);
            Strats = sourceElement.Strats.Values.Select(strat => strat.Finalize(mappings)).ToDictionary(strat => strat.Name).AsReadOnly();
        }

        /// <summary>
        /// The node that this LinkTo leads to
        /// </summary>
        public RoomNode TargetNode { get; }

        /// <summary>
        /// The strats that can be executed to follow this LinkTo, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Strat> Strats { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            foreach (Strat strat in Strats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions, model);
            }
        }

        protected override void UpdateLogicalProperties(SuperMetroidModel model)
        {
            base.UpdateLogicalProperties(model);
            LogicallyNever = CalculateLogicallyNever(model);
            LogicallyAlways = CalculateLogicallyAlways(model);
            LogicallyFree = CalculateLogicallyFree(model);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidModel model)
        {
            // A linkTo has no logical relevance if it's impossible to follow
            return !CalculateLogicallyNever(model);
        }

        /// <summary>
        /// If true, then this strat is impossible to execute given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidModel model)
        {
            // A LinkTo is impossible if it has no possible strats
            return !Strats.Values.Any(strat => !strat.LogicallyNever);
        }

        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways(SuperMetroidModel model)
        {
            // A LinkTo is always possible if it at least one strat that also is
            return Strats.Values.WhereLogicallyAlways().Any();
        }

        public bool LogicallyFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyFree"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyFree(SuperMetroidModel model)
        {
            // A LinkTo is free if it at least one strat that also is
            return Strats.Values.WhereLogicallyFree().Any();
        }
    }

    public class UnfinalizedLinkTo : AbstractUnfinalizedModelElement<UnfinalizedLinkTo, LinkTo>, InitializablePostDeserializeInRoom
    {
        public int TargetNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The node that this link leads to</para>
        /// </summary>
        public UnfinalizedRoomNode TargetNode { get; set; }

        /// <summary>
        /// The strats that can be executed to follow this LinkTo, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedStrat> Strats { get; set; } = new Dictionary<string, UnfinalizedStrat>();

        public UnfinalizedLinkTo()
        {

        }

        public UnfinalizedLinkTo(RawLinkTo rawLinkTo, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            TargetNodeId = rawLinkTo.Id;
            Strats = rawLinkTo.Strats.Select(rawStrat => new UnfinalizedStrat(rawStrat, knowledgeBase)).ToDictionary(strat => strat.Name);
        }

        protected override LinkTo CreateFinalizedElement(UnfinalizedLinkTo sourceElement, Action<LinkTo> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new LinkTo(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            // Initialize TargetNode
            TargetNode = room.Nodes[TargetNodeId];

            // Initialize strats
            foreach (UnfinalizedStrat strat in Strats.Values)
            {
                strat.InitializeProperties(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            foreach(UnfinalizedStrat strat in Strats.Values)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
