using sm_json_data_framework.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Helpers
{
    public class RawHelperContainer
    {
        public IEnumerable<RawHelper> Helpers { get; set; } = Enumerable.Empty<RawHelper>();
    }
}
