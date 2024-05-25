using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Rules.InitialState
{
    /// <summary>
    /// A partial inventory which contains only what relates to resources with <see cref="UnfinalizedItem"/>s and no logic.
    /// </summary>
    public class UnfinalizedResourceItemInventory
    {
        protected IDictionary<string, (UnfinalizedExpansionItem item, int count)> InternalExpansionItems { get; } = new Dictionary<string, (UnfinalizedExpansionItem item, int count)>();
        public ReadOnlyDictionary<string, (UnfinalizedExpansionItem item, int count)> ExpansionItems => InternalExpansionItems.AsReadOnly();

        public UnfinalizedResourceItemInventory()
        {

        }

        public UnfinalizedResourceItemInventory(UnfinalizedResourceItemInventory other)
        {
            InternalExpansionItems = new Dictionary<string, (UnfinalizedExpansionItem item, int count)>(other.InternalExpansionItems);
        }

        public virtual UnfinalizedResourceItemInventory Clone()
        {
            return new UnfinalizedResourceItemInventory(this);
        }

        /// <summary>
        /// Adds the provided item to this inventory.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>This, for chaining</returns>
        public UnfinalizedResourceItemInventory ApplyAddExpansionItem(UnfinalizedExpansionItem item)
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

            return this;
        }

        /// <summary>
        /// Removes the provided item to this inventory. For expansion items, removes one - not all.
        /// Does nothing if the item is not present.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>This, for chaining</returns>
        public UnfinalizedResourceItemInventory ApplyRemoveExpansionItem(UnfinalizedExpansionItem item)
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

            return this;
        }
    }
}
