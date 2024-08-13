using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    /// <summary>
    /// Represents the possibility of viewing a node from another node (presumably to see what item is there).
    /// </summary>
    public class ViewableNode : AbstractModelElement<UnfinalizedViewableNode, ViewableNode>, ILogicalExecutionPreProcessable
    {
        public ViewableNode(UnfinalizedViewableNode sourceElement, Action<ViewableNode> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Node = sourceElement.Node.Finalize(mappings);
            Strats = sourceElement.Strats.Values.Select(strat => strat.Finalize(mappings)).ToDictionary(strat => strat.Name).AsReadOnly();
        }

        /// <summary>
        /// The node that is viewable
        /// </summary>
        public RoomNode Node { get; }

        /// <summary>
        /// The strats that can be executed to view the node, mapped by name.
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
            // If a viewableNode cannot be used, it may as well not exist
            return !CalculateLogicallyNever(model);
        }

        /// <summary>
        /// If true, then this viewableNode is impossible to use given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidModel model)
        {
            // A viewableNode is impossible to use if it has no strats that can be executed
            return !Strats.Values.WhereLogicallyRelevant().Any();
        }

        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="model">The model this element belongs to</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways(SuperMetroidModel model)
        {
            // Always possible as long as at least one strat is
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
            // Free as long as at least one strat is
            return Strats.Values.WhereLogicallyFree().Any();
        }
    }

    public class UnfinalizedViewableNode : AbstractUnfinalizedModelElement<UnfinalizedViewableNode, ViewableNode>, InitializablePostDeserializeInNode
    {
        public int NodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The node that is viewable</para>
        /// </summary>
        public UnfinalizedRoomNode Node { get; set; }

        /// <summary>
        /// The strats that can be executed to view the node, mapped by name.
        /// </summary>
        public IDictionary<string, UnfinalizedStrat> Strats { get; set; } = new Dictionary<string, UnfinalizedStrat>();

        public UnfinalizedViewableNode()
        {

        }

        public UnfinalizedViewableNode(RawViewableNode rawViewableNode, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            NodeId = rawViewableNode.Id;
            Strats = rawViewableNode.Strats.Select(strat => new UnfinalizedStrat(strat, knowledgeBase)).ToDictionary(strat => strat.Name);
        }

        protected override ViewableNode CreateFinalizedElement(UnfinalizedViewableNode sourceElement, Action<ViewableNode> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new ViewableNode(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node)
        {
            // Initialize Node
            Node = room.Nodes[NodeId];

            // Initialize Strats
            foreach (UnfinalizedStrat strat in Strats.Values)
            {
                strat.InitializeProperties(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room, UnfinalizedRoomNode node)
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
