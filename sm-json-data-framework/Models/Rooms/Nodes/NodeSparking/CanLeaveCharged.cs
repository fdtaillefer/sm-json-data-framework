using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Node.NodeSparking
{
    public class CanLeaveCharged
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

        /// <summary>
        /// Initializes additional properties in this CanLeaveCharged, which wouldn't be initialized by simply parsing a rooms json file.
        /// All such properties are identified in their own documentation and should not be read if this method isn't called.
        /// </summary>
        /// <param name="model">The model to use to initialize the additional properties</param>
        /// <param name="node">The node in which this CanLeaveCharged is</param>
        public void Initialize(SuperMetroidModel model, RoomNode node)
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

        /// <summary>
        /// Goes through all logical elements within this CanLeaveCharged (and all LogicalRequirements within any of them),
        /// attempting to initialize any property that is an object referenced by another property(which is its identifier).
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this CanLeaveCharged is</param>
        /// <returns>A sequence of strings describing references that could not be initialized properly.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
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
