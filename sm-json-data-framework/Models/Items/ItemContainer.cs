using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Items
{
    /// <summary>
    /// <para>Contains the result of reading the items.json file.</para>
    /// <para>That file contains all items in the game, and the name of all game flags in the game.</para>
    /// <para>It also contains information about the default start situation: Starting room and node, initial items,
    /// initially open locks, and initially activated game flags.</para>
    /// </summary>
    public class ItemContainer
    {
        [JsonPropertyName("startingRoom")]
        public string StartingRoomName { get; set; }

        [JsonPropertyName("startingNode")]
        public int StartingNodeId { get; set; }

        [JsonPropertyName("startingItems")]
        public ISet<string> StartingItemNames { get; set; } = new HashSet<string>();

        [JsonPropertyName("startingFlags")]
        public ISet<string> StartingGameFlagNames { get; set; } = new HashSet<string>();

        [JsonPropertyName("startingLocks")]
        public ISet<string> StartingNodeLockNames { get; set; } = new HashSet<string>();

        public IList<ResourceCapacity> StartingResources { get; set; } = new List<ResourceCapacity>();

        [JsonPropertyName("implicitItems")]
        public ISet<string> ImplicitItemNames { get; set; } = new HashSet<string>();

        public IList<InGameItem> UpgradeItems { get; set; } = new List<InGameItem>();

        public IList<ExpansionItem> ExpansionItems { get; set; } = new List<ExpansionItem>();

        [JsonPropertyName("gameFlags")]
        public ISet<string> GameFlagNames { get; set; } = new HashSet<string>();
    }
}
