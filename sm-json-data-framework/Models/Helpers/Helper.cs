using sm_json_data_framework.Models.Raw.Helpers;
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

        public Helper()
        {

        }

        /// <summary>
        /// A constructor to create the skeleton of a Helper based on a RawHelper.
        /// This will not initialize logical requirements, because there are logical requirements that are helpers themselves -
        /// so if a Helper is being created, the knowledge needed to convert logical requirements is still being built.
        /// Logical requirements should be assigned in a second pass.
        /// </summary>
        /// <param name="helper"></param>
        public Helper (RawHelper helper)
        {
            Name = helper.Name;
        }

        public void InitializeProperties(SuperMetroidModel model)
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
