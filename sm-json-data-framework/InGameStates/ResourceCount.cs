using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace sm_json_data_framework.InGameStates
{
    /// <summary>
    /// Contains values for all rechargeable resource types. The context of those values is not known by this container.
    /// </summary>
    public class ResourceCount : ReadOnlyResourceCount
    {
        /// <summary>
        /// Creates and returns an instance of ResourceCount containing the vanilla base maximums.
        /// </summary>
        /// <returns>The ResourceCount</returns>
        public static ResourceCount CreateVanillaBaseResourceMaximums()
        {
            return new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, 99);
        }

        public ResourceCount()
        {
            foreach (RechargeableResourceEnum currentResource in Enum.GetValues<RechargeableResourceEnum>())
            {
                Amounts.Add(currentResource, 0);
            }
        }

        public ResourceCount(IEnumerable<ResourceCapacity> resourceAmounts) : this()
        {
            // Apply each resource amount
            foreach (ResourceCapacity capacity in resourceAmounts)
            {
                ApplyAmountIncrease(capacity.Resource, capacity.MaxAmount);
            }
        }

        /// <summary>
        /// Returns a read-only view of this ResourceCount. 
        /// The view will still be this instance and reflect any changes subsequently applied to it.
        /// </summary>
        /// <returns></returns>
        public ReadOnlyResourceCount AsReadOnly()
        {
            return this;
        }

        public ResourceCount(ReadOnlyResourceCount other)
        {
            ApplyAmounts(other);
        }

        public ResourceCount Clone()
        {
            return new ResourceCount(this);
        }

        public ResourceCount GetVariationWith(ReadOnlyResourceCount other)
        {
            ResourceCount returnValue = new ResourceCount();
            foreach (RechargeableResourceEnum currentResource in Enum.GetValues<RechargeableResourceEnum>())
            {
                returnValue.ApplyAmount(currentResource, GetAmount(currentResource) - other.GetAmount(currentResource));
            }

            return returnValue;
        }

        private IDictionary<RechargeableResourceEnum, int> Amounts { get; } = new Dictionary<RechargeableResourceEnum, int>();

        public int GetAmount(RechargeableResourceEnum resource)
        {
            return Amounts[resource];
        }

        public int GetAmount(ConsumableResourceEnum resource)
        {
            return resource.ToRechargeableResources().Select(resource => GetAmount(resource)).Sum();
        }

        public bool IsResourceAvailable(ConsumableResourceEnum resource, int quantity)
        {
            int actualAmount = GetAmount(resource);

            return IsResourceAvailable(resource, quantity, actualAmount);
        }

        public bool Any(Predicate<int> resourcePredicate)
        {
            return Amounts.Values.Any(count => resourcePredicate(count));
        }

        /// <summary>
        /// Reduces the provided quantity of the provided consumable resource.
        /// When reducing energy, regular energy is used up first (down to 1) then reserves are used (down to 0) then regular energy is consumed again into and past death.
        /// </summary>
        /// <param name="resource">The resource to reduce</param>
        /// <param name="quantity">The amount to reduce</param>
        /// <returns>This, for chaining</returns>
        public ResourceCount ApplyAmountReduction(ConsumableResourceEnum resource, int quantity)
        {
            switch (resource)
            {
                case ConsumableResourceEnum.Energy:
                    // Consume regular energy first, down to 1
                    int regularEnergy = GetAmount(RechargeableResourceEnum.RegularEnergy);
                    int regularEnergyToConsume = Math.Max(0, regularEnergy > quantity ? quantity : regularEnergy - 1);
                    Amounts[RechargeableResourceEnum.RegularEnergy] -= regularEnergyToConsume;
                    quantity -= regularEnergyToConsume;

                    // If more energy left to consume, consume reserve energy down to 0
                    if (quantity > 0)
                    {
                        int reserveEnergyToConsume = Math.Min(quantity, GetAmount(RechargeableResourceEnum.ReserveEnergy));
                        Amounts[RechargeableResourceEnum.ReserveEnergy] -= reserveEnergyToConsume;
                        quantity -= reserveEnergyToConsume;
                    }

                    // If yet more energy left to consume, consume regular energy
                    if (quantity > 0)
                    {
                        Amounts[RechargeableResourceEnum.RegularEnergy] -= quantity;
                    }
                    break;
                case ConsumableResourceEnum.Missile:
                    Amounts[RechargeableResourceEnum.Missile] -= quantity;
                    break;
                case ConsumableResourceEnum.Super:
                    Amounts[RechargeableResourceEnum.Super] -= quantity;
                    break;
                case ConsumableResourceEnum.PowerBomb:
                    Amounts[RechargeableResourceEnum.PowerBomb] -= quantity;
                    break;
            }

            return this;
        }

        /// <summary>
        /// Reduces the provided quantity of the provided rechargeable resource.
        /// </summary>
        /// <param name="resource">The resource to reduce</param>
        /// <param name="quantity">The amount to reduce</param>
        /// <returns>This, for chaining</returns>
        public ResourceCount ApplyAmountReduction(RechargeableResourceEnum resource, int quantity)
        {
            Amounts[resource] -= quantity;
            return this;
        }

        /// <summary>
        /// Increases the provided quantity of the provided rechargeable resource.
        /// </summary>
        /// <param name="resource">The resource to reduce</param>
        /// <param name="quantity">The amount to reduce</param>
        /// <returns>This, for chaining</returns>
        public ResourceCount ApplyAmountIncrease(RechargeableResourceEnum resource, int increase)
        {
            Amounts[resource] += increase;
            return this;
        }

        /// <summary>
        /// Subtracts the provided amount from reserve energy and adds it to regular energy.
        /// This method will naively put reserves into negatives if that's the result of the requested operation.
        /// </summary>
        /// <param name="amount">The amount of reserves to convert to regular energy</param>
        /// <returns>This, for chaining</returns>
        public ResourceCount ApplyConvertReservesToRegularEnergy(int amount)
        {
            return ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, amount)
                .ApplyAmountIncrease(RechargeableResourceEnum.RegularEnergy, amount);
        }

        /// <summary>
        /// Sets in this container the resource amounts found in the provided other container
        /// </summary>
        /// <param name="other">The ResourceCount to use amounts from</param>
        /// <returns>This, for chaining</returns>
        public ResourceCount ApplyAmounts(ReadOnlyResourceCount other)
        {
            foreach (RechargeableResourceEnum resource in Enum.GetValues<RechargeableResourceEnum>())
            {
                Amounts[resource] = other.GetAmount(resource);
            }

            return this;
        }

        /// <summary>
        /// Sets in this container the resource amount found in the provided other container for the provided resource.
        /// </summary>
        /// <param name="resource">The resource to apply an amount for</param>
        /// <param name="other">The ResourceCount to use the amount from</param>
        /// <returns>This, for chaining</returns>
        public ResourceCount ApplyAmount(RechargeableResourceEnum resource, ResourceCount other)
        {
            Amounts[resource] = other.Amounts[resource];
            return this;
        }

        /// <summary>
        /// Sets in this container the provided amount for the provided resource.
        /// </summary>
        /// <param name="resource">The resource to apply an amount for.</param>
        /// <param name="newAmount">The new amount to set.</param>
        /// <returns>This, for chaining</returns>
        public ResourceCount ApplyAmount(RechargeableResourceEnum resource, int newAmount)
        {
            Amounts[resource] = newAmount;
            return this;
        }

        /// <summary>
        /// Contains the logic, detached from any <see cref="ResourceCount"/> instance, for determining whether a given quantity of a given resource should be 
        /// considered available (can be consumed without dying) if it has a given actualAmount.
        /// </summary>
        /// <param name="resource">The resource for which we are checking for availability.</param>
        /// <param name="quantity">The quantity to check for availability</param>
        /// <param name="actualAmount">The amount of the resource that is present.</param>
        /// <returns></returns>
        public static bool IsResourceAvailable(ConsumableResourceEnum resource, int quantity, int actualAmount)
        {
            if (quantity == 0)
            {
                return true;
            }

            // The other resources can be fully spent, but for energy we don't want to go below 1
            if (resource == ConsumableResourceEnum.Energy)
            {
                return actualAmount > quantity;
            }
            else
            {
                return actualAmount >= quantity;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ReadOnlyResourceCount count)
            {
                foreach (RechargeableResourceEnum resource in Enum.GetValues<RechargeableResourceEnum>())
                {
                    if (GetAmount(resource) != count.GetAmount(resource))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            foreach (RechargeableResourceEnum resource in Enum.GetValues<RechargeableResourceEnum>())
            {
                hash.Add(Amounts[resource]);
            }
            return hash.ToHashCode();
        }
    }

    /// <summary>
    /// A read-only interface for a <see cref="ResourceCount"/>, allowing consultation without modification.
    /// </summary>
    public interface ReadOnlyResourceCount
    {
        /// <summary>
        /// Creates and returns a copy of this ResourceCount.
        /// </summary>
        /// <returns>The new copy, as a full-fledged ResourceCount</returns>
        public ResourceCount Clone();

        /// <summary>
        /// Creates and returns a new Resource count that expresses how many resources this has compared to the provided other resource count.
        /// Negative values means this has less.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public ResourceCount GetVariationWith(ReadOnlyResourceCount other);

        /// <summary>
        /// Returns the amount associated to this container for the provided rechargeable resource.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetAmount(RechargeableResourceEnum resource);

        /// <summary>
        /// Returns the amount associated to this container for the provided consumable resource.
        /// This is almost the same as getting the current amount of a rechargeable resource,
        /// except both types of energy are grouped together.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetAmount(ConsumableResourceEnum resource);

        /// <summary>
        /// Returns whether the amount in this container for the provided resource could be spent without dying.
        /// </summary>
        /// <param name="resource">The resource to check the ability to spend for</param>
        /// <param name="quantity">The amount to check the ability to spend for</param>
        /// <returns></returns>
        public bool IsResourceAvailable(ConsumableResourceEnum resource, int quantity);

        /// <summary>
        /// Returns whether any of the resource counts in this ResourceCount fulfill the provided predicate.
        /// </summary>
        /// <param name="resourcePredicate"></param>
        /// <returns></returns>
        public bool Any(Predicate<int> resourcePredicate);
    }
}
