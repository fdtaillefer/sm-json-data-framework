using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Node
{
    public class CanLeaveCharged : InitializablePostDeserializeInNode
    {
        public int UsedTiles { get; set; }

        public int FramesRemaining { get; set; }

        public int ShinesparkFrames { get; set; }

        [JsonPropertyName("initiateAt")]
        public int? OverrideInitiateAtNodeId { get; set; }

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, RoomNode)"/> has been called.</para>
        /// <para>The node referenced by the <see cref="OverrideInitiateAtNodeId"/> property, if any.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode OverrideInitiateAtNode { get; set; }

        /// <summary>
        /// <para>Not reliable before <see cref="Initialize(SuperMetroidModel)"/> has been called.</para>
        /// <para>The node at which Samus actually spawns upon entering the room via this node. In most cases it will be this node, but not always.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode InitiateAtNode { get { return OverrideInitiateAtNode ?? Node; } }

        public IEnumerable<Strat> Strats { get; set; } = Enumerable.Empty<Strat>();

        public int OpenEnd { get; set; } = 0;

        public int GentleUpTiles { get; set; } = 0;

        public int GentleDownTiles { get; set; } = 0;

        public int SteepUpTiles { get; set; } = 0;

        public int SteepDownTiles { get; set; } = 0;

        public int StartingDownTiles { get; set; } = 0;

        /// <summary>
        /// <para>Not available before <see cref="Initialize(SuperMetroidModel, RoomNode)"/> has been called.</para>
        /// <para>The node in which this CanLeaveCharged is.</para>
        /// </summary>
        [JsonIgnore]
        public RoomNode Node { get; set; }

        public void Initialize(SuperMetroidModel model, Room room, RoomNode node)
        {
            Node = node;

            // Initialize OverrideInitiateAtNode
            if (OverrideInitiateAtNodeId != null)
            {
                OverrideInitiateAtNode = node.Room.Nodes[(int)OverrideInitiateAtNodeId];
            }

            foreach(Strat strat in Strats)
            {
                strat.Initialize(model, node.Room);
            }
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room, RoomNode node)
        {
            List<string> unhandled = new List<string>();

            foreach(Strat strat in Strats)
            {
                unhandled.AddRange(strat.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
