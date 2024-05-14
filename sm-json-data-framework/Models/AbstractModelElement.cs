using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models
{
    /// <summary>
    /// A an abstract base class for all model elements in the <see cref="UnfinalizedSuperMetroidModel"/> hierarchy.
    /// A notable feature of this is the ability to be altered by applying <see cref="LogicalOptions"/>.
    /// </summary>
    /// <typeparam name="SourceType">The unfinalized type that finalizes into this type</typeparam>
    /// <typeparam name="ConcreteType">The self-type of the concrete sub-type</typeparam>
    public abstract class AbstractModelElement<SourceType, ConcreteType>: IModelElement
        where ConcreteType : AbstractModelElement<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedModelElement<SourceType, ConcreteType>
    {
        private SourceType InnerElement { get; set; }

        protected AbstractModelElement(SourceType innerElement, Action<ConcreteType> mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            mappingsInsertionCallback.Invoke((ConcreteType)this);
        }

        public bool UselessByLogicalOptions { get { return InnerElement.UselessByLogicalOptions; } }

        public ReadOnlyLogicalOptions AppliedLogicalOptions { get { return InnerElement.AppliedLogicalOptions; } }

        public void ApplyLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            InnerElement.ApplyLogicalOptions(logicalOptions);
        }
    }

    /// <summary>
    /// The untyped interface portion of <see cref="AbstractModelElement{ConcreteType, TargetType}"/>.
    /// </summary>
    public interface IModelElement : IConfigurableByLogicalOptions
    {

    }

    /// <summary>
    /// A an abstract base class for all unfinalized model elements in the <see cref="UnfinalizedSuperMetroidModel"/> hierarchy.
    /// A notable feature of this is the ability to be altered by applying <see cref="LogicalOptions"/>.
    /// </summary>
    public abstract class AbstractUnfinalizedModelElement<ConcreteType, TargetType> : IUnfinalizedModelElement
        where ConcreteType: AbstractUnfinalizedModelElement<ConcreteType, TargetType>
        where TargetType: AbstractModelElement<ConcreteType, TargetType>, IModelElement
    {
        /// <summary>
        /// Creates and returns a finalized version of this abstract logical element.
        /// If a finalized version already exists in the provided mappings, that will be returned instead.
        /// </summary>
        /// <param name="mappings">A model containing mappings between unfinalized instances and corresponding finalized instances</param>
        /// <returns></returns>
        public virtual TargetType Finalize(ModelFinalizationMappings mappings)
        {
            var constructorDelegate = CreateFinalizedElement;
            return mappings.GetFinalizedModel((ConcreteType)this, constructorDelegate);
        }

        public IModelElement FinalizeUntyped(ModelFinalizationMappings mappings)
        {
            return Finalize(mappings);
        }

        /// <summary>
        /// A delegate to the constructor of the concrete finalized type for this concrete unfinalized type.
        /// </summary>
        /// <param name="sourceElement">The concrete source element, the instance passed will always be this</param>
        /// <param name="mappingsInsertionCallback">A callback that should be passed to the constructor io insert the new instance in the mappings
        /// before concrete subconstructor code runs. This prevents infinite loops.</param>
        /// <param name="mappings">Mappings of unfinalized-to-finalized model element instances, which can be used as a reference to obtain finalized sub-models</param>
        /// <returns></returns>
        protected abstract TargetType CreateFinalizedElement(ConcreteType sourceElement, Action<TargetType> mappingsInsertionCallback, ModelFinalizationMappings mappings);

        public bool UselessByLogicalOptions { get; protected set; }

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

        public void ApplyLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            if (logicalOptions != AppliedLogicalOptions)
            {
                AppliedLogicalOptions = logicalOptions;
                UselessByLogicalOptions = ApplyLogicalOptionsEffects(logicalOptions);
            }
        }
    }

    /// <summary>
    /// The untyped interface portion of <see cref="AbstractUnfinalizedModelElement{ConcreteType, TargetType}"/>.
    /// </summary>
    public interface IUnfinalizedModelElement: IConfigurableByLogicalOptions
    {
        /// <summary>
        /// Does the same as <see cref="AbstractUnfinalizedModelElement{ConcreteType, TargetType}.Finalize(ModelFinalizationMappings)"/>, 
        /// but with an untyped signature.
        /// </summary>
        /// <param name="mappings">A model containing mappings between unfinalized instances and corresponding finalized instances</param>
        /// <returns></returns>
        public IModelElement FinalizeUntyped(ModelFinalizationMappings mappings);
    }

    /// <summary>
    /// Interface indicating that a model can have <see cref="ReadOnlyLogicalOptions"/> applied to it (in a non-destructive way),
    /// potentially rendering it useless while the options are applied.
    /// </summary>
    public interface IConfigurableByLogicalOptions
    {
        /// <summary>
        /// Applies alterations to this object, based on the provided ReadOnlyLogicalOptions. 
        /// The goal of doing this is to preprocess things and avoid re-calculating them multiple times on the fly.
        /// This should not be called except as part of the application of logical options to an entire <see cref="UnfinalizedSuperMetroidModel"/>.,
        /// as that could leave the model in an inconsistent state.
        /// </summary>
        /// <param name="logicalOptions">LogicalOptions being applied</param>
        public void ApplyLogicalOptions(ReadOnlyLogicalOptions logicalOptions);

        /// <summary>
        /// Indicates whether the <see cref="ReadOnlyLogicalOptions"/> applied to this make it meaningless, or impossible to fulfill.
        /// This should likely default to false when no logical options are applied.
        /// </summary>
        public bool UselessByLogicalOptions { get; }

        /// <summary>
        /// The LogicalOptions that are currently applied to this model, if any. Null means no logical options are currently applied.
        /// </summary>
        public ReadOnlyLogicalOptions AppliedLogicalOptions { get; }
    }
}
