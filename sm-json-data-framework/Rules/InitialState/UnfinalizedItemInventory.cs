using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Rules.InitialState
{
    /// <summary>
    /// A very simple inventory that contains <see cref="UnfinalizedItem"/>s and no logic.
    /// </summary>
    public class UnfinalizedItemInventory: UnfinalizedResourceItemInventory
    {
        /// <summary>
        /// Creates and returns an inventory representing the vanilla starting inventory.
        /// </summary>
        /// <param name="model">Model from which the Item instances for starting implicit items will be obtained.</param>
        /// <returns>The inventory</returns>
        public static UnfinalizedItemInventory CreateVanillaStartingUnfinalizedInventory(UnfinalizedSuperMetroidModel model)
        {
            return new UnfinalizedItemInventory()
                .ApplyAddItem(model.Items[SuperMetroidModel.POWER_BEAM_NAME])
                .ApplyAddItem(model.Items[SuperMetroidModel.POWER_SUIT_NAME]);
        }

        public UnfinalizedItemInventory()
        {

        }

        public UnfinalizedItemInventory(UnfinalizedItemInventory other): base(other)
        {
            InternalNonConsumableItems = new Dictionary<string, UnfinalizedItem>(other.InternalNonConsumableItems);
        }

        public override UnfinalizedItemInventory Clone()
        {
            return new UnfinalizedItemInventory(this);
        }

        protected IDictionary<string, UnfinalizedItem> InternalNonConsumableItems { get; } = new Dictionary<string, UnfinalizedItem>();
        public ReadOnlyDictionary<string, UnfinalizedItem> NonConsumableItems => InternalNonConsumableItems.AsReadOnly();

        /// <summary>
        /// Adds the provided item to this inventory.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>This, for chaining</returns>
        public UnfinalizedItemInventory ApplyAddItem(UnfinalizedItem item)
        {
            // Expansion items are handled by base class
            if (item is UnfinalizedExpansionItem expansionItem)
            {
                ApplyAddExpansionItem(expansionItem);
            }
            // Regular items don't have a count
            else
            {
                if (!InternalNonConsumableItems.ContainsKey(item.Name))
                {
                    InternalNonConsumableItems.Add(item.Name, item);
                }
            }

            return this;
        }

        /// <summary>
        /// Removes the provided item to this inventory. For expansion items, removes one - not all.
        /// Does nothing if the item is not present.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>This, for chaining</returns>
        public UnfinalizedItemInventory ApplyRemoveItem(UnfinalizedItem item)
        {
            // Expansion items are handled by base class
            if (item is UnfinalizedExpansionItem expansionItem)
            {
                ApplyRemoveExpansionItem(expansionItem);
            }
            // Regular items don't have a count
            else
            {
                InternalNonConsumableItems.Remove(item.Name);
            }

            return this;
        }
    }
}
