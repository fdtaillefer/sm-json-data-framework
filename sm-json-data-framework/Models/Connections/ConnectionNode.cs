using sm_json_data_framework.Models.Raw.Connections;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Connections
{
    /// <summary>
    /// A node within a <see cref="Connection"/>. It represents either the origin or destination of the one-way connection.
    /// </summary>
    public class ConnectionNode : AbstractModelElement<UnfinalizedConnectionNode, ConnectionNode>
    {
        public ConnectionNode(UnfinalizedConnectionNode sourceElement, Action<ConnectionNode> mappingsInsertionCallback) : base(sourceElement, mappingsInsertionCallback)
        {
            Area = sourceElement.Area;
            Subarea = sourceElement.Subarea;
            Roomid = sourceElement.Roomid;
            RoomName = sourceElement.RoomName;
            Nodeid = sourceElement.Nodeid;
            NodeName = sourceElement.NodeName;
            Position = sourceElement.Position;
        }

        /// <summary>
        /// The area of the room this ConnectionNode is in. See <see cref="Room.Area"/>.
        /// </summary>
        public string Area { get; }

        /// <summary>
        /// The sub-area of the room this ConnectionNode is in. See <see cref="Room.Subarea"/>.
        /// </summary>
        public string Subarea  { get; }

        /// <summary>
        /// Numerical ID of the room this ConnectionNode is in. See <see cref="Room.Id"/>.
        /// </summary>
        public int Roomid { get; }

        /// <summary>
        /// Name of the room this ConnectionNode is in. See <see cref="Room.Name"/>.
        /// </summary>
        public string RoomName { get; }

        /// <summary>
        /// In-room ID of the <see cref="RoomNode"/> this ConnectionNode references. See <see cref="RoomNode.Id"/>.
        /// </summary>
        public int Nodeid { get; }

        /// <summary>
        /// Name of the <see cref="RoomNode"/> this ConnectionNode references. See <see cref="RoomNode.Name"/>.
        /// </summary>
        public string NodeName { get; }

        /// <summary>
        /// Where this ConnectionNode is positioned geometrically relative to the connection.
        /// </summary>
        public ConnectionNodePositionEnum Position { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidModel model)
        {
            // Nothing to do here
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidModel model)
        {
            // The two ends of a connection are always relevant given their connection is
            return true;
        }
    }

    public class UnfinalizedConnectionNode : AbstractUnfinalizedModelElement<UnfinalizedConnectionNode, ConnectionNode>
    {
        public string Area { get; set; }

        public string Subarea { get; set; }

        public int Roomid { get; set; }

        public string RoomName { get; set; }

        public int Nodeid { get; set; }

        public string NodeName { get; set; }

        public ConnectionNodePositionEnum Position { get; set; }

        public UnfinalizedConnectionNode()
        {

        }

        public UnfinalizedConnectionNode(RawConnectionNode rawNode)
        {
            Area = rawNode.Area;
            Subarea = rawNode.Subarea;
            Roomid = rawNode.Roomid;
            RoomName = rawNode.RoomName;
            Nodeid = rawNode.Nodeid;
            NodeName = rawNode.NodeName;
            Position = rawNode.Position;
        }

        protected override ConnectionNode CreateFinalizedElement(UnfinalizedConnectionNode sourceElement, Action<ConnectionNode> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new ConnectionNode(sourceElement, mappingsInsertionCallback);
        }

        /// <summary>
        /// <para>A string that identifies this node, often used as a key in Dictionaries.</para>
        /// </summary>
        public string IdentifyingString { get => SuperMetroidUtils.BuildNodeIdentifyingString(RoomName, Nodeid); }
    }
}
