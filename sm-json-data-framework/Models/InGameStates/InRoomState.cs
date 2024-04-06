﻿using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// Represents the logically-relevant parts of the state of the current room.
    /// </summary>
    public class InRoomState : ReadOnlyInRoomState
    {
        public InRoomState(RoomNode initialNode)
        {
            ApplyEnterRoom(initialNode);
        }

        public InRoomState(InRoomState other)
        {
            InternalVisitedRoomPath = other.InternalVisitedRoomPath.Select(pair => (new InNodeState(pair.nodeState), pair.strat)).ToList();
            DestroyedObstacleIdsSet = new HashSet<string>(other.DestroyedObstacleIdsSet);
            LastStrat = other.LastStrat;
        }

        public InRoomState Clone()
        {
            return new InRoomState(this);
        }

        public ReadOnlyInRoomState AsReadOnly()
        {
            return this;
        }


        protected InNodeState InternalCurrentNodeState { get => InternalVisitedRoomPath.Any() ? InternalVisitedRoomPath.Last().nodeState : null; }

        public ReadOnlyInNodeState CurrentNodeState { get { return InternalCurrentNodeState?.AsReadOnly(); } }

        public RoomNode CurrentNode { get => InternalCurrentNodeState?.Node; }

        public bool BypassedExitLock
        {
            get
            {
                InNodeState nodeState = InternalCurrentNodeState;
                return nodeState != null && nodeState.BypassedLocks.Any();
            }
        }

        public bool OpenedExitLock
        {
            get
            {
                InNodeState nodeState = InternalCurrentNodeState;
                return nodeState != null && nodeState.OpenedLocks.Any();
            }
        }

        public Room CurrentRoom { get => CurrentNode?.Room; }

        /// <summary>
        /// Inner list containing the nodes that have been visited in this room since entering, in order,
        /// starting with the node through which the room was entered.
        /// Each node ID is accompanied by the strat that was used to reach it, when applicable.
        /// This strat can be null since nodes are reached without using a strat when entering.
        /// </summary>
        protected List<(InNodeState nodeState, Strat strat)> InternalVisitedRoomPath { get; } = new List<(InNodeState, Strat)>();

        public IEnumerable<(ReadOnlyInNodeState nodeState, Strat strat)> VisitedRoomPath
        {
            get
            {
                return InternalVisitedRoomPath.Select<(InNodeState nodeState, Strat strat), (ReadOnlyInNodeState nodeState, Strat strat)>
                    (pair => (pair.nodeState.AsReadOnly(), pair.strat));
            }
        }

        /// <summary>
        /// The inner HashSet containing the ID of obstacles that have been destroyed in this room since entering.
        /// </summary>
        protected HashSet<string> DestroyedObstacleIdsSet { get; set; } = new HashSet<string>();

        public ReadOnlySet<string> DestroyedObstacleIds { get { return DestroyedObstacleIdsSet.AsReadOnly(); } }

        public Strat LastStrat { get; protected set; }

        /// <summary>
        /// Sets this InRoomState's state to that of immediate entry of a room via the provided entry node.
        /// This may actually place the player at a different node if the node calls for it.
        /// </summary>
        /// <param name="entryNode">The node by which the room is being entered.</param>
        public void ApplyEnterRoom(RoomNode entryNode)
        {
            ClearRoomState();

            if (entryNode != null)
            {
                // Visit entry node immediately
                ApplyVisitNode(entryNode, null);

                // If Samus is considered to spawn at a different node, it means she visits that node after entry before player input.
                if (entryNode.SpawnAtNode != entryNode)
                {
                    ApplyVisitNode(entryNode.SpawnAtNode, null);
                }
            }
        }

        /// <summary>
        /// Updates the in-room state by moving the player to the provided node.
        /// </summary>
        /// <param name="nodeId">ID of the node to visit.</param>
        /// <param name="stratName">The name of the strat through which the node is being reached. Can be null, but only for the first node visited in the room, 
        /// and for the second node only if Samus is seen as spawning there. Additionally, MUST be null for the first node visited in the room.
        /// If not null, must be present on a link that connects previous node to new node.</param>
        /// <exception cref="ArgumentException">Thrown if the nodeId doesn't exist in the room, or if the stratName doesn't exist on the link between the nodes. 
        /// Also thrown if a strat is provided when the node visit is part of spawning in the room; If visit is not part of spawning, 
        /// thrown if there is no link from current node to target, or if no strat from that link is provided.</exception>
        public void ApplyVisitNode(int nodeId, string stratName)
        {
            CurrentRoom.Nodes.TryGetValue(nodeId, out RoomNode node);
            if(node == null)
            {
                throw new ArgumentException($"Node {nodeId} doesn't exist in room '{CurrentRoom.Name}'");
            }
            CurrentNode.Links.TryGetValue(nodeId, out LinkTo linkTo);
            if (linkTo == null)
            {
                throw new ArgumentException($"There is no link from current node {CurrentNode.Id} to target node {node.Id} in room '{CurrentRoom.Name}'");
            }
            Strat strat = null;
            if(stratName != null)
            {
                linkTo.Strats.TryGetValue(stratName, out strat);
                if (strat == null)
                {
                    throw new ArgumentException($"There is no strat '{stratName}' in the link from current node {CurrentNode.Id} to target node {node.Id} in room '{CurrentRoom.Name}'");
                }
            }

            ApplyVisitNode(node, strat);
        }

        /// <summary>
        /// Updates the in-room state by moving the player to the provided node.
        /// </summary>
        /// <param name="node">Node to visit.</param>
        /// <param name="strat">The strat through which the node is being reached. Can be null, but only for the first node visited in the room, 
        /// and for the second node only if Samus is seen as spawning there. Additionally, MUST be null for the first node visited in the room.
        /// If not null, must be present on a link that connects previous node to new node.</param>
        /// <exception cref="ArgumentException">Thrown if a strat is provided when the node visit is part of spawning in the room; If visit is not part of spawning, 
        /// thrown if there is no link from current node to target, or if no strat from that link is provided.</exception>
        public void ApplyVisitNode(RoomNode node, Strat strat)
        {
            // Only allow Strat to be null if this is the first visited node in current room visit, or if this is the second visited node
            // and the second node is where the first node causes Samus to be considered to spawn.
            if (strat == null && VisitedRoomPath.Any() && (VisitedRoomPath.Count() > 1 || VisitedRoomPath.First().nodeState.Node.OverrideSpawnAtNodeId != node.Id))
            {
                throw new ArgumentException("A strat must be provided when visiting a node except when spawning in the room.");
            }

            if (strat != null)
            {
                if (!VisitedRoomPath.Any())
                {
                    throw new ArgumentException("A strat must not be provided when spawning in the room.");
                }

                CurrentNode.Links.TryGetValue(node.Id, out LinkTo link);
                if (link == null)
                {
                    throw new ArgumentException($"There is no link from current node {CurrentNode.Id} to target node {node.Id} in room '{CurrentRoom.Name}'");
                }
                else
                {
                    link.Strats.TryGetValue(strat.Name, out Strat existingStrat);
                    if (existingStrat == null || existingStrat != strat)
                    {
                        throw new ArgumentException($"The specified strat must be in a link fromfrom current node {CurrentNode.Id} to target node {node.Id} in room '{CurrentRoom.Name}'");
                    }
                }
            }

            InternalVisitedRoomPath.Add((new InNodeState(node), strat));
            LastStrat = strat;
        }

        /// <summary>
        /// Updates the in-room state to contain a mention of the destruction of the obstacle in the current room with the provided ID.
        /// </summary>
        /// <param name="obstacleId">ID of the obstacle to destroy.</param>
        /// <exception cref="ArgumentException">Thrown if the obstacle is not in the current room</exception>
        public void ApplyDestroyObstacle(string obstacleId)
        {
            CurrentRoom.Obstacles.TryGetValue(obstacleId, out RoomObstacle obstacle);
            if(obstacle == null)
            {
                throw new ArgumentException($"Obstacle '{obstacleId}' not present in current room '{CurrentRoom.Name}'");
            }
            ApplyDestroyObstacleSafe(obstacle);
        }

        /// <summary>
        /// Updates the in-room state to contain a mention of the destruction of the provided obstacle.
        /// </summary>
        /// <param name="obstacle">The obstacle to destroy.</param>
        /// <exception cref="ArgumentException">Thrown if the obstacle is not in the current room</exception>
        public void ApplyDestroyObstacle(RoomObstacle obstacle)
        {
            CurrentRoom.Obstacles.TryGetValue(obstacle.Id, out RoomObstacle foundObstacle);
            if(foundObstacle != obstacle)
            {
                throw new ArgumentException("Provided obstacle must exist in current room");
            }
            ApplyDestroyObstacleSafe(obstacle);
        }

        /// <summary>
        /// Does the actual updates of the in-room state to contain a mention of the destruction of the provided obstacle.
        /// This should only be called by one of the public ApplyDestroyObstacle() methods, after validating the operation.
        /// </summary>
        /// <param name="obstacle">The obstacle to destroy.</param>
        protected void ApplyDestroyObstacleSafe(RoomObstacle obstacle)
        {
            DestroyedObstacleIdsSet.Add(obstacle.Id);
        }

        /// <summary>
        /// Registers the lock with the provided name as being opened at the current node.
        /// </summary>
        /// <param name="nodeLock">Lock being opened</param>
        public void ApplyOpenLock(string lockName)
        {
            InternalCurrentNodeState.ApplyOpenLock(lockName);
        }


        /// <summary>
        /// Registers the provided NodeLock as being opened at the current node.
        /// </summary>
        /// <param name="nodeLock">Lock being opened</param>
        public void ApplyOpenLock(NodeLock nodeLock)
        {
            InternalCurrentNodeState.ApplyOpenLock(nodeLock);
        }

        /// <summary>
        /// Registers the lock with the provided name as being bypassed at the current node.
        /// </summary>
        /// <param name="nodeLock">Lock being bypassed</param>
        public void ApplyBypassLock(string lockName)
        {
            InternalCurrentNodeState.ApplyBypassLock(lockName);
        }

        /// <summary>
        /// Registers the provided NodeLock as being bypassed at the current node.
        /// </summary>
        /// <param name="nodeLock">Lock being bypassed</param>
        public void ApplyBypassLock(NodeLock nodeLock)
        {
            InternalCurrentNodeState.ApplyBypassLock(nodeLock);
        }

        public IEnumerable<NodeLock> OpenedExitLocks
        {
            get
            {
                InNodeState nodeState = InternalCurrentNodeState;
                if (nodeState == null)
                {
                    return Enumerable.Empty<NodeLock>();
                }
                else
                {
                    return nodeState.OpenedLocks;
                }
            }
        }

        public IEnumerable<NodeLock> BypassedExitLocks {
            get
            {
                InNodeState nodeState = InternalCurrentNodeState;
                if (nodeState == null)
                {
                    return Enumerable.Empty<NodeLock>();
                }
                else
                {
                    return nodeState.BypassedLocks;
                }
            }
        }

        /// <summary>
        /// Removes all data from this InRoomState. Useful if this has been initialized at a starting node but in-room state is not going to be maintained.
        /// </summary>
        public void ClearRoomState()
        {
            DestroyedObstacleIdsSet.Clear();
            InternalVisitedRoomPath.Clear();
            LastStrat = null;
        }
    }

    /// <summary>
    /// Exposes the read-only portion of an <see cref="InRoomState"/>.
    /// </summary>
    public interface ReadOnlyInRoomState
    {
        /// <summary>
        /// Creates and returns a copy of this InRoomState, as a full-fledged modifiable one.
        /// </summary>
        /// <returns>The clone</returns>
        public InRoomState Clone();

        /// <summary>
        /// The logically-relevant information about Samus' current visit to the current node.
        /// </summary>
        public ReadOnlyInNodeState CurrentNodeState { get; }

        /// <summary>
        /// The node the player is currently at. This can be null if in-room state isn't being tracked.
        /// </summary>
        public RoomNode CurrentNode { get; }

        /// <summary>
        /// Indicates whether the room described by this state was exited by bypassing the exit door's lock (based on the premise that the room was indeed exited).
        /// </summary>
        public bool BypassedExitLock { get; }

        /// <summary>
        /// Indicates whether the room described by this state was exited by opening an exit door's lock (based on the premise that the room was indeed exited)
        /// </summary>
        public bool OpenedExitLock { get; }

        /// <summary>
        /// The room the player is currently in. This can be null if in-room state isn't being tracked.
        /// </summary>
        public Room CurrentRoom { get; }

        /// <summary>
        /// Enumeration of the nodes that have been visited in this room since entering, in order, starting with the node through which the room was entered.
        /// Each node ID is accompanied by the strat that was used to reach it, when applicable.
        /// This strat can be null since nodes are reached without using a strat when entering.
        /// </summary>
        public IEnumerable<(ReadOnlyInNodeState nodeState, Strat strat)> VisitedRoomPath { get; }

        /// <summary>
        /// A read-only set of IDs of obstacles that have been destroyed in this room since entering.
        /// </summary>
        public ReadOnlySet<string> DestroyedObstacleIds { get; }

        /// <summary>
        /// The strat that was used to reach the current node, if any. Otherwise, is null.
        /// </summary>
        public Strat LastStrat { get; }

        /// <summary>
        /// The locks opened by Samus in the last node she visited in this room.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NodeLock> OpenedExitLocks { get; }

        /// <summary>
        /// The locks bypassed by Samus in the last node she visited in this room.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NodeLock> BypassedExitLocks { get; }
    }
}
