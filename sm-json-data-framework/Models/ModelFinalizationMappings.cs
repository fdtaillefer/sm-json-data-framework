using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models
{
    /// <summary>
    /// Contains all the mappings of unfinalized model instances to finalized model instances,
    /// with helper methods that can either obtain finalized instances or create and remember them.
    /// </summary>
    public class ModelFinalizationMappings
    {
        private Dictionary<object, object> FinalizedMappingDictionary = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// Returns the finalized version of the provided source model, creating it only if it hasn't been created yet.
        /// This version uses a constructor delegate that will not receive this ModelFinalizationMappings.
        /// </summary>
        /// <typeparam name="SourceType">The type of the unfinalized model</typeparam>
        /// <typeparam name="TargetType">The type of the converted model</typeparam>
        /// <param name="unfinalizedModel">The unfinalized model to get a finalized version of</param>
        /// <param name="constructorDelegate">A delegate to the constructor that creates the finalized model</param>
        /// <returns>The finalized model</returns>
        /// <exception cref="Exception">If for some reason the constructor the finalized model did not invoke the callback it receives</exception>
        public TargetType GetFinalizedModel<SourceType, TargetType>(SourceType unfinalizedModel, Func<SourceType, Action<TargetType>, TargetType> constructorDelegate) 
            where SourceType : AbstractUnfinalizedModelElement<SourceType, TargetType>
            where TargetType: AbstractModelElement<SourceType, TargetType>
        {
            Func<SourceType, Action<TargetType>, ModelFinalizationMappings, TargetType> adjustedConstructorCallback 
                = (source, callback, mappings) => constructorDelegate(source, callback);
            return GetFinalizedModel(unfinalizedModel, adjustedConstructorCallback);
        }

        /// <summary>
        /// Returns the finalized version of the provided source model, creating it only if it hasn't been created yet.
        /// This version uses a constructor delegate that will receive this ModelFinalizationMappings, on top of the two standard parameters.
        /// </summary>
        /// <typeparam name="SourceType">The type of the unfinalized model</typeparam>
        /// <typeparam name="TargetType">The type of the converted model</typeparam>
        /// <param name="unfinalizedModel">The unfinalized model to get a finalized version of</param>
        /// <param name="constructorDelegate">A delegate to the constructor that creates the finalized model</param>
        /// <returns>The finalized model</returns>
        /// <exception cref="Exception">If for some reason the constructor the finalized model did not invoke the callback it receives</exception>
        public TargetType GetFinalizedModel<SourceType, TargetType>(SourceType unfinalizedModel, Func<SourceType, Action<TargetType>, ModelFinalizationMappings, TargetType> constructorDelegate)
            where SourceType : AbstractUnfinalizedModelElement<SourceType, TargetType>
            where TargetType : AbstractModelElement<SourceType, TargetType>
        {
            if (!FinalizedMappingDictionary.TryGetValue(unfinalizedModel, out object finalizedModel))
            {
                Action<TargetType> insertionCallback = finalized => FinalizedMappingDictionary.Add(unfinalizedModel, finalized);
                TargetType createdInstance = constructorDelegate.Invoke(unfinalizedModel, insertionCallback, this);
                if (!FinalizedMappingDictionary.TryGetValue(unfinalizedModel, out finalizedModel))
                {
                    if (finalizedModel != createdInstance)
                    {
                        throw new Exception("Creation of a finalized model did not call provided callback with the created instance");
                    }
                }
            }
            return (TargetType)finalizedModel;
        }
    }
}
