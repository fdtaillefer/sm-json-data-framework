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
        public IList<RawHelper> Helpers { get; set; } = new List<RawHelper>();
    }
}
