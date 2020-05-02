using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// <para>A container for in-game items.</para>
    /// <para>Includes and understands both non-consumable items (which are unstackable)
    /// and expansion items (which can be stacked, and increase the max value of resources).</para>
    /// <para>An ItemInventory has no care for current resource counts, but is aware of maximum resource values.</para>
    /// </summary>
    public class ItemInventory
    {
        public ItemInventory(IEnumerable<ResourceCapacity> startingResources)
        {
            ResourceCapacityChanges = new ResourceCount();

            // Apply startingResources to base maximums
            BaseResourceMaximums = new ResourceCount();
            foreach (ResourceCapacity capacity in startingResources)
            {
                BaseResourceMaximums.ApplyAmountIncrease(capacity.Resource, capacity.MaxAmount);
            }
        }

        public ItemInventory(ItemInventory other)
        {
            NonConsumableItems = new Dictionary<string, Item>(other.NonConsumableItems);

            ExpansionItems = new Dictionary<string, (ExpansionItem item, int count)>(other.ExpansionItems);
        }

        public ItemInventory Clone()
        {
            return new ItemInventory(this);
        }

        protected IDictionary<string, Item> NonConsumableItems { get; set; } = new Dictionary<string, Item>();

        protected IDictionary<string, (ExpansionItem item, int count)> ExpansionItems { get; set; } = new Dictionary<string, (ExpansionItem item, int count)>();

        /// <summary>
        /// Returns a read-only view of the dictionary of non-consumable items in this inventory.
        /// The items are mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, Item> GetNonConsumableItemsDictionary()
        {
            return new ReadOnlyDictionary<string, Item>(NonConsumableItems);
        }

        /// <summary>
        /// Returns a read-only view of the dictionary of expansion items (along with how many of each is present) in this inventory.
        /// The items are mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, (ExpansionItem item, int count)> GetExpansionItemsDictionary()
        {
            return new ReadOnlyDictionary<string, (ExpansionItem item, int count)>(ExpansionItems);
        }

        /// <summary>
        /// The resource maximums that are available with 0 items.
        /// Useful to differentiate between the maximum amounts associated with this inventory,
        /// and the impact on maximum amounts made by this inventory.
        /// </summary>
        protected ResourceCount BaseResourceMaximums { get; set; }

        /// <summary>
        /// Expresses the change that this inventory applies on maximum resource counts.
        /// This differs from maximum counts because it excludes base maximum counts.
        /// </summary>
        protected ResourceCount ResourceCapacityChanges { get; set; }

        //protected IDictionary<RechargeableResourceEnum, int> ResourceMaximums { get; set; } = new Dictionary<RechargeableResourceEnum, int>();

        /// <summary>
        /// Returns whether this inventory contains the provided item.
        /// </summary>
        /// <param name="item">Item to look for</param>
        /// <returns></returns>
        public bool HasItem(Item item)
        {
            return HasItem(item.Name);
        }

        /// <summary>
        /// Returns whether this inventory contains an item with the provided name.
        /// </summary>
        /// <returns></returns>
        public bool HasItem(string itemName)
        {
            return NonConsumableItems.ContainsKey(itemName) || ExpansionItems.ContainsKey(itemName);
        }

        /// <summary>
        /// Returns specifically whether the Varia Suit is present in this inventory.
        /// </summary>
        /// <returns></returns>
        public bool HasVariaSuit()
        {
            return HasItem("Varia");
        }

        /// <summary>
        /// Returns specifically whether the Gravity Suit is present in this inventory.
        /// </summary>
        /// <returns></returns>
        public bool HasGravitySuit()
        {
            return HasItem("Gravity");
        }

        /// <summary>
        /// Returns specifically whether the Speed Booster is present in this inventory.
        /// </summary>
        /// <returns></returns>
        public bool HasSpeedBooster()
        {
            return HasItem("SpeedBooster");
        }

        /// <summary>
        /// Returns the max amount of the provided resource, according to the contents of this inventory.
        /// </summary>
        /// <param name="resource">The resource to return the maximumamount for.</param>
        /// <returns></returns>
        public int GetMaxAmount(RechargeableResourceEnum resource)
        {
            return BaseResourceMaximums.GetAmount(resource) + ResourceCapacityChanges.GetAmount(resource);
        }

        /// <summary>
        /// Adds the provided item to this inventory.
        /// </summary>
        /// <param name="item"></param>
        public void ApplyAddItem(Item item)
        {
            // Expansion items have a count
            if (item is ExpansionItem expansionItem)
            {
                if (!ExpansionItems.ContainsKey(expansionItem.Name))
                {
                    // Add item with an initial quantity of 1
                    ExpansionItems.Add(expansionItem.Name, (expansionItem, 1));
                }
                else
                {
                    // Increment count
                    var itemWithCount = ExpansionItems[expansionItem.Name];
                    itemWithCount.count++;
                    ExpansionItems[expansionItem.Name] = itemWithCount;
                }

                // In either case, the inner maximum of the proper resource should increase
                ResourceCapacityChanges.ApplyAmountIncrease(expansionItem.Resource, expansionItem.ResourceAmount);

                // Capacity pickups may add current resources as well, but they are not repeatable so by default we don't want logic to rely on them.
                // So we will not alter current resources.
            }
            // Regular items don't have a count
            else
            {
                if (!NonConsumableItems.ContainsKey(item.Name))
                {
                    NonConsumableItems.Add(item.Name, item);
                }
            }
        }
    }
}
