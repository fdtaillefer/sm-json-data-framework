﻿using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
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
        private UnfinalizedStratFailure InnerElement { get; set; }

        public StratFailure(UnfinalizedStratFailure innerElement, Action<StratFailure> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            LeadsToNode = InnerElement.LeadsToNode?.Finalize(mappings);
            Cost = InnerElement.Cost.Finalize(mappings);
        }

        /// <summary>
        /// A name given to this StratFailure. This is only unique within the strat.
        /// </summary>
        public string Name { get { return InnerElement.Name; }  }

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
        public bool Softlock { get { return InnerElement.Softlock; } }

        /// <summary>
        /// Whether this failure means Samus should no longer be seen as arriving from the previous node.
        /// </summary>
        public bool ClearsPreviousNode { get { return InnerElement.ClearsPreviousNode; }  }
    }

    public class UnfinalizedStratFailure : AbstractUnfinalizedModelElement<UnfinalizedStratFailure, StratFailure>, InitializablePostDeserializeInRoom
    {
        public string Name { get; set; }

        [JsonPropertyName("leadsToNode")]
        public int? LeadsToNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/> has been called.</para>
        /// <para>The node that this strat failure leads to, if it leads to a node</para>
        /// </summary>
        [JsonIgnore]
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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            Cost.ApplyLogicalOptions(logicalOptions);

            // A failure object is not useless even if it describes an effective softlock
            return false;
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
