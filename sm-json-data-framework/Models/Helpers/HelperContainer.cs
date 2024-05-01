using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Helpers
{
    public class HelperContainer
    {
        public IList<Helper> Helpers { get; set; } = new List<Helper>();
    }
}
