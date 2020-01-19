using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// Represents the logically-relevant parts of the state of the current room.
    /// </summary>
    public class InRoomState
    {

        public InRoomState(RoomNode initialNode)
        {
            EnterRoom(initialNode);
        }
        
        public InRoomState(InRoomState other)
        {
            CurrentNode = other.CurrentNode;
            VisitedNodeIds = new List<int>(other.VisitedNodeIds);
            DestroyedObstacleIds = new HashSet<string>(other.DestroyedObstacleIds);
        }

        /// <summary>
        /// The node the player is currently at. This can be null if in-room state isn't being tracked.
        /// </summary>
        public RoomNode CurrentNode { get; protected set; }

        /// <summary>
        /// The room the player is currently in. This can be null if in-room state isn't being tracked.
        /// </summary>
        public Room CurrentRoom { get => CurrentNode?.Room; }

        /// <summary>
        ///  The ID of nodes that have been visited in this room since entering, in order, starting with the node through which the room was entered.
        /// </summary>
        protected List<int> VisitedNodeIds { get; set; } = new List<int>();

        /// <summary>
        /// The ID of obstacles that have been destroyed in this room since entering.
        /// </summary>
        protected HashSet<string> DestroyedObstacleIds { get; set; } = new HashSet<string>();

        /// <summary>
        /// Sets this InRoomState's state to that of immediate entry of a room via the provided entry node.
        /// This may actually place the player at a different node if the node calls for it.
        /// </summary>
        /// <param name="entryNode">The node by which the room is being entered.</param>
        public void EnterRoom(RoomNode entryNode)
        {
            DestroyedObstacleIds.Clear();
            VisitedNodeIds.Clear();

            if(entryNode == null)
            {
                CurrentNode = null;
            }
            else
            {
                VisitNode(entryNode);
                if(entryNode.SpawnAtNode != null)
                {
                    VisitNode(entryNode.SpawnAtNode);
                }
            }
        }

        /// <summary>
        /// Updates the in-room state by moving the player to the provided node. Should not be called for a node that is not in the current room.
        /// </summary>
        /// <param name="node">Node to visit.</param>
        public void VisitNode(RoomNode node)
        {
            CurrentNode = node;
            VisitedNodeIds.Add(node.Id);
        }

        /// <summary>
        /// Updates the in-room state to contain a mention of the destruction of the provided obstacle.
        /// Should not be called for an obstacle that is not in the current room.
        /// </summary>
        /// <param name="obstacle">The obstacle to destroy.</param>
        public void DestroyObstacle(RoomObstacle obstacle)
        {
            DestroyedObstacleIds.Add(obstacle.Id);
        }

        /// <summary>
        /// Removes all data from this InRoomState. Useful if this has been initialized at a starting node but in-room state is not going to be maintained.
        /// </summary>
        public void ClearRoomState()
        {
            EnterRoom(null);
        }
    }
}
