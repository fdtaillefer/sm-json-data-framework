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
    /// <typeparam name="SourceType">The unfinalized type that finalizes into this type</typeparam>
    /// <typeparam name="ConcreteType">The self-type of the concrete sub-type</typeparam>
    public abstract class AbstractModelElement<SourceType, ConcreteType>: IModelElement
        where ConcreteType : AbstractModelElement<SourceType, ConcreteType>
        where SourceType : AbstractUnfinalizedModelElement<SourceType, ConcreteType>
    {
        protected AbstractModelElement()
        {

        }

        protected AbstractModelElement(SourceType innerElement, Action<ConcreteType> mappingsInsertionCallback)
        {
            mappingsInsertionCallback.Invoke((ConcreteType)this);
        }

        public ReadOnlyLogicalOptions AppliedLogicalOptions { get; protected set; }

        /// <summary>
        /// <para>
        /// Propagates the application of a LogicalOptions instance to other models.
        /// </para>
        /// <para>
        /// Note that this will not be called for a LogicalOptions instance that is already altering this model.
        /// </para>
        /// <para>
        /// Concrete implementations of this method should call <see cref="ApplyLogicalOptions(ReadOnlyLogicalOptions)"/> on:
        /// <list type="bullet">
        /// <item>All owned sub-models</item>
        /// <item>All non-owned sub-models whose logically-updated behavior they need to rely in order to properly apply logical options on themselves,
        /// or to calculate logial properties</item>
        /// <item>Optionally any other  model (it's not needed but not harmful)</item>
        /// </list>
        /// </para>
        /// <para>
        /// It is important for an element to NOT propagate to elements that depend on their own logical state for their logical properties, 
        /// because then they may end up relying on that element while its logical state is not fully updated.
        /// </para>
        /// </summary>
        /// <param name="logicalOptions">LogicalOptions being applied</param>
        protected abstract void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions);

        public void ApplyLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            if (logicalOptions != AppliedLogicalOptions)
            {
                AppliedLogicalOptions = logicalOptions;
                PropagateLogicalOptions(logicalOptions);
                UpdateLogicalProperties();
            }
        }

        /// <summary>
        /// Updates any logical property of this model, as a result of logical options being applied.
        /// Any override of this method should call the base implementation first.
        /// </summary>
        protected virtual void UpdateLogicalProperties()
        {
            LogicallyRelevant = CalculateLogicallyRelevant();
        }

        public bool LogicallyRelevant { get; private set; }

        /// <summary>
        /// Calculates and returns what the value of <see cref="LogicallyRelevant"/> should be.
        /// </summary>
        /// <returns></returns>
        public abstract bool CalculateLogicallyRelevant();
    }

    /// <summary>
    /// The untyped interface portion of <see cref="AbstractModelElement{ConcreteType, TargetType}"/>.
    /// Notably, a model that implements this interface can have <see cref="ReadOnlyLogicalOptions"/> applied to it (in a non-destructive way),
    /// potentially rendering it useless while the options are applied.
    /// </summary>
    public interface IModelElement
    {
        /// <summary>
        /// Applies alterations to this object, based on the provided ReadOnlyLogicalOptions. 
        /// The goal of doing this is to preprocess things and avoid re-calculating them multiple times on the fly.
        /// This should not be called except as part of the application of logical options to an entire <see cref="SuperMetroidModel"/>.,
        /// as that could leave the model in an inconsistent state.
        /// </summary>
        /// <param name="logicalOptions">LogicalOptions being applied</param>
        public void ApplyLogicalOptions(ReadOnlyLogicalOptions logicalOptions);

        /// <summary>
        /// <para>
        /// The LogicalOptions that are currently applied to this model.
        /// This is never null, since some default logical options will be used instead.
        /// </para>
        /// <para>
        /// Note that the <see cref="StartConditions"/> in this instance is also never null.
        /// </para>
        /// </summary>
        public ReadOnlyLogicalOptions AppliedLogicalOptions { get; }

        /// <summary>
        /// Whether this element, given the current logical options, has any kind of logical relevance.
        /// If this is false, it is effectively safe to pretend the element doesn't exist.
        /// </summary>
        public bool LogicallyRelevant { get; }
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
    }

    /// <summary>
    /// The untyped interface portion of <see cref="AbstractUnfinalizedModelElement{ConcreteType, TargetType}"/>.
    /// </summary>
    public interface IUnfinalizedModelElement
    {
        /// <summary>
        /// Does the same as <see cref="AbstractUnfinalizedModelElement{ConcreteType, TargetType}.Finalize(ModelFinalizationMappings)"/>, 
        /// but with an untyped signature.
        /// </summary>
        /// <param name="mappings">A model containing mappings between unfinalized instances and corresponding finalized instances</param>
        /// <returns></returns>
        public IModelElement FinalizeUntyped(ModelFinalizationMappings mappings);
    }
}
