using sm_json_data_framework.Models.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Connections
{
    /// <summary>
    /// Corresponds to a (potentially) two-way "Connection" object in the json model.
    /// </summary>
    public class RawConnection
    {
        public ConnectionTypeEnum ConnectionType { get; set; }

        public string Description { get; set; }

        public IEnumerable<RawConnectionNode> Nodes { get; set; } = Enumerable.Empty<RawConnectionNode>();

        public ConnectionDirectionEnum Direction { get; set; } = ConnectionDirectionEnum.Bidirectional;
    }
}
