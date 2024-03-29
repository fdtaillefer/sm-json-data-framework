using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Rules
{
    /// <summary>
    /// A container for game start configuration.
    /// </summary>
    public class StartConditions
    {
        public StartConditions()
        {

        }

        public StartConditions(StartConditions other)
        {
            StartingGameFlags = new List<GameFlag>(other.StartingGameFlags);
            StartingInventory = other.StartingInventory?.Clone();
            StartingNode = other.StartingNode;
            StartingOpenLocks = new List<NodeLock>(other.StartingOpenLocks);
            StartingResources = other.StartingResources?.Clone();
            StartingTakenItemLocations = new List<RoomNode>(other.StartingTakenItemLocations);
        }

        public StartConditions Clone()
        {
            return new StartConditions(this);
        }

        public RoomNode StartingNode { get; set; }

        private ItemInventory _itemInventory;
        public ItemInventory StartingInventory { get; set; }

        public ReadOnlyResourceCount BaseResourceMaximums { get { return StartingInventory?.BaseResourceMaximums; } }

        public ResourceCount StartingResources { get; set; }

        public IEnumerable<GameFlag> StartingGameFlags { get; set; } = Enumerable.Empty<GameFlag>();

        public IEnumerable<NodeLock> StartingOpenLocks { get; set; } = Enumerable.Empty<NodeLock>();

        public IEnumerable<RoomNode> StartingTakenItemLocations { get; set; } = Enumerable.Empty<RoomNode>();
    }
}
