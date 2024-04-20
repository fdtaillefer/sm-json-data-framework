using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Rooms;
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
            InternalDestroyedObstacleIds = new HashSet<string>(other.InternalDestroyedObstacleIds);
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

        protected List<(InNodeState nodeState, Strat strat)> InternalVisitedRoomPath { get; } = new List<(InNodeState, Strat)>();

        public IReadOnlyList<(ReadOnlyInNodeState nodeState, Strat strat)> VisitedRoomPath
        {
            get
            {
                return InternalVisitedRoomPath.Select<(InNodeState nodeState, Strat strat), (ReadOnlyInNodeState nodeState, Strat strat)>
                    (pair => (pair.nodeState.AsReadOnly(), pair.strat)).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// The inner HashSet containing the ID of obstacles that have been destroyed in this room since entering.
        /// </summary>
        protected HashSet<string> InternalDestroyedObstacleIds { get; } = new HashSet<string>();

        public ReadOnlySet<string> DestroyedObstacleIds { get { return InternalDestroyedObstacleIds.AsReadOnly(); } }

        /// <summary>
        /// The strat that was used to reach to current node, if any. Can be null if there is no current node 
        /// or current node was reached during the process of spawning in the room.
        /// </summary>
        public Strat LastStrat { get { return VisitedRoomPath.LastOrDefault().strat; } }

        /// <summary>
        /// Sets this InRoomState's state to that of immediate entry of a room via the provided entry node.
        /// This may actually place the player at a different node if the node calls for it.
        /// </summary>
        /// <param name="entryNode">The node by which the room is being entered.</param>
        /// <returns>This, for chaining</returns>
        public InRoomState ApplyEnterRoom(RoomNode entryNode)
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
            return this;
        }

        /// <summary>
        /// Updates the in-room state by moving the player to the provided node.
        /// </summary>
        /// <param name="nodeId">ID of the node to visit.</param>
        /// <param name="stratName">The name of the strat through which the node is being reached. Can be null, but only for the first node visited in the room, 
        /// and for the second node only if Samus is seen as spawning there. Additionally, MUST be null for the first node visited in the room.
        /// If not null, must be present on a link that connects previous node to new node.</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ArgumentException">Thrown if the nodeId doesn't exist in the room, or if the stratName doesn't exist on the link between the nodes. 
        /// Also thrown if a strat is provided when the node visit is part of spawning in the room; If visit is not part of spawning, 
        /// thrown if there is no link from current node to target, or if no strat from that link is provided.</exception>
        /// <exception cref="InvalidOperationException">If this is called while this InRoomState has no current room</exception>
        public InRoomState ApplyVisitNode(int nodeId, string stratName)
        {
            if(CurrentRoom == null)
            {
                throw new InvalidOperationException("Cannot try to visit a node by ID because there is no current room defined");
            }

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

            return ApplyVisitNode(node, strat);
        }

        /// <summary>
        /// Updates the in-room state by moving the player to the provided node.
        /// </summary>
        /// <param name="node">Node to visit.</param>
        /// <param name="strat">The strat through which the node is being reached. Can be null, but only for the first node visited in the room, 
        /// and for the second node only if Samus is seen as spawning there. Additionally, MUST be null for the first node visited in the room.
        /// If not null, must be present on a link that connects previous node to new node.</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ArgumentException">Thrown if a strat is provided when the node visit is part of spawning in the room; If visit is not part of spawning, 
        /// thrown if there is no link from current node to target, or if no strat from that link is provided.</exception>
        public InRoomState ApplyVisitNode(RoomNode node, Strat strat)
        {
            // Spawning in the room is ongoing if no nodes have been visited, or only one node has been visited and that node spawns Samus elsewhere
            bool spawnOngoing = !VisitedRoomPath.Any() || (VisitedRoomPath.Count == 1 && CurrentNode.SpawnsAtDifferentNode);

            // Only allow Strat to be null if spawn is ongoing
            if (strat == null && !spawnOngoing)
            {
                throw new ArgumentException("A strat must be provided when visiting a node except while spawning in the room.");
            }
            // Require Strat to be null if spawn is not ongoing
            else if (spawnOngoing && strat != null)
            {
                throw new ArgumentException("A strat must not be provided when visiting a node while spawning in the room.");
            }

            // If we're trying to visit the second node of a two-node spawning process, only the node referenced by the first node is valid
            if (spawnOngoing && VisitedRoomPath.Any() && CurrentNode.OverrideSpawnAtNode != node)
            {
                throw new ArgumentException($"Spawn is ongoing at node {CurrentNode.Id} of room '{CurrentRoom.Name}', and that node forces the next " +
                    $"node visit to be at node {CurrentNode.OverrideSpawnAtNodeId}");
            }

            if (strat != null)
            {
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
                        throw new ArgumentException($"The specified strat must be in a link from current node {CurrentNode.Id} to target node {node.Id} in room '{CurrentRoom.Name}'");
                    }
                }
            }

            InternalVisitedRoomPath.Add((new InNodeState(node), strat));

            return this;
        }

        public LinkTo GetLinkToNode(int targetNodeId)
        {
            LinkTo link;
            CurrentNode.Links.TryGetValue(targetNodeId, out link);
            return link;
        }

        public Strat GetStratToNode(int targetNodeId, string stratName)
        {
            Strat resultStrat = null;
            LinkTo link = GetLinkToNode(targetNodeId);
            link?.Strats.TryGetValue(stratName, out resultStrat);
            return resultStrat;
        }

        /// <summary>
        /// Updates the in-room state to contain a mention of the destruction of the obstacle in the current room with the provided ID.
        /// </summary>
        /// <param name="obstacleId">ID of the obstacle to destroy.</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ArgumentException">Thrown if the obstacle is not in the current room</exception>
        /// <exception cref="InvalidOperationException">Thrown if this state is not at a node (and hence has no room)</exception>
        public InRoomState ApplyDestroyObstacle(string obstacleId)
        {
            if (CurrentRoom == null)
            {
                throw new InvalidOperationException("Cannot destroy an obstacle when not in a room");
            }
            CurrentRoom.Obstacles.TryGetValue(obstacleId, out RoomObstacle obstacle);
            if(obstacle == null)
            {
                throw new ArgumentException($"Obstacle '{obstacleId}' not present in current room '{CurrentRoom.Name}'");
            }
            return ApplyDestroyObstacleSafe(obstacle);
        }

        /// <summary>
        /// Updates the in-room state to contain a mention of the destruction of the provided obstacle.
        /// </summary>
        /// <param name="obstacle">The obstacle to destroy.</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="ArgumentException">Thrown if the obstacle is not in the current room</exception>
        /// <exception cref="InvalidOperationException">Thrown if this state is not at a node (and hence has no room)</exception>
        public InRoomState ApplyDestroyObstacle(RoomObstacle obstacle)
        {
            if (CurrentRoom == null)
            {
                throw new InvalidOperationException("Cannot destroy an obstacle when not in a room");
            }
            CurrentRoom.Obstacles.TryGetValue(obstacle.Id, out RoomObstacle foundObstacle);
            if(foundObstacle != obstacle)
            {
                throw new ArgumentException("Provided obstacle must exist in current room");
            }
            return ApplyDestroyObstacleSafe(obstacle);
        }

        /// <summary>
        /// Does the actual updates of the in-room state to contain a mention of the destruction of the provided obstacle.
        /// This should only be called by one of the public ApplyDestroyObstacle() methods, after validating the operation.
        /// </summary>
        /// <param name="obstacle">The obstacle to destroy.</param>
        /// <returns>This, for chaining</returns>
        protected InRoomState ApplyDestroyObstacleSafe(RoomObstacle obstacle)
        {
            InternalDestroyedObstacleIds.Add(obstacle.Id);
            return this;
        }

        /// <summary>
        /// Registers the lock with the provided name as being opened at the current node.
        /// </summary>
        /// <param name="nodeLock">Lock being opened</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="InvalidOperationException">Thrown if this state is not at a node</exception>
        public InRoomState ApplyOpenLock(string lockName)
        {
            if(InternalCurrentNodeState == null)
            {
                throw new InvalidOperationException("Cannot open a lock when not at a node");
            }
            InternalCurrentNodeState.ApplyOpenLock(lockName);
            return this;
        }


        /// <summary>
        /// Registers the provided NodeLock as being opened at the current node.
        /// </summary>
        /// <param name="nodeLock">Lock being opened</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="InvalidOperationException">Thrown if this state is not at a node</exception>
        public InRoomState ApplyOpenLock(NodeLock nodeLock)
        {
            if (InternalCurrentNodeState == null)
            {
                throw new InvalidOperationException("Cannot open a lock when not at a node");
            }
            InternalCurrentNodeState.ApplyOpenLock(nodeLock);
            return this;
        }

        /// <summary>
        /// Registers the lock with the provided name as being bypassed at the current node.
        /// </summary>
        /// <param name="nodeLock">Lock being bypassed</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="InvalidOperationException">Thrown if this state is not at a node</exception>
        public InRoomState ApplyBypassLock(string lockName)
        {
            if (InternalCurrentNodeState == null)
            {
                throw new InvalidOperationException("Cannot bypass a lock when not at a node");
            }
            InternalCurrentNodeState.ApplyBypassLock(lockName);
            return this;
        }

        /// <summary>
        /// Registers the provided NodeLock as being bypassed at the current node.
        /// </summary>
        /// <param name="nodeLock">Lock being bypassed</param>
        /// <returns>This, for chaining</returns>
        /// <exception cref="InvalidOperationException">Thrown if this state is not at a node</exception>
        public InRoomState ApplyBypassLock(NodeLock nodeLock)
        {
            if (InternalCurrentNodeState == null)
            {
                throw new InvalidOperationException("Cannot bypass a lock when not at a node");
            }
            InternalCurrentNodeState.ApplyBypassLock(nodeLock);
            return this;
        }

        /// <summary>
        /// The locks opened by Samus at the last node visited in this room.
        /// </summary>
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

        /// <summary>
        /// The locks bypassed by Samus at the last node visited in this room.
        /// </summary>
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
        /// This will leave this InRoomState in a state where it is at no node.
        /// It's strongly recommended to visit a node after this before anything else.
        /// </summary>
        public void ClearRoomState()
        {
            InternalDestroyedObstacleIds.Clear();
            InternalVisitedRoomPath.Clear();
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
        /// List containing the nodes that have been visited in this room since entering, in order, starting with the node through which the room was entered.
        /// Note that this list may not stay in sync with future changes to this InRoomState.
        /// Each node ID is accompanied by the strat that was used to reach it, when applicable.
        /// This strat can be null for nodes visited during the process of spawning in the room (always the first node, and sometimes the second).
        /// </summary>
        public IReadOnlyList<(ReadOnlyInNodeState nodeState, Strat strat)> VisitedRoomPath { get; }

        /// <summary>
        /// A read-only set of IDs of obstacles that have been destroyed in this room since entering.
        /// </summary>
        public ReadOnlySet<string> DestroyedObstacleIds { get; }

        /// <summary>
        /// The strat that was used to reach the current node, if any. Otherwise, is null.
        /// </summary>
        public Strat LastStrat { get; }

        /// <summary>
        /// Tries to return a LinkTo from current node to provided targetNodeId. Returns null if not found.
        /// </summary>
        /// <param name="targetNodeId">Target node ID of the link to look for</param>
        /// <returns>The obtained LinkTo, or null if not found</returns>
        public LinkTo GetLinkToNode(int targetNodeId);

        /// <summary>
        /// Tries to find a link from current node to provided targetNodeId, then a strat on that link with provided stratName.
        /// Returns null if either portion fails.
        /// </summary>
        /// <param name="targetNodeId">Target node ID of the link to look for</param>
        /// <param name="stratName">Name of the strat to look on the link</param>
        /// <returns>The obtained Strat, or null</returns>
        public Strat GetStratToNode(int targetNodeId, string stratName);

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
