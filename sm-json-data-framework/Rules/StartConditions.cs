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
        public RoomNode StartingNode { get; set; }

        private ItemInventory _itemInventory;
        public ItemInventory StartingInventory { 
            get { return _itemInventory; }
            set {
                _itemInventory = value;
                BaseResourceMaximums = value?.GetBaseResourceMaximumsClone();
            }
        }

        public ResourceCount BaseResourceMaximums { get; private set; }

        public ResourceCount StartingResources { get; set; }

        public IEnumerable<GameFlag> StartingGameFlags { get; set; } = Enumerable.Empty<GameFlag>();

        public IEnumerable<NodeLock> StartingOpenLocks { get; set; } = Enumerable.Empty<NodeLock>();

        public IEnumerable<RoomNode> StartingTakenItemLocations { get; set; } = Enumerable.Empty<RoomNode>();
    }
}
