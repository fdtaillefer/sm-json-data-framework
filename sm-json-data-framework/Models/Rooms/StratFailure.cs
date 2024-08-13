using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms
{
    /// <summary>
    /// Represents the consequences of failing a strat in a specific way.
    /// </summary>
    public class StratFailure : AbstractModelElement<UnfinalizedStratFailure, StratFailure>
    {
        public StratFailure(UnfinalizedStratFailure sourceElement, Action<StratFailure> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Name = sourceElement.Name;
            Softlock = sourceElement.Softlock;
            ClearsPreviousNode = sourceElement.ClearsPreviousNode;
            LeadsToNode = sourceElement.LeadsToNode?.Finalize(mappings);
            Cost = sourceElement.Cost.Finalize(mappings);
        }

        /// <summary>
        /// A name given to this StratFailure. This is only unique within the strat.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The node that this strat failure leads to, if it leads to a node. Null otherwise.
        /// </summary>
        public RoomNode LeadsToNode { get; }

        /// <summary>
        /// The cost of this StratFailure. Inability to fulfill these requirements means a softlock.
        /// </summary>
        public LogicalRequirements Cost { get; }

        /// <summary>
        /// Whether this failure is systematically a softlock.
        /// </summary>
        public bool Softlock { get; }

        /// <summary>
        /// Whether this failure means Samus should no longer be seen as arriving from the previous node.
        /// </summary>
        public bool ClearsPreviousNode { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            Cost.ApplyLogicalOptions(logicalOptions, model);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidModel model)
        {
            // A StratFailure remains relevant even if it describes an effective softlock
            return true;
        }
    }

    public class UnfinalizedStratFailure : AbstractUnfinalizedModelElement<UnfinalizedStratFailure, StratFailure>, InitializablePostDeserializeInRoom
    {
        public string Name { get; set; }

        public int? LeadsToNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The node that this strat failure leads to, if it leads to a node</para>
        /// </summary>
        public UnfinalizedRoomNode LeadsToNode { get; set; }

        public UnfinalizedLogicalRequirements Cost { get; set; } = new UnfinalizedLogicalRequirements();

        public bool Softlock { get; set; } = false;

        public bool ClearsPreviousNode { get; set; } = false;

        public UnfinalizedStratFailure()
        {

        }

        public UnfinalizedStratFailure(RawStratFailure rawFailure, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Name = rawFailure.Name;
            LeadsToNodeId = rawFailure.LeadsToNode;
            Cost = rawFailure.Cost.ToLogicalRequirements(knowledgeBase);
            Softlock = rawFailure.Softlock;
            ClearsPreviousNode = rawFailure.ClearsPreviousNode;
        }

        protected override StratFailure CreateFinalizedElement(UnfinalizedStratFailure sourceElement, Action<StratFailure> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new StratFailure(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            // Initialize LeadsToNode
            if (LeadsToNodeId != null)
            {
                LeadsToNode = room.Nodes[(int)LeadsToNodeId];
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Cost.InitializeReferencedLogicalElementProperties(model, room));

            return unhandled.Distinct();
        }
    }
}
