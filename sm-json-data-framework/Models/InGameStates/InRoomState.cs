﻿using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// Represents the logically-relevant parts of the state of the current room.
    /// </summary>
    public class InRoomState
    {

        public InRoomState(RoomNode initialNode)
        {
            ApplyEnterRoom(initialNode);
        }
        
        public InRoomState(InRoomState other)
        {
            VisitedRoomPathList = other.VisitedRoomPathList.Select(pair => (new InNodeState(pair.nodeState), pair.strat)).ToList();
            DestroyedObstacleIdsSet = new HashSet<string>(other.DestroyedObstacleIdsSet);
            LastStrat = other.LastStrat;
        }

        public InNodeState CurrentNodeState { get => VisitedRoomPathList.Any() ? VisitedRoomPathList.Last().nodeState : null; }

        /// <summary>
        /// The node the player is currently at. This can be null if in-room state isn't being tracked.
        /// </summary>
        /// // Current room is the last node in the visited room path
        public RoomNode CurrentNode { get => CurrentNodeState?.Node;  }

        /// <summary>
        /// Indicates whether the room described by this state was exited by bypassing the exit door's lock (based on the premise that the room was indeed exited).
        /// </summary>
        public bool BypassedExitLock { get
            {
                InNodeState state = CurrentNodeState;
                return state != null && state.BypassedLocks.Any();
            }
        }

        /// <summary>
        /// Indicates whether the room described by this state was exited by opening an exit door's lock (based on the premise that the room was indeed exited)
        /// </summary>
        public bool OpenedExitLock
        {
            get
            {
                InNodeState state = CurrentNodeState;
                return state != null && state.OpenedLocks.Any();
            }
        }

        /// <summary>
        /// The room the player is currently in. This can be null if in-room state isn't being tracked.
        /// </summary>
        public Room CurrentRoom { get => CurrentNode?.Room; }

        /// <summary>
        /// Inner list containing the nodes that have been visited in this room since entering, in order,
        /// starting with the node through which the room was entered.
        /// Each node ID is accompanied by the strat that was used to reach it, when applicable.
        /// This strat can be null since nodes are reached without using a strat when entering.
        /// </summary>
        protected List<(InNodeState nodeState, Strat strat)> VisitedRoomPathList { get; } = new List<(InNodeState, Strat)>();

        // This will just return VisitedRoomPathList as an IEnumerable
        /// <summary>
        /// A sequence of nodes that have been visited in this room since entering, in order,
        /// starting with the node through which the room was entered.
        /// Each node ID is accompanied by the strat that was used to reach it, when applicable.
        /// This strat can be null since nodes are reached without using a strat when entering.
        /// </summary>
        public IEnumerable<(InNodeState nodeState, Strat strat)> VisitedRoomPath { get { return VisitedRoomPathList; } }

        /// <summary>
        /// The inner HashSet containing the ID of obstacles that have been destroyed in this room since entering.
        /// </summary>
        protected HashSet<string> DestroyedObstacleIdsSet { get; set; } = new HashSet<string>();

        // This will just return DestroyedObstacleIdsSet as an IEnumerable
        /// <summary>
        /// A sequence of IDs of obstacles that have been destroyed in this room since entering.
        /// </summary>
        public IEnumerable<string> DestroyedObstacleIds { get { return DestroyedObstacleIdsSet; } }

        /// <summary>
        /// The strat that was used to reach the current node, if any. Otherwise, is null.
        /// </summary>
        public Strat LastStrat { get; protected set; }

        /// <summary>
        /// Sets this InRoomState's state to that of immediate entry of a room via the provided entry node.
        /// This may actually place the player at a different node if the node calls for it.
        /// </summary>
        /// <param name="entryNode">The node by which the room is being entered.</param>
        public void ApplyEnterRoom(RoomNode entryNode)
        {
            ClearRoomState();

            if(entryNode != null)
            {
                // Visit entry node immediately
                ApplyVisitNode(entryNode, null);

                // If Samus is considered to spawn at a different node, it means she visits that node after entry before player input.
                if(entryNode.SpawnAtNode != entryNode)
                {
                    ApplyVisitNode(entryNode.SpawnAtNode, null);
                }
            }
        }

        /// <summary>
        /// Updates the in-room state by moving the player to the provided node. Should not be called for a node that is not in the current room.
        /// </summary>
        /// <param name="node">Node to visit.</param>
        /// <param name="strat">The strat through which the node is being reached. Can be null. If not null, only makes sense if 
        /// it's on a link that connects previous node to new node.</param>
        public void ApplyVisitNode(RoomNode node, Strat strat)
        {
            VisitedRoomPathList.Add((new InNodeState(node), strat));
            LastStrat = strat;
        }

        /// <summary>
        /// Updates the in-room state to contain a mention of the destruction of the provided obstacle.
        /// Should not be called for an obstacle that is not in the current room.
        /// </summary>
        /// <param name="obstacle">The obstacle to destroy.</param>
        public void ApplyDestroyedObstacle(RoomObstacle obstacle)
        {
            DestroyedObstacleIdsSet.Add(obstacle.Id);
        }

        /// <summary>
        /// Registers the provided NodeLock as being opened at the current node.
        /// </summary>
        /// <param name="nodeLock">Lock being opened</param>
        public void ApplyOpenLock(NodeLock nodeLock)
        {
            CurrentNodeState.ApplyOpenLock(nodeLock);
        }

        /// <summary>
        /// Registers the provided NodeLock as being bypassed at the current node.
        /// </summary>
        /// <param name="nodeLock">Lock being bypassed</param>
        public void ApplyBypassLock(NodeLock nodeLock)
        {
            CurrentNodeState.ApplyBypassLock(nodeLock);
        }

        /// <summary>
        /// Returns the locks bypassed by Samus in the last node she visited in this room.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NodeLock> GetBypassedLocks()
        {
            InNodeState nodeState = CurrentNodeState;
            if(nodeState == null)
            {
                return Enumerable.Empty<NodeLock>();
            }
            else
            {
                return nodeState.BypassedLocks;
            }
        }

        /// <summary>
        /// Removes all data from this InRoomState. Useful if this has been initialized at a starting node but in-room state is not going to be maintained.
        /// </summary>
        public void ClearRoomState()
        {
            DestroyedObstacleIdsSet.Clear();
            VisitedRoomPathList.Clear();
            LastStrat = null;
        }
    }
}
