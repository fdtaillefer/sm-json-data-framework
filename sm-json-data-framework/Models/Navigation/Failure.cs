using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{

    /// <summary>
    /// A navigation action that ends in failure. What was attempted is not known.
    /// </summary>
    public class Failure : INavigationAction
    {
        public static readonly Failure Instance = new Failure();

        private Failure()
        {
            Succeeded = false;
            ItemsGained = Enumerable.Empty<Item>();
            ItemsLost= Enumerable.Empty<Item>();
            GameFlagsGained = Enumerable.Empty<GameFlag>();
            GameFlagsLost = Enumerable.Empty<GameFlag>();
            LocksOpened = Enumerable.Empty<NodeLock>();
            LocksClosed = Enumerable.Empty<NodeLock>();
            ObstaclesDestroyed = Enumerable.Empty<RoomObstacle>();
            ObstaclesRestored = Enumerable.Empty<RoomObstacle>();
            ResourcesChanged = Enumerable.Empty<(RechargeableResourceEnum resource, int quantityChange)>();
        }

        public bool Succeeded { get; private set;}

        public (RoomNode fromNode, RoomNode toNode) PositionChange { get; private set; }

        public IEnumerable<Item> ItemsGained { get; private set; }

        public IEnumerable<Item> ItemsLost { get; private set; }

        public IEnumerable<GameFlag> GameFlagsGained { get; private set; }

        public IEnumerable<GameFlag> GameFlagsLost { get; private set; }

        public IEnumerable<NodeLock> LocksOpened { get; private set; }

        public IEnumerable<NodeLock> LocksClosed { get; private set; }

        public IEnumerable<RoomObstacle> ObstaclesDestroyed { get; private set; }

        public IEnumerable<RoomObstacle> ObstaclesRestored { get; private set; }

        public IEnumerable<(RechargeableResourceEnum resource, int quantityChange)> ResourcesChanged { get; private set; }

        public INavigationAction Reverse()
        {
            // Failures have no reverse, they just fail.
            return this;
        }
    }
}
