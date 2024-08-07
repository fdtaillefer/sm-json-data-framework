﻿using sm_json_data_framework.Models.Raw.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Rules.InitialState
{
    /// <summary>
    /// A model describing the game's start conditions, using int and string foreign keys rather than constructed models.
    /// </summary>
    public class BasicStartConditions
    {
        public string StartingRoomName { get; set; }

        public int StartingNodeId { get; set; }

        public IEnumerable<string> StartingFlagNames { get; set; }

        public IEnumerable<string> StartingLockNames { get; set; }

        public IEnumerable<string> StartingItemNames { get; set; }

        public IEnumerable<RawResourceCapacity> StartingResources { get; set; }

        public BasicStartConditions(RawItemContainer rawItemContainer)
        {
            StartingRoomName = rawItemContainer.StartingRoom;
            StartingNodeId = rawItemContainer.StartingNode;
            StartingFlagNames = new List<string>(rawItemContainer.StartingFlags);
            StartingLockNames = new List<string>(rawItemContainer.StartingLocks);
            StartingItemNames = new List<string>(rawItemContainer.StartingItems);
            StartingResources = new List<RawResourceCapacity>(rawItemContainer.StartingResources);
        }
    }
}
