using sm_json_data_framework.Models.Raw.Helpers;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
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
        private UnfinalizedHelper InnerElement { get; set; }

        public Helper(UnfinalizedHelper innerElement, Action<Helper> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Requires = innerElement.Requires.Finalize(mappings);
        }

        /// <summary>
        /// A unique name that identifies this helper.
        /// </summary>
        public string Name { get { return InnerElement.Name;  } }

        /// <summary>
        /// The logical requirements that this helper represents.
        /// </summary>
        public LogicalRequirements Requires { get; }
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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Propagate to requirements
            Requires.ApplyLogicalOptions(logicalOptions);

            // This helper becomes useless if it becomes impossible
            return Requires.UselessByLogicalOptions;
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
