using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models
{
    /// <summary>
    /// A an abstract base class for all model elements in the <see cref="SuperMetroidModel"/> hierarchy.
    /// A notable feature of this is the ability to be altered by applying <see cref="LogicalOptions"/>.
    /// </summary>
    public abstract class AbstractModelElement
    {
        /// <summary>
        /// Indicates whether the <see cref="LogicalOptions"/> applied to this make it meaningless, or impossible to fulfill.
        /// This should likely default to false when no logical options are applied.
        /// </summary>
        public bool UselessByLogicalOptions { get; protected set; }

        /// <summary>
        /// The LogicalOptions that are currently applied to this model, if any. Null means no logical options are currently applied.
        /// </summary>
        public ReadOnlyLogicalOptions AppliedLogicalOptions { get; protected set; }

        /// <summary>
        /// <para>
        /// Applies alterations to this logical element, based on the provided LogicalOptions.
        /// </para>
        /// <para>
        /// Note that this will not be called for a LogicalOptions that is already altering this model.
        /// </para>
        /// <para>
        /// Concrete implementations of this method should:
        /// <list type="bullet">
        /// <item>Propagate this call to all owned sub-models</item>
        /// <item>Propagate this call to all non-owned sub-models whose altered state they need to rely on, and optionally any other</item>
        /// <item>Apply all alterations in an undoable, non-destructive way</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="logicalOptions">LogicalOptions on which to base alterations</param>
        /// <returns>True if this model is rendered useless by the logical options, false otherwise</returns>
        protected abstract bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions);

        /// <para>
        /// Applies alterations to this logical element, based on the provided LogicalOptions. 
        /// The goal of doing this is to preprocess things and avoid re-calculating them multiple times on the fly.
        /// This should not be called except as part of the applicationof logical options to an entire <see cref="SuperMetroidModel"/>.,
        /// as that would leave the model in an inconsistent state.
        /// </para>
        /// <param name="logicalOptions">LogicalOptions being applied</param>
        public void ApplyLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            if (logicalOptions != AppliedLogicalOptions)
            {
                AppliedLogicalOptions = logicalOptions;
                UselessByLogicalOptions = ApplyLogicalOptionsEffects(logicalOptions);
            }
        }
    }
}
