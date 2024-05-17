using sm_json_data_framework.Models.Raw.Connections;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
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
        private UnfinalizedConnectionNode InnerElement { get; set; }

        public ConnectionNode(UnfinalizedConnectionNode innerElement, Action<ConnectionNode> mappingsInsertionCallback) : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
        }

        /// <summary>
        /// The area of the room this ConnectionNode is in. See <see cref="Room.Area"/>.
        /// </summary>
        public string Area { get { return InnerElement.Area; } }

        /// <summary>
        /// The sub-area of the room this ConnectionNode is in. See <see cref="Room.Subarea"/>.
        /// </summary>
        public string Subarea { get { return InnerElement.Subarea; } }

        /// <summary>
        /// Numerical ID of the room this ConnectionNode is in. See <see cref="Room.Id"/>.
        /// </summary>
        public int Roomid { get { return InnerElement.Roomid; } }

        /// <summary>
        /// Name of the room this ConnectionNode is in. See <see cref="Room.Name"/>.
        /// </summary>
        public string RoomName { get { return InnerElement.RoomName; } }

        /// <summary>
        /// In-room ID of the <see cref="RoomNode"/> this ConnectionNode references. See <see cref="RoomNode.Id"/>.
        /// </summary>
        public int Nodeid { get { return InnerElement.Nodeid; } }

        /// <summary>
        /// Name of the <see cref="RoomNode"/> this ConnectionNode references. See <see cref="RoomNode.Name"/>.
        /// </summary>
        public string NodeName { get { return InnerElement.NodeName; } }

        /// <summary>
        /// Where this ConnectionNode is positioned geometrically relative to the connection.
        /// </summary>
        public ConnectionNodePositionEnum Position { get { return InnerElement.Position; } }
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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Logical Options have no power here
            return false;
        }

        /// <summary>
        /// <para>A string that identifies this node, often used as a key in Dictionaries.</para>
        /// </summary>
        public string IdentifyingString { get => SuperMetroidUtils.BuildNodeIdentifyingString(RoomName, Nodeid); }
    }
}
