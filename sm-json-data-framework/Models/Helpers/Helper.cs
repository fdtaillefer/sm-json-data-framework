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

        public void InitializeForeignProperties(SuperMetroidModel model)
        {
            // Nothing relevant to initialize
        }

        public void InitializeOtherProperties(SuperMetroidModel model)
        {
            // Nothing relevant to initialize
        }


        public bool CleanUpUselessValues(SuperMetroidModel model)
        {
            // Nothing relevant to clean up

            // A helper, even with requirements that can never be executed, is still useful in the sense that it confirms
            // that a corresponding HelperLogicalElement is a valid reference.
            return true;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model)
        {
            return Requires.InitializeReferencedLogicalElementProperties(model, null);
        }
    }
}
