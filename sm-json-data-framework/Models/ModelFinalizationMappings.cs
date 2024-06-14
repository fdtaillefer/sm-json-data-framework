using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models
{
    /// <summary>
    /// Contains all the mappings of unfinalized model element instances to finalized model element instances to support the construction of a <see cref="SuperMetroidModel"/> instance,
    /// with helper methods that can either obtain finalized instances or create and remember them.
    /// </summary>
    public class ModelFinalizationMappings
    {
        /// <summary>
        /// The model instance that is being constructed by the model finalization operation supported by this ModelFinalizationMappings instance.
        /// </summary>
        public SuperMetroidModel Model { get; }
        private Dictionary<object, object> FinalizedMappingDictionary = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);

        public ModelFinalizationMappings(SuperMetroidModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Returns the finalized version of the provided source model element, creating it only if it hasn't been created yet.
        /// This version uses a constructor delegate that will not receive this ModelFinalizationMappings.
        /// </summary>
        /// <typeparam name="SourceType">The type of the unfinalized model element</typeparam>
        /// <typeparam name="TargetType">The type of the converted model element</typeparam>
        /// <param name="unfinalizedElement">The unfinalized model element to get a finalized version of</param>
        /// <param name="constructorDelegate">A delegate to the constructor that creates the finalized model element</param>
        /// <returns>The finalized model element</returns>
        /// <exception cref="Exception">If for some reason the constructor of the finalized model element did not invoke the callback it receives</exception>
        public TargetType GetFinalizedElement<SourceType, TargetType>(SourceType unfinalizedElement, Func<SourceType, Action<TargetType>, TargetType> constructorDelegate) 
            where SourceType : AbstractUnfinalizedModelElement<SourceType, TargetType>
            where TargetType: AbstractModelElement<SourceType, TargetType>
        {
            Func<SourceType, Action<TargetType>, ModelFinalizationMappings, TargetType> adjustedConstructorCallback 
                = (source, callback, mappings) => constructorDelegate(source, callback);
            return GetFinalizedElement(unfinalizedElement, adjustedConstructorCallback);
        }

        /// <summary>
        /// Returns the finalized version of the provided source model element, creating it only if it hasn't been created yet.
        /// This version uses a constructor delegate that will receive this ModelFinalizationMappings, on top of the two standard parameters.
        /// </summary>
        /// <typeparam name="SourceType">The type of the unfinalized model element</typeparam>
        /// <typeparam name="TargetType">The type of the converted model element</typeparam>
        /// <param name="unfinalizedElement">The unfinalized model element to get a finalized version of</param>
        /// <param name="constructorDelegate">A delegate to the constructor that creates the finalized model element</param>
        /// <returns>The finalized model element</returns>
        /// <exception cref="Exception">If for some reason the constructor of the finalized model element did not invoke the callback it receives</exception>
        public TargetType GetFinalizedElement<SourceType, TargetType>(SourceType unfinalizedElement, Func<SourceType, Action<TargetType>, ModelFinalizationMappings, TargetType> constructorDelegate)
            where SourceType : AbstractUnfinalizedModelElement<SourceType, TargetType>
            where TargetType : AbstractModelElement<SourceType, TargetType>
        {
            if (!FinalizedMappingDictionary.TryGetValue(unfinalizedElement, out object finalizedElement))
            {
                Action<TargetType> insertionCallback = finalized => FinalizedMappingDictionary.Add(unfinalizedElement, finalized);
                TargetType createdInstance = constructorDelegate.Invoke(unfinalizedElement, insertionCallback, this);
                if (!FinalizedMappingDictionary.TryGetValue(unfinalizedElement, out finalizedElement))
                {
                    if (finalizedElement != createdInstance)
                    {
                        throw new Exception("Creation of a finalized model element did not call provided callback with the created instance");
                    }
                }
            }
            return (TargetType)finalizedElement;
        }
    }
}
