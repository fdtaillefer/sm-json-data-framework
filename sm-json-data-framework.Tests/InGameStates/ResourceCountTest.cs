using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace sm_json_data_framework.Tests.InGameStates
{
    public class ResourceCountTest
    {
        /// <summary>
        /// Returns all values of <see cref="RechargeableResourceEnum"/> in a format that can be used by <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> RechargeableResourceValues()
        {

            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                yield return new object[] { resource };
            }
        }

        /// <summary>
        /// Returns all values of <see cref="ConsumableResourceEnum"/> in a format that can be used by <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> ConsumableResourceValues()
        {

            foreach (ConsumableResourceEnum resource in Enum.GetValues(typeof(ConsumableResourceEnum)))
            {
                yield return new object[] { resource };
            }
        }

        /// <summary>
        /// Returns all values of <see cref="ConsumableResourceEnum"/> that map one-to-one with a <see cref="RechargeableResourceEnum"/> in a format that can be used by <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> ConsumableOneToOneResourceValues()
        {

            return ConsumableResourceValues().Except(new[] { new object[] { ConsumableResourceEnum.ENERGY } });
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void GetAmount_ConsumableAmmo_ReturnsCorrectValue(RechargeableResourceEnum resource)
        {
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amount);
            Assert.Equal(amount, resourceCount.GetAmount(resource.ToConsumableResource()));

            foreach (ConsumableResourceEnum loopResource in Enum.GetValues(typeof(ConsumableResourceEnum)))
            {
                if(loopResource != resource.ToConsumableResource())
                {
                    Assert.Equal(0, resourceCount.GetAmount(loopResource));
                }
            }
        }

        [Fact]
        public void GetAmount_ConsumableEnergy_ReturnsSumOfBothEnergies()
        {
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 6);

            Assert.Equal(11, resourceCount.GetAmount(ConsumableResourceEnum.ENERGY));
        }

        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void Constructor_InitializesAt0(RechargeableResourceEnum resource)
        {
            ResourceCount resourceCount = new ResourceCount();

            Assert.Equal(0, resourceCount.GetAmount(resource));
        }

        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ApplyAmount_SetsAmount(RechargeableResourceEnum resource)
        {
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();

            resourceCount.ApplyAmount(resource, amount);

            Assert.Equal(amount, resourceCount.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != resource)
                {
                    Assert.Equal(0, resourceCount.GetAmount(otherResource));
                }
            }
        }

        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ApplyAmount_FromOther_SetsAmount(RechargeableResourceEnum resource)
        {
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            ResourceCount otherResourceCount = new ResourceCount();
            otherResourceCount.ApplyAmount(resource, amount);

            resourceCount.ApplyAmount(resource, otherResourceCount);

            Assert.Equal(otherResourceCount.GetAmount(resource), resourceCount.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != resource)
                {
                    Assert.Equal(0, resourceCount.GetAmount(otherResource));
                }
            }
        }

        [Fact]
        public void ApplyAmounts_AppliesCorrectValues()
        {
            ResourceCount resourceCount = new ResourceCount();
            ResourceCount otherResourceCount = new ResourceCount();
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                otherResourceCount.ApplyAmount(resource, value++);
            }

            resourceCount.ApplyAmounts(otherResourceCount);

            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(otherResourceCount.GetAmount(resource), resourceCount.GetAmount(resource));
            }
        }

        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ApplyAmountIncrease_SetsAmount(RechargeableResourceEnum resource)
        {
            int initialAmount = 2;
            int addedAmount = 5;
            ResourceCount resourceCount = new ResourceCount();

            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(loopResource, initialAmount);
            }

            resourceCount.ApplyAmountIncrease(resource, addedAmount);

            Assert.Equal(initialAmount + addedAmount, resourceCount.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != resource)
                {
                    Assert.Equal(initialAmount, resourceCount.GetAmount(otherResource));
                }
            }
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void ApplyAmountReduction_Ammo_SetsAmount(RechargeableResourceEnum resource)
        {
            int initialAmount = 5;
            int removedAmount = 2;
            ResourceCount resourceCount = new ResourceCount();

            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(loopResource, initialAmount);
            }

            resourceCount.ApplyAmountReduction(resource.ToConsumableResource(), removedAmount);

            Assert.Equal(initialAmount - removedAmount, resourceCount.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != resource)
                {
                    Assert.Equal(initialAmount, resourceCount.GetAmount(otherResource));
                }
            }
        }

        [Fact]
        public void ApplyAmountReduction_RegularEnergy_SetsAmount()
        {
            int initialAmount = 5;
            int removedAmount = 2;
            ResourceCount resourceCount = new ResourceCount();

            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                // Don't put any reserve energy in, we're checking regular energy
                if (loopResource != RechargeableResourceEnum.ReserveEnergy)
                {
                    resourceCount.ApplyAmount(loopResource, initialAmount);
                }
            }

            resourceCount.ApplyAmountReduction(ConsumableResourceEnum.ENERGY, removedAmount);

            Assert.Equal(initialAmount - removedAmount, resourceCount.GetAmount(RechargeableResourceEnum.RegularEnergy));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != RechargeableResourceEnum.RegularEnergy && otherResource != RechargeableResourceEnum.ReserveEnergy)
                {
                    Assert.Equal(initialAmount, resourceCount.GetAmount(otherResource));
                }
            }
        }

        [Fact]
        public void ApplyAmountReduction_ReserveEnergy_SetsAmount()
        {
            int initialAmount = 5;
            int removedAmount = 2;
            ResourceCount resourceCount = new ResourceCount();

            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                // Don't put any regular energy in, we're checking reserve energy
                if (loopResource != RechargeableResourceEnum.RegularEnergy)
                {
                    resourceCount.ApplyAmount(loopResource, initialAmount);
                }
            }

            resourceCount.ApplyAmountReduction(ConsumableResourceEnum.ENERGY, removedAmount);

            Assert.Equal(initialAmount - removedAmount, resourceCount.GetAmount(RechargeableResourceEnum.ReserveEnergy));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != RechargeableResourceEnum.RegularEnergy && otherResource != RechargeableResourceEnum.ReserveEnergy)
                {
                    Assert.Equal(initialAmount, resourceCount.GetAmount(otherResource));
                }
            }
        }

        [Fact]
        public void ApplyAmountReduction_MixedEnergy_ConsumesRegularEnergyFirst()
        {
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 10);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 10);

            resourceCount.ApplyAmountReduction(ConsumableResourceEnum.ENERGY, 12);

            Assert.Equal(1, resourceCount.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(7, resourceCount.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Fact]
        public void ApplyAmountReduction_MixedEnergy_ConsumesReservesBeforeGoingTo0Regular()
        {
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 10);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 10);

            resourceCount.ApplyAmountReduction(ConsumableResourceEnum.ENERGY, 19);

            Assert.Equal(1, resourceCount.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(0, resourceCount.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Fact]
        public void ApplyAmountReduction_MixedEnergy_ConsumesReservesBeforeGoingToNegativeRegular()
        {
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 10);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 10);

            resourceCount.ApplyAmountReduction(ConsumableResourceEnum.ENERGY, 22);

            Assert.Equal(-2, resourceCount.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(0, resourceCount.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Theory]
        [MemberData(nameof(ConsumableResourceValues))]
        public void IsResourceAvailable_Requesting0_ReturnsTrue(ConsumableResourceEnum resource)
        {
            ResourceCount resourceCount = new ResourceCount();

            Assert.True(resourceCount.IsResourceAvailable(resource, 0));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingExactPresentAmount_Ammo_ReturnsTrue(RechargeableResourceEnum resource)
        {
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amount);

            Assert.True(resourceCount.IsResourceAvailable(resource.ToConsumableResource(), amount));
        }

        [Fact]
        public void IsResourceAvailable_RequestingExactPresentAmount_Energy_ReturnsFalse()
        {
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amount);

            // X energy is not available to spend if you have exactly X energy, because you'd die
            Assert.False(resourceCount.IsResourceAvailable(ConsumableResourceEnum.ENERGY, amount));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Ammo_ReturnsTrue(RechargeableResourceEnum resource)
        {
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amountToRequest + 1);

            Assert.True(resourceCount.IsResourceAvailable(resource.ToConsumableResource(), amountToRequest));
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Energy_ReturnsTrue()
        {
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amountToRequest + 1);

            Assert.True(resourceCount.IsResourceAvailable(ConsumableResourceEnum.ENERGY, amountToRequest));
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_EnergyMixOfReserveAndNormal_ReturnsTrue()
        {
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 3);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 3);

            Assert.True(resourceCount.IsResourceAvailable(ConsumableResourceEnum.ENERGY, 5));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Ammo_ReturnsFalse(RechargeableResourceEnum resource)
        {
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amountToRequest - 1);

            Assert.False(resourceCount.IsResourceAvailable(resource.ToConsumableResource(), amountToRequest));
        }

        [Fact]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Energy_ReturnsFalse()
        {
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amountToRequest - 1);

            Assert.False(resourceCount.IsResourceAvailable(ConsumableResourceEnum.ENERGY, amountToRequest));
        }

        [Fact]
        public void Clone_CopiesCorrectly()
        {
            ResourceCount resourceCount = new ResourceCount();
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(resource, value++);
            }

            ResourceCount clone = resourceCount.Clone();
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(resourceCount.GetAmount(resource), clone.GetAmount(resource));
            }
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            ResourceCount resourceCount = new ResourceCount();
            ResourceCount clone = resourceCount.Clone();
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                clone.ApplyAmount(resource, value++);
            }

            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(0, resourceCount.GetAmount(resource));
            }
        }
    }
}
