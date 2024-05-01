using sm_json_data_framework.Models.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Connections
{
    public class RawConnectionContainer
    {
        public IList<RawConnection> Connections { get; set; }
    }
}
