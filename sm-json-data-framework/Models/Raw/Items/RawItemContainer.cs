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

        public ISet<string> StartingItems { get; set; } = new HashSet<string>();

        public ISet<string> StartingFlags { get; set; } = new HashSet<string>();

        public ISet<string> StartingLocks { get; set; } = new HashSet<string>();

        public IList<RawResourceCapacity> StartingResources { get; set; } = new List<RawResourceCapacity>();

        public ISet<string> ImplicitItems { get; set; } = new HashSet<string>();

        public IList<RawInGameItem> UpgradeItems { get; set; } = new List<RawInGameItem>();

        public IList<RawExpansionItem> ExpansionItems { get; set; } = new List<RawExpansionItem>();

        public ISet<string> GameFlags { get; set; } = new HashSet<string>();
    }
}
