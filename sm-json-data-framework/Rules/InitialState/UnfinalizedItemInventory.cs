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
    public class UnfinalizedItemInventory
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

        public UnfinalizedItemInventory(UnfinalizedItemInventory other)
        {
            InternalNonConsumableItems = new Dictionary<string, UnfinalizedItem>(other.InternalNonConsumableItems);
            InternalExpansionItems = new Dictionary<string, (UnfinalizedExpansionItem item, int count)>(other.InternalExpansionItems);
        }

        public UnfinalizedItemInventory Clone()
        {
            return new UnfinalizedItemInventory(this);
        }

        protected IDictionary<string, UnfinalizedItem> InternalNonConsumableItems { get; } = new Dictionary<string, UnfinalizedItem>();
        public ReadOnlyDictionary<string, UnfinalizedItem> NonConsumableItems { get { return InternalNonConsumableItems.AsReadOnly(); } }

        protected IDictionary<string, (UnfinalizedExpansionItem item, int count)> InternalExpansionItems { get; } = new Dictionary<string, (UnfinalizedExpansionItem item, int count)>();
        public ReadOnlyDictionary<string, (UnfinalizedExpansionItem item, int count)> ExpansionItems { get { return InternalExpansionItems.AsReadOnly(); } }

        /// <summary>
        /// Adds the provided item to this inventory.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>This, for chaining</returns>
        public UnfinalizedItemInventory ApplyAddItem(UnfinalizedItem item)
        {
            // Expansion items have a count
            if (item is UnfinalizedExpansionItem expansionItem)
            {
                if (!InternalExpansionItems.ContainsKey(expansionItem.Name))
                {
                    // Add item with an initial quantity of 1
                    InternalExpansionItems.Add(expansionItem.Name, (expansionItem, 1));
                }
                else
                {
                    // Increment count
                    var itemWithCount = InternalExpansionItems[expansionItem.Name];
                    itemWithCount.count++;
                    InternalExpansionItems[expansionItem.Name] = itemWithCount;
                }
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
            // Expansion items have a count
            if (item is UnfinalizedExpansionItem expansionItem)
            {
                if (InternalExpansionItems.ContainsKey(expansionItem.Name))
                {
                    // Decrement count
                    var itemWithCount = InternalExpansionItems[expansionItem.Name];
                    itemWithCount.count--;
                    if (itemWithCount.count <= 0)
                    {
                        InternalExpansionItems.Remove(item.Name);
                    }
                    else
                    {
                        InternalExpansionItems[expansionItem.Name] = itemWithCount;
                    }
                }
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
