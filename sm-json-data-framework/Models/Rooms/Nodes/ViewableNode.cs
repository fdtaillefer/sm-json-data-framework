﻿using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class ViewableNode: AbstractModelElement, InitializablePostDeserializeInNode
    {
        [JsonPropertyName("id")]
        public int NodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, Room)"/> has been called.</para>
        /// <para>The node that is viewable</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node { get; set; }

        /// <summary>
        /// The strats that can be executed to view the node, mapped by name.
        /// </summary>
        public IDictionary<string, Strat> Strats { get; set; } = new Dictionary<string, Strat>();

        public ViewableNode()
        {

        }

        public ViewableNode(RawViewableNode rawViewableNode, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            NodeId = rawViewableNode.Id;
            Strats = rawViewableNode.Strats.Select(strat => new Strat(strat, knowledgeBase)).ToDictionary(strat => strat.Name);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            bool noUsefulStrat = true;
            foreach (Strat strat in Strats.Values)
            {
                strat.ApplyLogicalOptions(logicalOptions);
                if (!strat.UselessByLogicalOptions)
                {
                    noUsefulStrat = false;
                }
            }

            // A runway becomes useless if its strats are impossible
            return noUsefulStrat;
        }

        public void InitializeProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            // Initialize Node
            Node = room.Nodes[NodeId];

            // Initialize Strats
            foreach (Strat strat in Strats.Values)
            {
                strat.InitializeProperties(model, room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            List<string> unhandled = new List<string>();

            foreach(Strat strat in Strats.Values)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
