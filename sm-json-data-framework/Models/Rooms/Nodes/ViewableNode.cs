using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    public class ViewableNode: InitializablePostDeserializeInNode
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

        public ViewableNode(RawViewableNode viewableNode, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            NodeId = viewableNode.Id;
            Strats = viewableNode.Strats.Select(strat => new Strat(strat, knowledgeBase)).ToDictionary(strat => strat.Name);
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

        public bool CleanUpUselessValues(SuperMetroidModel model, Room room, RoomNode node)
        {
            Strats = Strats.Where(kvp => kvp.Value.CleanUpUselessValues(model, room)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            // If there no usable strats remaining to view the node, this Viewable node becomes useless
            return Strats.Any();
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
