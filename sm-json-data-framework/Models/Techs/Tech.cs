using sm_json_data_framework.Models.Raw.Techs;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Techs
{
    /// <summary>
    /// Represents a technique that a player may learn to execute. 
    /// Techs can have logical requirements involving items, ammo, or other techs, and may be turned off logically.
    /// </summary>
    public class Tech : AbstractModelElement<UnfinalizedTech, Tech>
    {
        private UnfinalizedTech InnerElement { get; set; }

        public Tech(UnfinalizedTech innerElement, Action<Tech> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Requires = InnerElement.Requires.Finalize(mappings);
            ExtensionTechs = InnerElement.ExtensionTechs.Select(tech => tech.Finalize(mappings)).ToList().AsReadOnly();
        }

        /// <summary>
        /// A unique name that identifies this Tech.
        /// </summary>
        public string Name { get { return InnerElement.Name; } }

        /// <summary>
        /// Logical requirements that must be fulfilled to execute this Tech.
        /// </summary>
        public LogicalRequirements Requires { get; }

        /// <summary>
        /// A list of techs that are more complex or specific variations of this tech.
        /// </summary>
        public IReadOnlyList<Tech> ExtensionTechs { get; }

        /// <summary>
        /// Returns a list containing this Tech and all its extension techs (and all their own extension techs, and so on).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tech> SelectWithExtensions()
        {
            return ExtensionTechs.SelectMany(tech => tech.SelectWithExtensions()).Prepend(this).ToList();
        }
    }

    public class UnfinalizedTech : AbstractUnfinalizedModelElement<UnfinalizedTech, Tech>, InitializablePostDeserializeOutOfRoom
    {
        public string Name { get; set; }

        public UnfinalizedLogicalRequirements Requires { get; set; } = new UnfinalizedLogicalRequirements();

        public IList<UnfinalizedTech> ExtensionTechs { get; set; } = new List<UnfinalizedTech>();

        public UnfinalizedTech()
        {

        }

        /// <summary>
        /// A constructor to create the skeleton of a Tech based on a RawTech.
        /// This will not initialize logical requirements, because there are logical requirements that are techs themselves -
        /// so if a Tech is being created, the knowledge needed to convert logical requirements is still being built.
        /// Logical requirements should be assigned in a second pass.
        /// </summary>
        /// <param name="rawTech">RawTech to use as a base</param>
        public UnfinalizedTech(RawTech rawTech)
        {
            Name = rawTech.Name;
            ExtensionTechs = rawTech.ExtensionTechs.Select(subTech => new UnfinalizedTech(subTech)).ToList();
        }

        protected override Tech CreateFinalizedElement(UnfinalizedTech sourceElement, Action<Tech> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Tech(sourceElement, mappingsInsertionCallback, mappings);
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
            foreach(UnfinalizedTech tech in ExtensionTechs) {
                tech.ApplyLogicalOptions(logicalOptions);
            }

            return UselessByLogicalOptions;
        }

        /// <summary>
        /// Returns a list containing this Tech and all its extension techs (and all their own extension techs, and so on).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UnfinalizedTech> SelectWithExtensions()
        {
            return ExtensionTechs.SelectMany(tech => tech.SelectWithExtensions()).Prepend(this).ToList();
        }

        public void InitializeProperties(SuperMetroidModel model)
        {
            // Nothing relevant to initialize
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model)
        {
            return Requires.InitializeReferencedLogicalElementProperties(model, null);
        }
    }
}
