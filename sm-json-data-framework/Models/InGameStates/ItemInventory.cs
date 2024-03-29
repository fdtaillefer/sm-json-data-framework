﻿using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
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
        /// <summary>
        /// Creates and returns an inventory representing the vanilla starting inventory.
        /// </summary>
        /// <param name="model">Model from which the Item instances for starting implicit items will be obtained.</param>
        /// <returns>The inventory</returns>
        public static ItemInventory CreateVanillaStartingInventory(SuperMetroidModel model)
        {
            return new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddItem(model.Items[SuperMetroidModel.POWER_BEAM_NAME])
                .ApplyAddItem(model.Items[SuperMetroidModel.POWER_SUIT_NAME]);
        }

        /// <summary>
        /// A constructor that receives an enumeration of ResourceCapacity to express the base resource maximums.
        /// </summary>
        /// <param name="baseResourceMaximums">The base maximum for all resources</param>
        public ItemInventory(IEnumerable<ResourceCapacity> baseResourceMaximums)
        {
            ResourceCapacityChanges = new ResourceCount();

            // Apply startingResources to base maximums
            BaseResourceMaximums = new ResourceCount();
            foreach (ResourceCapacity capacity in baseResourceMaximums)
            {
                BaseResourceMaximums.ApplyAmountIncrease(capacity.Resource, capacity.MaxAmount);
            }
        }

        /// <summary>
        /// A constructor that receives a ResourceCount to express the base resource maximums.
        /// </summary>
        /// <param name="baseResourceMaximums">A ResourceCount containing the base maximums.
        /// This instance will be cloned and will never be modified as a result of being passed here.</param>
        public ItemInventory(ResourceCount baseResourceMaximums)
        {
            BaseResourceMaximums = baseResourceMaximums.Clone();
            ResourceCapacityChanges = new ResourceCount();
        }

        public ItemInventory(ItemInventory other)
        {
            NonConsumableItems = new Dictionary<string, Item>(other.NonConsumableItems);
            ExpansionItems = new Dictionary<string, (ExpansionItem item, int count)>(other.ExpansionItems);
            DisabledItemNames = new HashSet<string>(other.DisabledItemNames);

            BaseResourceMaximums = other.BaseResourceMaximums.Clone();
            ResourceCapacityChanges = other.ResourceCapacityChanges.Clone();
        }

        public ItemInventory Clone()
        {
            return new ItemInventory(this);
        }

        protected IDictionary<string, Item> NonConsumableItems { get; set; } = new Dictionary<string, Item>();

        /// <summary>
        /// A set of items that have been manually disabled. This doesn't remove them from inventory but does prevent
        /// <see cref="HasItem(Item)"/> from returning true for them.
        /// </summary>
        protected ISet<string> DisabledItemNames { get; set; } = new HashSet<string>();

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
        public ResourceCount GetBaseResourceMaximumsClone()
        {
            return BaseResourceMaximums.Clone();
        }

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
        /// Returns whether this inventory contains an item with the provided name. If the item is present but disabled, returns false.
        /// </summary>
        /// <returns></returns>
        public bool HasItem(string itemName)
        {
            return (NonConsumableItems.ContainsKey(itemName) && !DisabledItemNames.Contains(itemName))|| ExpansionItems.ContainsKey(itemName);
        }

        /// <summary>
        /// Returns specifically whether the Varia Suit is present in this inventory.
        /// </summary>
        /// <returns></returns>
        public bool HasVariaSuit()
        {
            return HasItem(SuperMetroidModel.VARIA_SUIT_NAME);
        }

        /// <summary>
        /// Returns specifically whether the Gravity Suit is present in this inventory.
        /// </summary>
        /// <returns></returns>
        public bool HasGravitySuit()
        {
            return HasItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
        }

        /// <summary>
        /// Returns specifically whether the Speed Booster is present in this inventory.
        /// </summary>
        /// <returns></returns>
        public bool HasSpeedBooster()
        {
            return HasItem(SuperMetroidModel.SPEED_BOOSTER_NAME);
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
        /// Returns whether there is any explicit in-game item in this inventory.
        /// </summary>
        /// <returns></returns>
        public bool ContainsAnyInGameItem()
        {
            return NonConsumableItems.Values.Where(i => i is InGameItem).Any()
                || ExpansionItems.Values.Where(i => i.item is InGameItem).Any();
        }

        /// <summary>
        /// Adds the provided item to this inventory.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>This, for chaining</returns>
        public ItemInventory ApplyAddItem(Item item)
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

            return this;
        }

        /// <summary>
        /// Returns whether the provided item is present and disabled in this inventory.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns></returns>
        public bool IsItemDisabled(Item item)
        {
            return IsItemDisabled(item.Name);
        }

        /// <summary>
        /// Returns whether the item with the provided name is present and disabled in this inventory.
        /// </summary>
        /// <param name="itemName">Name of the item to check</param>
        /// <returns></returns>
        public bool IsItemDisabled(string itemName)
        {
            return NonConsumableItems.ContainsKey(itemName) && DisabledItemNames.Contains(itemName);
        }

        /// <summary>
        /// Returns the names of items that are disabled in this inventory.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetDisabledItemNames()
        {
            return DisabledItemNames;
        }

        /// <summary>
        ///  Disables the provided non-consumable if it's in this inventory.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to disable</param>
        /// <returns>This, for chaining</returns>
        public ItemInventory ApplyDisableItem(Item item)
        {
            return ApplyDisableItem(item.Name);
        }

        /// <summary>
        ///  Disables the non-consumable item with the provided name if it's in this inventory.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to disable</param>
        /// <returns>This, for chaining</returns>
        public ItemInventory ApplyDisableItem(string itemName)
        {
            if(NonConsumableItems.ContainsKey(itemName))
            {
                DisabledItemNames.Add(itemName);
            }
            return this;
        }

        /// <summary>
        ///  Re-enables the provided non-consumable if it's in this inventory and disabled.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to enable</param>
        /// <returns>This, for chaining</returns>
        public ItemInventory ApplyEnableItem(Item item)
        {
            return ApplyEnableItem(item.Name);
        }

        /// <summary>
        ///  Re-enables the non-consumable item with the provided name if it's in this inventory and disabled.
        ///  Does nothing otherwise.
        /// </summary>
        /// <param name="itemName">Name of the item to enable</param>
        /// <returns>This, for chaining</returns>
        public ItemInventory ApplyEnableItem(string itemName)
        {
            if (NonConsumableItems.ContainsKey(itemName))
            {
                DisabledItemNames.Remove(itemName);
            }
            return this;
        }

        /// <summary>
        /// Creates and returns a new ItemInventory containing all items from this Inventory
        /// that aren't found in the provided other inventory.
        /// </summary>
        /// <param name="other">The other inventory</param>
        /// <returns></returns>
        public ItemInventory ExceptWith(ItemInventory other)
        {
            // Create a new empty inventory
            ItemInventory returnInventory = new ItemInventory(BaseResourceMaximums);
            // Keeping disabled items makes no sense given what this method does
            returnInventory.DisabledItemNames.Clear();

            // For non-consumable items, just check for absence in other
            foreach (KeyValuePair<string, Item> kvp in NonConsumableItems)
            {
                if (!other.NonConsumableItems.ContainsKey(kvp.Key))
                {
                    returnInventory.ApplyAddItem(kvp.Value);
                }
            }

            // For expansion items, we need a count difference
            foreach(var kvp in ExpansionItems)
            {
                // Find how many of this item are present in this and not in other
                int timesMissing = 0;
                if (other.ExpansionItems.TryGetValue(kvp.Key, out var otherExpansionItem))
                {
                    // This can turn out negative, the for loop will properly do nothing
                    timesMissing = kvp.Value.count - otherExpansionItem.count;
                }
                else
                {
                    timesMissing = kvp.Value.count;
                }

                // Add this item as many times as the count difference
                for(int i = 0; i < timesMissing; i++)
                {
                    returnInventory.ApplyAddItem(kvp.Value.item);
                }
            }

            return returnInventory;
        }
    }

}
