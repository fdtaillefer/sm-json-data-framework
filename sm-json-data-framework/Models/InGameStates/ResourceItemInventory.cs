using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Rules.InitialState;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// A partial inventory which contains only what relates to resources - so base resource maximums and expansion items.
    /// </summary>
    public class ResourceItemInventory: ReadOnlyResourceItemInventory
    {
        /// <summary>
        /// A constructor that receives an enumeration of ResourceCapacity to express the base resource maximums.
        /// </summary>
        /// <param name="baseResourceMaximums">The base maximum for all resources</param>
        public ResourceItemInventory(IEnumerable<ResourceCapacity> baseResourceMaximums)
        {
            ResourceCapacityChanges = new ResourceCount();
            InternalBaseResourceMaximums = new ResourceCount(baseResourceMaximums);
        }

        /// <summary>
        /// A constructor that receives a ResourceCount to express the base resource maximums.
        /// </summary>
        /// <param name="baseResourceMaximums">A ResourceCount containing the base maximums.
        /// This instance will be cloned and will never be modified as a result of being passed here.</param>
        public ResourceItemInventory(ResourceCount baseResourceMaximums)
        {
            InternalBaseResourceMaximums = baseResourceMaximums.Clone();
            ResourceCapacityChanges = new ResourceCount();
        }

        /// <summary>
        /// Creates and returns a new ResourceItemInventory containing data copied from the provided otherInventory, 
        /// except (optionally) the base resource maximums which will be a clone of the provided baseResourceMaximums (if any)
        /// </summary>
        /// <param name="otherInventory">The other inventory, on which most data will be based</param>
        /// <param name="baseResourceMaximums">The base resource maximums to use instead of the one from otherInventory</param>
        public ResourceItemInventory(ResourceItemInventory otherInventory, ReadOnlyResourceCount baseResourceMaximums = null)
        {
            InternalExpansionItems = new Dictionary<string, (ExpansionItem item, int count)>(otherInventory.InternalExpansionItems);

            if (baseResourceMaximums == null)
            {
                InternalBaseResourceMaximums = otherInventory.InternalBaseResourceMaximums.Clone();
            }
            else
            {
                InternalBaseResourceMaximums = baseResourceMaximums.Clone();
            }
            ResourceCapacityChanges = otherInventory.ResourceCapacityChanges.Clone();
        }

        public ResourceItemInventory(UnfinalizedStartConditions unfinalizedStartConditions, ModelFinalizationMappings mappings)
        {
            InternalBaseResourceMaximums = unfinalizedStartConditions.BaseResourceMaximums.Clone();
            ResourceCapacityChanges = new ResourceCount();

            foreach (var itemAndCount in unfinalizedStartConditions.StartingInventory.ExpansionItems.Values)
            {
                ExpansionItem item = itemAndCount.item.Finalize(mappings);
                ApplyAddExpansionItem(item, itemAndCount.count);
            }
        }

        public ResourceItemInventory(UnfinalizedResourceItemInventory unfinalizedResourceInventory, ReadOnlyResourceCount baseResourceMaximums, ModelFinalizationMappings mappings)
        {
            InternalBaseResourceMaximums = baseResourceMaximums.Clone();
            ResourceCapacityChanges = new ResourceCount();

            foreach (var itemAndCount in unfinalizedResourceInventory.ExpansionItems.Values)
            {
                ExpansionItem item = itemAndCount.item.Finalize(mappings);
                ApplyAddExpansionItem(item, itemAndCount.count);
            }
        }

        public virtual ResourceItemInventory Clone()
        {
            return new ResourceItemInventory(this);
        }

        /// <summary>
        /// Creates and returns a clone of this inventory, but with the provided base resource maximums
        /// </summary>
        /// <param name="baseResourceMaximums">The base resource maximums to use for the clone</param>
        /// <returns>The created clone</returns>
        public virtual ResourceItemInventory WithBaseResourceMaximums(ReadOnlyResourceCount baseResourceMaximums)
        {
            return new ResourceItemInventory(this, baseResourceMaximums);
        }

        /// <summary>
        /// Returns a read-only view of this ResourceItemInventory. 
        /// The view will still be this instance and reflect any changes subsequently applied to it.
        /// </summary>
        /// <returns></returns>
        public virtual ReadOnlyResourceItemInventory AsReadOnly()
        {
            return this;
        }

        protected IDictionary<string, (ExpansionItem item, int count)> InternalExpansionItems { get; } = new Dictionary<string, (ExpansionItem item, int count)>();
        public ReadOnlyDictionary<string, (ExpansionItem item, int count)> ExpansionItems => InternalExpansionItems.AsReadOnly();

        protected ResourceCount InternalBaseResourceMaximums { get; }
        public ReadOnlyResourceCount BaseResourceMaximums => InternalBaseResourceMaximums.AsReadOnly();

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
                if (_resourceMaximums == null)
                {
                    _resourceMaximums = new AggregateResourceCount(BaseResourceMaximums, ResourceCapacityChanges);
                }
                return _resourceMaximums;
            }
        }

        public virtual bool HasItem(Item item)
        {
            return HasItem(item.Name);
        }

        public virtual bool HasItem(string itemName)
        {
            return InternalExpansionItems.ContainsKey(itemName);
        }

        /// <summary>
        /// Adds the provided expansionItem to this inventory.
        /// </summary>
        /// <param name="expansionItem"></param>
        /// <param name="count">Number of times to add the item</param>
        /// <returns>This, for chaining</returns>
        public ResourceItemInventory ApplyAddExpansionItem(ExpansionItem expansionItem, int count = 1)
        {
            // If we already have some of this, just increase the count
            if (InternalExpansionItems.ContainsKey(expansionItem.Name))
            {
                // Increment count
                var itemWithCount = InternalExpansionItems[expansionItem.Name];
                itemWithCount.count += count;
                InternalExpansionItems[expansionItem.Name] = itemWithCount;
            }
            // If we don't have any of this, we have to insert it in the dictionary
            else
            {
                // Add item with the requested quantity
                InternalExpansionItems.Add(expansionItem.Name, (expansionItem, count));
            }

            // In either case, the inner maximum of the proper resource should increase
            ResourceCapacityChanges.ApplyAmountIncrease(expansionItem.Resource, expansionItem.ResourceAmount * count);

            // Capacity pickups may add current resources as well, but they are not repeatable so by default we don't want logic to rely on them.
            // So we will not alter current resources.


            return this;
        }
    }

    /// <summary>
    /// A read-only interface for a <see cref="ResourceItemInventory"/>, allowing consultation without modification.
    /// </summary>
    public interface ReadOnlyResourceItemInventory
    {
        /// <summary>
        /// Creates and returns a copy of this inventory.
        /// </summary>
        /// <returns>The new copy, as a full-fledged (not just read-only) inventory</returns>
        public ResourceItemInventory Clone();

        /// <summary>
        /// Returns a clone of this inventory, but with the provided base resource Maximums
        /// </summary>
        /// <param name="baseResourceMaximums">The base resource maximums to assign to the new clone</param>
        /// <returns>The new copy, as a full-fledged (not just read-only) inventory</returns>
        public ResourceItemInventory WithBaseResourceMaximums(ReadOnlyResourceCount baseResourceMaximums);

        /// <summary>
        /// A dictionary of the expansion items in this inventory (bundled with the number of pickups owned), mapped by name.
        /// </summary>
        public ReadOnlyDictionary<string, (ExpansionItem item, int count)> ExpansionItems { get; }

        /// <summary>
        /// The resource maximums that Samus would have if this inventory contained no expansion items.
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
        /// Returns whether this inventory contains an item with the provided name.
        /// </summary>
        /// <param name="item">Item name to look for</param>
        /// <returns></returns>
        public bool HasItem(string itemName);
    }
}
