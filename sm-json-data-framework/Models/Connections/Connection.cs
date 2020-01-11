using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Connections
{
    public class Connection
    {
        public ConnectionTypeEnum ConnectionType { get; set; }

        public string Description { get; set; }

        public IEnumerable<ConnectionNode> Nodes { get; set; } = Enumerable.Empty<ConnectionNode>();
    }
}
