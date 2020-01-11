using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Items
{
    public class ItemContainer
    {

        [JsonPropertyName("baseItems")]
        public IEnumerable<string> BaseItemNames { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<BaseResource> BaseResources { get; set; } = Enumerable.Empty<BaseResource>();

        public IEnumerable<InGameItem> UpgradeItems { get; set; } = Enumerable.Empty<InGameItem>();

        public IEnumerable<ExpansionItem> ExpansionItems { get; set; } = Enumerable.Empty<ExpansionItem>();

        [JsonPropertyName("gameFlags")]
        public IEnumerable<string> GameFlagNames { get; set; } = Enumerable.Empty<string>();
    }
}
