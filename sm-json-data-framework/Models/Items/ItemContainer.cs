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
        public IEnumerable<string> StartingItemNames { get; set; } = Enumerable.Empty<string>();

        [JsonPropertyName("startingFlags")]
        public IEnumerable<string> StartingGameFlagNames { get; set; } = Enumerable.Empty<string>();

        [JsonPropertyName("startingLocks")]
        public IEnumerable<string> StartingNodeLockNames { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<ResourceCapacity> StartingResources { get; set; } = Enumerable.Empty<ResourceCapacity>();

        [JsonPropertyName("implicitItems")]
        public IEnumerable<string> ImplicitItemNames { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<InGameItem> UpgradeItems { get; set; } = Enumerable.Empty<InGameItem>();

        public IEnumerable<ExpansionItem> ExpansionItems { get; set; } = Enumerable.Empty<ExpansionItem>();

        [JsonPropertyName("gameFlags")]
        public IEnumerable<string> GameFlagNames { get; set; } = Enumerable.Empty<string>();
    }
}
