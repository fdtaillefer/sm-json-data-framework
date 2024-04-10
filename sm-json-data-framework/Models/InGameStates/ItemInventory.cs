using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class ItemInventory : ReadOnlyItemInventory
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
            InternalBaseResourceMaximums = new ResourceCount(baseResourceMaximums);
        }

        /// <summary>
        /// A constructor that receives a ResourceCount to express the base resource maximums.
        /// </summary>
        /// <param name="baseResourceMaximums">A ResourceCount containing the base maximums.
        /// This instance will be cloned and will never be modified as a result of being passed here.</param>
        public ItemInventory(ResourceCount baseResourceMaximums)
        {
            InternalBaseResourceMaximums = baseResourceMaximums.Clone();
            ResourceCapacityChanges = new ResourceCount();
        }

        public ItemInventory(ItemInventory other)
        {
            InternalNonConsumableItems = new Dictionary<string, Item>(other.InternalNonConsumableItems);
            InternalExpansionItems = new Dictionary<string, (ExpansionItem item, int count)>(other.InternalExpansionItems);
            InternalDisabledItemNames = new HashSet<string>(other.InternalDisabledItemNames);

            InternalBaseResourceMaximums = other.InternalBaseResourceMaximums.Clone();
            ResourceCapacityChanges = other.ResourceCapacityChanges.Clone();
        }

        /// <summary>
        /// Creates and returns a new ItemInventory containing data copied from the provided otherInventory, 
        /// except the base resource maximums which will be a clone of the provided baseResourceMaximums
        /// </summary>
        /// <param name="otherInventory">The other inventory, on which most data will be based</param>
        /// <param name="baseResourceMaximums">The base resource maximums to use instead of the one from otherInventory</param>
        public ItemInventory(ItemInventory otherInventory, ReadOnlyResourceCount baseResourceMaximums) : this(otherInventory)
        {
            InternalBaseResourceMaximums = baseResourceMaximums.Clone();
        }

        public ItemInventory Clone()
        {
            return new ItemInventory(this);
        }

        /// <summary>
        /// Creates and returns a clone of this inventory, but with the provided base resource maximums
        /// </summary>
        /// <param name="baseResourceMaximums">The base resource maximums to use for the clone</param>
        /// <returns>The created clone</returns>
        public ItemInventory WithBaseResourceMaximums(ReadOnlyResourceCount baseResourceMaximums)
        {
            return new ItemInventory(this, baseResourceMaximums);
        }

        /// <summary>
        /// Returns a read-only view of this ItemInventory. 
        /// The view will still be this instance and reflect any changes subsequently applied to it.
        /// </summary>
        /// <returns></returns>
        public ReadOnlyItemInventory AsReadOnly()
        {
            return this;
        }

        protected IDictionary<string, Item> InternalNonConsumableItems { get; } = new Dictionary<string, Item>();
        public ReadOnlyDictionary<string, Item> NonConsumableItems { get { return InternalNonConsumableItems.AsReadOnly(); } }

        protected ISet<string> InternalDisabledItemNames { get; } = new HashSet<string>();
        public IReadOnlySet<string> DisabledItemNames { get { return InternalDisabledItemNames.AsReadOnly(); } }

        
        protected IDictionary<string, (ExpansionItem item, int count)> InternalExpansionItems { get; } = new Dictionary<string, (ExpansionItem item, int count)>();
        public ReadOnlyDictionary<string, (ExpansionItem item, int count)> ExpansionItems { get { return InternalExpansionItems.AsReadOnly(); } }

        protected ResourceCount InternalBaseResourceMaximums { get; }

        public ReadOnlyResourceCount BaseResourceMaximums { get { return InternalBaseResourceMaximums.AsReadOnly(); } }

        /// <summary>
        /// Expresses the change that this inventory applies on maximum resource counts.
        /// This differs from maximum counts because it excludes base maximum counts.
        /// </summary>
        protected ResourceCount ResourceCapacityChanges { get; }

        private ReadOnlyResourceCount _resourceMaximums;
        public ReadOnlyResourceCount ResourceMaximums 
        {
            get
            {
                if(_resourceMaximums == null)
                {
                    _resourceMaximums = new AggregateResourceCount(BaseResourceMaximums, ResourceCapacityChanges);
                }
                return _resourceMaximums;
            } 
        }

        public bool HasItem(Item item)
        {
            return HasItem(item.Name);
        }

        public bool HasItem(string itemName)
        {
            return (InternalNonConsumableItems.ContainsKey(itemName) && !InternalDisabledItemNames.Contains(itemName))|| InternalExpansionItems.ContainsKey(itemName);
        }

        public bool HasVariaSuit()
        {
            return HasItem(SuperMetroidModel.VARIA_SUIT_NAME);
        }

        public bool HasGravitySuit()
        {
            return HasItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
        }

        public bool HasSpeedBooster()
        {
            return HasItem(SuperMetroidModel.SPEED_BOOSTER_NAME);
        }

        public bool ContainsAnyInGameItem()
        {
            return InternalNonConsumableItems.Values.Where(i => i is InGameItem).Any()
                || InternalExpansionItems.Values.Where(i => i.item is InGameItem).Any();
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

                // In either case, the inner maximum of the proper resource should increase
                ResourceCapacityChanges.ApplyAmountIncrease(expansionItem.Resource, expansionItem.ResourceAmount);

                // Capacity pickups may add current resources as well, but they are not repeatable so by default we don't want logic to rely on them.
                // So we will not alter current resources.
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

        public bool IsItemDisabled(Item item)
        {
            return IsItemDisabled(item.Name);
        }

        public bool IsItemDisabled(string itemName)
        {
            return InternalNonConsumableItems.ContainsKey(itemName) && InternalDisabledItemNames.Contains(itemName);
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
            if(InternalNonConsumableItems.ContainsKey(itemName))
            {
                InternalDisabledItemNames.Add(itemName);
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
            if (InternalNonConsumableItems.ContainsKey(itemName))
            {
                InternalDisabledItemNames.Remove(itemName);
            }
            return this;
        }

        public ItemInventory ExceptIn(ReadOnlyItemInventory other)
        {
            // Create a new empty inventory
            ItemInventory returnInventory = new ItemInventory(InternalBaseResourceMaximums);
            // Keeping disabled items makes no sense given what this method does
            returnInventory.InternalDisabledItemNames.Clear();

            // For non-consumable items, just check for absence in other
            foreach (KeyValuePair<string, Item> kvp in InternalNonConsumableItems)
            {
                if (!other.NonConsumableItems.ContainsKey(kvp.Key))
                {
                    returnInventory.ApplyAddItem(kvp.Value);
                }
            }

            // For expansion items, we need a count difference
            foreach(var kvp in InternalExpansionItems)
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

    /// <summary>
    /// A read-only interface for an <see cref="ItemInventory"/>, allowing consultation without modification.
    /// </summary>
    public interface ReadOnlyItemInventory
    {
        /// <summary>
        /// Creates and returns a copy of this ItemInventory.
        /// </summary>
        /// <returns>The new copy, as a full-fledged ItemInventory</returns>
        public ItemInventory Clone();

        /// <summary>
        /// Returns a clone of this inventory, but with the provided base resource Maximums
        /// </summary>
        /// <param name="baseMaximumResourceMaximums">The base resource maximums to assign to the new clone</param>
        /// <returns>The new copy, as a full-fledged ItemInventory</returns>
        public ItemInventory WithBaseResourceMaximums(ReadOnlyResourceCount baseResourceMaximums);

        /// <summary>
        /// A dictionary of the non consumable items in this inventory, mapped by name.
        /// </summary>
        public ReadOnlyDictionary<string, Item> NonConsumableItems { get; }

        /// <summary>
        /// A set of items that have been manually disabled. This doesn't remove them from inventory but does prevent
        /// <see cref="HasItem(Item)"/> from returning true for them.
        /// </summary>
        public IReadOnlySet<string> DisabledItemNames { get; }

        /// <summary>
        /// A dictionary of the non consumable items in this inventory (bundled with the number of pickups owned), mapped by name.
        /// </summary>
        public ReadOnlyDictionary<string, (ExpansionItem item, int count)> ExpansionItems { get; }

        /// <summary>
        /// The resource maximums that Samus would have if this item inventory contained no expansion items.
        /// </summary>
        public ReadOnlyResourceCount BaseResourceMaximums { get; }

        /// <summary>
        /// Expresses the actual maximum resources for this inventory, accounting for both base resource maximums and expansion items.
        /// </summary>
        public ReadOnlyResourceCount ResourceMaximums { get; }

        /// <summary>
        /// Returns whether this inventory contains the provided item.
        /// </summary>
        /// <param name="item">Item to look for</param>
        /// <returns></returns>
        public bool HasItem(Item item);

        /// <summary>
        /// Returns whether this inventory contains an item with the provided name. If the item is present but disabled, returns false.
        /// </summary>
        /// <param name="item">Item name to look for</param>
        /// <returns></returns>
        public bool HasItem(string itemName);

        /// <summary>
        /// Returns specifically whether the Varia Suit is present in this inventory. If it's present but disabled, return false.
        /// </summary>
        /// <returns></returns>
        public bool HasVariaSuit();

        /// <summary>
        /// Returns specifically whether the Gravity Suit is present in this inventory. If it's present but disabled, return false.
        /// </summary>
        /// <returns></returns>
        public bool HasGravitySuit();

        /// <summary>
        /// Returns specifically whether the Speed Booster is present in this inventory. If it's present but disabled, return false.
        /// </summary>
        /// <returns></returns>
        public bool HasSpeedBooster();

        /// <summary>
        /// Returns whether there is any explicit in-game item in this inventory.
        /// </summary>
        /// <returns></returns>
        public bool ContainsAnyInGameItem();

        /// <summary>
        /// Returns whether the provided item is present but disabled in this inventory.
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns></returns>
        public bool IsItemDisabled(Item item);

        /// <summary>
        /// Returns whether the item with the provided name is present but disabled in this inventory.
        /// </summary>
        /// <param name="itemName">Name of the item to check</param>
        /// <returns></returns>
        public bool IsItemDisabled(string itemName);

        /// <summary>
        /// Creates and returns a new ItemInventory containing all items from this Inventory
        /// that aren't found in the provided other inventory.
        /// </summary>
        /// <param name="other">The other inventory</param>
        /// <returns></returns>
        public ItemInventory ExceptIn(ReadOnlyItemInventory other);
    }

}
