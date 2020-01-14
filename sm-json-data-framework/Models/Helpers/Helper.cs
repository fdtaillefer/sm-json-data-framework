using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Helpers
{
    public class Helper : InitializablePostDeserializeOutOfRoom
    {
        public string Name { get; set; }

        public LogicalRequirements Requires { get; set; } = new LogicalRequirements();

        public void Initialize(SuperMetroidModel model)
        {
            // Nothing relevant to initialize
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model)
        {
            return Requires.InitializeReferencedLogicalElementProperties(model, null);
        }
    }
}
