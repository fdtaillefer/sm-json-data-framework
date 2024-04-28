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
        /// Creates and returns start conditions using the provided ItemContainer.
        /// </summary>
        /// <param name="model">A SuperMetroidModel in the process of being created. Its rooms (and nodes), items, and game flags
        /// are already assigned and available to be referenced.</param>
        /// <param name="itemContainer">The result of reading items.json.</param>
        /// <returns>The created start conditions</returns>
        StartConditions CreateStartConditions(SuperMetroidModel model, ItemContainer itemContainer);

        /// <summary>
        /// Creates and returns start conditions using the provided RawItemContainer.
        /// </summary>
        /// <param name="model">A SuperMetroidModel in the process of being created. Its rooms (and nodes), items, and game flags
        /// are already assigned and available to be referenced.</param>
        /// <param name="rawItemContainer">The raw result of reading items.json.</param>
        /// <returns>The created start conditions</returns>
        StartConditions CreateStartConditions(SuperMetroidModel model, RawItemContainer rawItemContainer);
    }
}
