﻿using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    /// <summary>
    /// <para>A node in the path that goes from a <see cref="CanLeaveCharged"/>'s remote initiation node to the exit door,
    /// as obtained from the json model.</para>
    /// <para>It contains the ID of a destination node, and the names of strats that can be used to get there. 
    /// The origin node is figured out by the previous node in the path.
    /// The first node in a path originates at the remote initiation node.</para>
    /// </summary>
    public class InitiateRemotelyPathToDoorNode
    {
        public int DestinationNodeId { get; set; }

        public ISet<string> StratNames { get; set; } = new HashSet<string>();

        public InitiateRemotelyPathToDoorNode()
        {

        }

        public InitiateRemotelyPathToDoorNode(RawInitiateRemotelyPathToDoorNode pathNode)
        {
            DestinationNodeId = pathNode.DestinationNode;
            StratNames = new HashSet<string>(pathNode.Strats);
        }
    }
}
