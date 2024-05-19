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
            ExtensionTechs = InnerElement.ExtensionTechs.Select(tech => tech.Finalize(mappings)).ToDictionary(tech => tech.Name).AsReadOnly();
        }

        /// <summary>
        /// A unique name that identifies this Tech.
        /// </summary>
        public string Name => InnerElement.Name;

        /// <summary>
        /// Logical requirements that must be fulfilled to execute this Tech.
        /// </summary>
        public LogicalRequirements Requires { get; }

        /// <summary>
        /// The techs that are more complex or specific variations of this tech, mapped by name.
        /// </summary>
        public IReadOnlyDictionary<string, Tech> ExtensionTechs { get; }

        /// <summary>
        /// Returns a list containing this Tech and all its extension techs (and all their own extension techs, and so on).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tech> SelectWithExtensions()
        {
            return ExtensionTechs.SelectMany(tech => tech.Value.SelectWithExtensions()).Prepend(this).ToList();
        }

        protected override bool PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            bool explicitlyDisabled = !logicalOptions.IsTechEnabled(this);
            // Propagate to requirements
            Requires.ApplyLogicalOptions(logicalOptions);

            // Don't propagate to extension techs. They don't "belong" to this, and they will depend on the logical state of this to be up-to-date
            // in order to update their own logical state, so we don't want to be in this halfway state when that happens.
            // This all assumes that no Tech can ever depend on itself by any circular logic.

            return UselessByLogicalOptions;
        }

        protected override void UpdateLogicalProperties()
        {
            base.UpdateLogicalProperties();
            LogicallyNever = CalculateLogicallyNever();
        }

        /// <summary>
        /// If true, then this tech is impossible to execute.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyNever()
        {
            // Tech is impossible if it's disabled or if its requirements are impossible
            return !AppliedLogicalOptions.IsTechEnabled(this) || Requires.LogicallyNever;
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

        /// <summary>
        /// Returns a list containing this Tech and all its extension techs (and all their own extension techs, and so on).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UnfinalizedTech> SelectWithExtensions()
        {
            return ExtensionTechs.SelectMany(tech => tech.SelectWithExtensions()).Prepend(this).ToList();
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model)
        {
            // Nothing relevant to initialize
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model)
        {
            return Requires.InitializeReferencedLogicalElementProperties(model, null);
        }
    }
}
