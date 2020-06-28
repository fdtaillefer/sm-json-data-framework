using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.InGameStates
{
    /// <summary>
    /// Contains values for all rechargeable resource types. The context of those values is not known by this container.
    /// </summary>
    public class ResourceCount
    {
        public ResourceCount()
        {
            foreach (RechargeableResourceEnum currentResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Amounts.Add(currentResource, 0);
            }
        }

        public ResourceCount(ResourceCount other)
        {
            ApplyAmounts(other);
        }

        /// <summary>
        /// Creates and returns a copy of this ResourceCount.
        /// </summary>
        /// <returns></returns>
        public ResourceCount Clone()
        {
            return new ResourceCount(this);
        }

        /// <summary>
        /// Creates and returns a copy of this ResourceCount, with positive and negative values flipped.
        /// </summary>
        /// <returns></returns>
        public ResourceCount CloneNegative()
        {
            ResourceCount clone = Clone();
            foreach (RechargeableResourceEnum currentResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                clone.ApplyAmount(currentResource, clone.GetAmount(currentResource) * -1);
            }

            return clone;
        }

        private IDictionary<RechargeableResourceEnum, int> Amounts { get; set; } = new Dictionary<RechargeableResourceEnum, int>();

        /// <summary>
        /// Returns the amount associated to this container for the provided rechargeable resource.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetAmount(RechargeableResourceEnum resource)
        {
            return Amounts[resource];
        }

        /// <summary>
        /// Returns the amount associated to this container for the provided consumable resource.
        /// This is almost the same as getting the current amount of a rechargeable resource,
        /// except both types of energy are grouped together.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetAmount(ConsumableResourceEnum resource)
        {
            return resource.ToRechargeableResources().Select(resource => GetAmount(resource)).Sum();
        }

        /// <summary>
        /// Returns whether the amount in this container for the provided resource could be spent without dying.
        /// </summary>
        /// <param name="resource">The resource to check the ability to spend for</param>
        /// <param name="quantity">The amount to check the ability to spend for</param>
        /// <returns></returns>
        public bool IsResourceAvailable(ConsumableResourceEnum resource, int quantity)
        {
            if (quantity == 0)
            {
                return true;
            }

            // The other resources can be fully spent, but for energy we don't want to go below 1
            if (resource == ConsumableResourceEnum.ENERGY)
            {
                return GetAmount(resource) > quantity;
            }
            else
            {
                return GetAmount(resource) >= quantity;
            }
        }

        /// <summary>
        /// Returns whether any of the resource counts in this ResourceCount fulfill the provided predicate.
        /// </summary>
        /// <param name="resourcePredicate"></param>
        /// <returns></returns>
        public bool Any(Predicate<int> resourcePredicate)
        {
            return Amounts.Values.Any(count => resourcePredicate(count));
        }

        /// <summary>
        /// Reduces the provided quantity of the provided consumable resource.
        /// When reducing energy, regular energy is used up first (down to 1) then reserves are used.
        /// </summary>
        /// <param name="resource">The resource to reduce</param>
        /// <param name="quantity">The amount to reduce</param>
        public void ApplyAmountReduction(ConsumableResourceEnum resource, int quantity)
        {
            switch (resource)
            {
                case ConsumableResourceEnum.ENERGY:
                    // Consume regular energy first, down to 1
                    int regularEnergy = GetAmount(RechargeableResourceEnum.RegularEnergy);
                    int regularEnergyToConsume = regularEnergy > quantity ? quantity : regularEnergy - 1;
                    Amounts[RechargeableResourceEnum.RegularEnergy] -= regularEnergyToConsume;
                    quantity -= regularEnergyToConsume;
                    if (quantity > 0)
                    {
                        Amounts[RechargeableResourceEnum.ReserveEnergy] -= quantity;
                    }
                    break;
                case ConsumableResourceEnum.MISSILE:
                    Amounts[RechargeableResourceEnum.Missile] -= quantity;
                    break;
                case ConsumableResourceEnum.SUPER:
                    Amounts[RechargeableResourceEnum.Super] -= quantity;
                    break;
                case ConsumableResourceEnum.POWER_BOMB:
                    Amounts[RechargeableResourceEnum.PowerBomb] -= quantity;
                    break;
            }
        }

        /// <summary>
        /// Increases the provided quantity of the provided rechargeable resource.
        /// </summary>
        /// <param name="resource">The resource to reduce</param>
        /// <param name="quantity">The amount to reduce</param>
        public void ApplyAmountIncrease(RechargeableResourceEnum resource, int increase)
        {
            Amounts[resource] += increase;
        }

        /// <summary>
        /// Sets in this container the resource amounts found in the provided other container
        /// </summary>
        /// <param name="other">The ResourceCount to use amounts from</param>
        public void ApplyAmounts(ResourceCount other)
        {
            Amounts = new Dictionary<RechargeableResourceEnum, int>(other.Amounts);
        }

        /// <summary>
        /// Sets in this container the resource amount found in the provided other container for the provided resource.
        /// </summary>
        /// <param name="resource">The resource to apply an amount for</param>
        /// <param name="other">The ResourceCount to use the amount from</param>
        public void ApplyAmount(RechargeableResourceEnum resource, ResourceCount other)
        {
            Amounts[resource] = other.Amounts[resource];
        }

        /// <summary>
        /// Sets in this container the provided amount for the provided resource.
        /// </summary>
        /// <param name="resource">The resource to apply an amount for.</param>
        /// <param name="newAmount">The new amount to set.</param>
        public void ApplyAmount(RechargeableResourceEnum resource, int newAmount)
        {
            Amounts[resource] = newAmount;
        }
    }
}
