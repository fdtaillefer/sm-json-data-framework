using sm_json_data_framework.Models.Raw.Connections;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Connections
{
    /// <summary>
    /// Description of a one-way game transition from an origin node to a destination node.
    /// Contains the nodes as they are described in the json model.
    /// </summary>
    public class Connection : AbstractModelElement<UnfinalizedConnection, Connection>
    {
        private UnfinalizedConnection InnerElement { get; set; }

        public Connection(UnfinalizedConnection innerElement, Action<Connection> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            FromNode = InnerElement.FromNode.Finalize(mappings);
            ToNode = InnerElement.ToNode.Finalize(mappings);
        }

        /// <summary>
        /// The type of connection this is.
        /// </summary>
        public ConnectionTypeEnum ConnectionType { get { return InnerElement.ConnectionType; } }

        /// <summary>
        /// Details about the origin node of this Connection.
        /// </summary>
        public ConnectionNode FromNode { get; }

        /// <summary>
        /// Details about the destination node of this Connection.
        /// </summary>
        public ConnectionNode ToNode { get; }
    }

    public class UnfinalizedConnection : AbstractUnfinalizedModelElement<UnfinalizedConnection, Connection>
    {
        public ConnectionTypeEnum ConnectionType { get; set; }

        public UnfinalizedConnectionNode FromNode { get; set; }

        public UnfinalizedConnectionNode ToNode { get; set; }

        public UnfinalizedConnection() { 

        }

        public UnfinalizedConnection(RawConnection rawConnection, RawConnectionNode fromNode, RawConnectionNode toNode)
        {
            ConnectionType = rawConnection.ConnectionType;
            FromNode = new UnfinalizedConnectionNode(fromNode);
            ToNode = new UnfinalizedConnectionNode(toNode);
        }

        protected override Connection CreateFinalizedElement(UnfinalizedConnection sourceElement, Action<Connection> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Connection(sourceElement, mappingsInsertionCallback, mappings);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Logical options have no power here, but we can still delegate to the nodes
            FromNode.ApplyLogicalOptions(logicalOptions);
            ToNode.ApplyLogicalOptions(logicalOptions);
            return false;
        }
    }
}
