using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Items
{
    public class RawItemContainer
    {
        public string StartingRoom { get; set; }

        public int StartingNode { get; set; }

        public IEnumerable<string> StartingItems { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<string> StartingFlags { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<string> StartingLocks { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<RawResourceCapacity> StartingResources { get; set; } = Enumerable.Empty<RawResourceCapacity>();

        public IEnumerable<string> ImplicitItems { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<RawInGameItem> UpgradeItems { get; set; } = Enumerable.Empty<RawInGameItem>();

        public IEnumerable<RawExpansionItem> ExpansionItems { get; set; } = Enumerable.Empty<RawExpansionItem>();

        public IEnumerable<string> GameFlags { get; set; } = Enumerable.Empty<string>();
    }
}
