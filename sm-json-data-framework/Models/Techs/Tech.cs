using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Techs
{
    public class Tech : InitializablePostDeserializeOutOfRoom
    {
        public string Name { get; set; }

        public LogicalRequirements Requires { get; set; } = new LogicalRequirements();

        public IEnumerable<Tech> ExtensionTechs { get; set; } = Enumerable.Empty<Tech>();

        /// <summary>
        /// Returns a list containing this Tech and all its extension techs (and all their own extension techs, and so on).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tech> SelectWithExtensions()
        {
            return ExtensionTechs.SelectMany(tech => tech.SelectWithExtensions()).Prepend(this).ToList();
        }

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
            // Nothing relevant to cleanup

            // It's possible that a tech could be impossible to execute, but the Tech itself is still useful to validate that the tech exists
            return true;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model)
        {
            return Requires.InitializeReferencedLogicalElementProperties(model, null);
        }
    }
}
