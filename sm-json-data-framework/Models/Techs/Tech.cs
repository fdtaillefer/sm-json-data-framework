using sm_json_data_framework.Models.Raw.Techs;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Techs
{
    public class Tech : AbstractModelElement, InitializablePostDeserializeOutOfRoom
    {
        public string Name { get; set; }

        public LogicalRequirements Requires { get; set; } = new LogicalRequirements();

        public IList<Tech> ExtensionTechs { get; set; } = new List<Tech>();

        public Tech()
        {

        }

        /// <summary>
        /// A constructor to create the skeleton of a Tech based on a RawTech.
        /// This will not initialize logical requirements, because there are logical requirements that are techs themselves -
        /// so if a Tech is being created, the knowledge needed to convert logical requirements is still being built.
        /// Logical requirements should be assigned in a second pass.
        /// </summary>
        /// <param name="rawTech">RawTech to use as a base</param>
        public Tech(RawTech rawTech)
        {
            Name = rawTech.Name;
            ExtensionTechs = rawTech.ExtensionTechs.Select(subTech => new Tech(subTech)).ToList();
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            bool explicitlyDisabled = !logicalOptions.IsTechEnabled(this);
            // Propagate to requirements
            Requires.ApplyLogicalOptions(logicalOptions);
            // Assign UselessByLogicalOptions explicitly now, because extension techs requirements will depend on this tech's state.
            // Tech becomes useless if explicitly disabled or if it becomes impossible to execute
            UselessByLogicalOptions = explicitlyDisabled || Requires.UselessByLogicalOptions;

            // Propagate to extension techs
            foreach(Tech tech in ExtensionTechs) {
                tech.ApplyLogicalOptions(logicalOptions);
            }

            return UselessByLogicalOptions;
        }

        /// <summary>
        /// Returns a list containing this Tech and all its extension techs (and all their own extension techs, and so on).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tech> SelectWithExtensions()
        {
            return ExtensionTechs.SelectMany(tech => tech.SelectWithExtensions()).Prepend(this).ToList();
        }

        public void InitializeProperties(SuperMetroidModel model)
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
