﻿using sm_json_data_framework.Models.Raw.Connections;
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
        public Connection(UnfinalizedConnection innerElement, Action<Connection> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            ConnectionType = innerElement.ConnectionType;
            FromNode = innerElement.FromNode.Finalize(mappings);
            ToNode = innerElement.ToNode.Finalize(mappings);
        }

        /// <summary>
        /// The type of connection this is.
        /// </summary>
        public ConnectionTypeEnum ConnectionType { get; }

        /// <summary>
        /// Details about the origin node of this Connection.
        /// </summary>
        public ConnectionNode FromNode { get; }

        /// <summary>
        /// Details about the destination node of this Connection.
        /// </summary>
        public ConnectionNode ToNode { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            FromNode.ApplyLogicalOptions(logicalOptions);
            ToNode.ApplyLogicalOptions(logicalOptions);
        }

        public override bool CalculateLogicallyRelevant()
        {
            // Arguably a connection might not be relevant if on a node that's impossible to interact with,
            // but that's a matter of layout and not logic, and is out of scope here.
            return true;
        }
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
    }
}
