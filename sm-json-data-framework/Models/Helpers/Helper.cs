using sm_json_data_framework.Models.Raw.Helpers;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Helpers
{
    /// <summary>
    /// A helper is a container for a common set of logical requirements, which can be referenced many times across the model.
    /// </summary>
    public class Helper : AbstractModelElement<UnfinalizedHelper, Helper>
    {
        public Helper(UnfinalizedHelper sourceElement, Action<Helper> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Name = sourceElement.Name;
            Requires = sourceElement.Requires.Finalize(mappings);
        }

        /// <summary>
        /// A unique name that identifies this helper.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The logical requirements that this helper represents.
        /// </summary>
        public LogicalRequirements Requires { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Propagate to requirements
            Requires.ApplyLogicalOptions(logicalOptions, rules);
        }

        protected override void UpdateLogicalProperties(SuperMetroidRules rules)
        {
            base.UpdateLogicalProperties(rules);
            LogicallyNever = CalculateLogicallyNever(rules);
            LogicallyAlways = CalculateLogicallyAlways(rules);
            LogicallyFree = CalculateLogicallyFree(rules);
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // A helper that can't be executed may as well not exist
            return !CalculateLogicallyNever(rules);
        }

        /// <summary>
        /// If true, then this helper is impossible to execute given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyNever"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // Helper is impossible if its requirements also are.
            return Requires.LogicallyNever;
        }

        /// <summary>
        /// If true, then this Helper is always possible to execute given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            return Requires.LogicallyAlways;
        }

        /// <summary>
        /// If true, not only can this helper always be executed given the current logical options, regardless of in-game state,
        /// but that fulfillment is also guaranteed to cost no resources.
        /// </summary>
        public bool LogicallyFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyFree"/> should currently be.
        /// </summary>
        /// <param name="rules">The active SuperMetroidRules, provided so they're available for consultation</param>
        /// <returns></returns>
        protected bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            // A helper is always free if its requirements are free
            return Requires.LogicallyFree;
        }
    }

    public class UnfinalizedHelper : AbstractUnfinalizedModelElement<UnfinalizedHelper, Helper>, InitializablePostDeserializeOutOfRoom
    {
        public string Name { get; set; }

        public UnfinalizedLogicalRequirements Requires { get; set; } = new UnfinalizedLogicalRequirements();

        public UnfinalizedHelper()
        {

        }

        /// <summary>
        /// A constructor to create the skeleton of a Helper based on a RawHelper.
        /// This will not initialize logical requirements, because there are logical requirements that are helpers themselves -
        /// so if a Helper is being created, the knowledge needed to convert logical requirements is still being built.
        /// Logical requirements should be assigned in a second pass.
        /// </summary>
        /// <param name="helper">RawHelper to use as a base</param>
        public UnfinalizedHelper (RawHelper helper)
        {
            Name = helper.Name;
        }

        protected override Helper CreateFinalizedElement(UnfinalizedHelper sourceElement, Action<Helper> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Helper(sourceElement, mappingsInsertionCallback, mappings);
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
