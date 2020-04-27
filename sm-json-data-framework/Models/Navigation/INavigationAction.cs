using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{
    /// <summary>
    /// An interface for actions that can be performed by a <see cref="GameNavigator"/>.
    /// </summary>
    public interface INavigationAction
    {
        /// <summary>
        /// Indicates whether this action was a success. A failed attempt is fully ignored by a GameNavigator and doesn't actually change the state.
        /// </summary>
        bool Succeeded { get; }

        /// <summary>
        /// If the action changed the player's position, this property describes it. Otherwise, this is null.
        /// </summary>
        (RoomNode fromNode, RoomNode toNode) PositionChange { get; }

        /// <summary>
        /// The enumeration of items that have been gained by the player as a result of this action.
        /// </summary>
        IEnumerable<Item> ItemsGained { get; }

        /// <summary>
        /// The enumeration of items that have been lost by the player as a result of this action.
        /// This can only really happen by reversing an action.
        /// </summary>
        IEnumerable<Item> ItemsLost { get; }

        /// <summary>
        /// The enumeration of game flags that have been obtained by the player as a result of this action.
        /// </summary>
        IEnumerable<GameFlag> GameFlagsGained { get; }

        /// <summary>
        /// The enumeration of game flags that have been lost by the player as a result of this action.
        /// This can only really happen by reversing an action.
        /// </summary>
        IEnumerable<GameFlag> GameFlagsLost { get; }

        /// <summary>
        /// The enumeration of node  locks that have been opened as a result of this action.
        /// </summary>
        IEnumerable<NodeLock> LocksOpened { get; }

        /// <summary>
        /// The enumeration of node  locks that have been closed as a result of this action.
        /// This can only really happen by reversing an action.
        /// </summary>
        IEnumerable<NodeLock> LocksClosed { get; }

        /// <summary>
        /// The enumeration of in-room obstacles that have been destroyed as a result of this action.
        /// </summary>
        IEnumerable<RoomObstacle> ObstaclesDestroyed { get; }

        /// <summary>
        /// The enumeration of in-room obstacles that have been restored as a result of this action.
        /// This can only really happen by reversing an action, since it's deemed unnecessary to indicate that exiting a room restores obstacles.
        /// </summary>
        IEnumerable<RoomObstacle> ObstaclesRestored { get; }

        /// <summary>
        /// The enumeration of resources that have been gained or lost as a result of this action.
        /// Quantities will be negative for lost resources.
        /// </summary>
        IEnumerable<(RechargeableResourceEnum resource, int quantityChange)> ResourcesChanged { get; }

        /// <summary>
        /// Creates and returns an action representing the reverse of this action.
        /// </summary>
        /// <returns></returns>
        INavigationAction Reverse();
    }
}
