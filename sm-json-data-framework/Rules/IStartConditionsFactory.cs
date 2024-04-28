using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Rules
{
    /// <summary>
    /// An interface for classes that can create start conditions based on a partial model and the contents of items.json.
    /// It gets called at the appropriate time during the creation of a <see cref="SuperMetroidModel"/>
    /// </summary>
    public interface IStartConditionsFactory
    {
        /// <summary>
        /// Creates and returns start conditions using the provided model and BasicStartConditions.
        /// </summary>
        /// <param name="model">A SuperMetroidModel in the process of being created. Its rooms (and nodes), items, and game flags
        /// are already assigned and available to be referenced.</param>
        /// <param name="basicStartConditions">A model describing the start conditions using no complex objects.</param>
        /// <returns>The created start conditions</returns>
        StartConditions CreateStartConditions(SuperMetroidModel model, BasicStartConditions basicStartConditions);
    }
}
