using sm_json_data_framework.Models.Raw.Techs;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
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
    public class Tech : AbstractModelElement<UnfinalizedTech, Tech>, ILogicalExecutionPreProcessable
    {
        public Tech(UnfinalizedTech sourceElement, Action<Tech> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Name = sourceElement.Name;
            Requires = sourceElement.Requires.Finalize(mappings);
            ExtensionTechs = sourceElement.ExtensionTechs.Select(tech => tech.Finalize(mappings)).ToDictionary(tech => tech.Name).AsReadOnly();
        }

        /// <summary>
        /// A unique name that identifies this Tech.
        /// </summary>
        public string Name { get; }

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

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Propagate to requirements
            Requires.ApplyLogicalOptions(logicalOptions, rules);

            // Don't propagate to extension techs. They don't "belong" to this, and they will depend on the logical state of this to be up-to-date
            // in order to update their own logical state, so we don't want to be in this halfway state when that happens.
            // This all assumes that no Tech can ever depend on itself by any circular logic.
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // A tech that can't be executed may as well not exist
            return !CalculateLogicallyNever(rules);
        }

        protected override void UpdateLogicalProperties(SuperMetroidRules rules)
        {
            base.UpdateLogicalProperties(rules);
            LogicallyNever = CalculateLogicallyNever(rules);
            LogicallyAlways = CalculateLogicallyAlways(rules);
            LogicallyFree = CalculateLogicallyFree(rules);
        }

        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // Tech is impossible if it's disabled or if its requirements are impossible
            return !AppliedLogicalOptions.IsTechEnabled(this) || Requires.LogicallyNever;
        }

        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            // A Tech is always possible it's enabled and its requirements are always possible
            return AppliedLogicalOptions.IsTechEnabled(this) && Requires.LogicallyAlways;
        }

        public bool LogicallyFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyFree"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // A Tech is always free it's enabled and its requirements are free
            return AppliedLogicalOptions.IsTechEnabled(this) && Requires.LogicallyFree;
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
