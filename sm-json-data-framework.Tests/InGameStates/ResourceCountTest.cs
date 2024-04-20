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

            return ConsumableResourceValues().Except(new[] { new object[] { ConsumableResourceEnum.Energy } });
        }

        #region Tests for GetAmount()
        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void GetAmount_ConsumableAmmo_ReturnsCorrectValue(RechargeableResourceEnum resource)
        {
            // Given
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amount);

            // When
            int result = resourceCount.GetAmount(resource.ToConsumableResource());

            // Expect
            Assert.Equal(amount, result);

            foreach (ConsumableResourceEnum loopResource in Enum.GetValues(typeof(ConsumableResourceEnum)))
            {
                if(loopResource != resource.ToConsumableResource())
                {
                    // And when
                    result = resourceCount.GetAmount(loopResource);

                    // Expect
                    Assert.Equal(0, result);
                }
            }
        }

        [Fact]
        public void GetAmount_ConsumableEnergy_ReturnsSumOfBothEnergies()
        {
            // Given
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 6);

            // When
            int result = resourceCount.GetAmount(ConsumableResourceEnum.Energy);

            // Expect
            Assert.Equal(11, result);
        }
        #endregion

        #region Tests for ctor()
        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void Constructor_InitializesAt0(RechargeableResourceEnum resource)
        {
            // When
            ResourceCount resourceCount = new ResourceCount();

            // Expect
            Assert.Equal(0, resourceCount.GetAmount(resource));
        }
        #endregion

        #region Tests for ApplyAmount()
        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ApplyAmount_SetsAmount(RechargeableResourceEnum resource)
        {
            // Given
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();

            // When
            resourceCount.ApplyAmount(resource, amount);

            // Expect
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
            // Given
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            ResourceCount otherResourceCount = new ResourceCount();
            otherResourceCount.ApplyAmount(resource, amount);

            // When
            resourceCount.ApplyAmount(resource, otherResourceCount);

            // Expect
            Assert.Equal(otherResourceCount.GetAmount(resource), resourceCount.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != resource)
                {
                    Assert.Equal(0, resourceCount.GetAmount(otherResource));
                }
            }
        }
        #endregion

        #region Tests for ApplyAmounts()
        [Fact]
        public void ApplyAmounts_AppliesCorrectValues()
        {
            // Given
            ResourceCount resourceCount = new ResourceCount();
            ResourceCount otherResourceCount = new ResourceCount();
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                otherResourceCount.ApplyAmount(resource, value++);
            }

            // When
            resourceCount.ApplyAmounts(otherResourceCount);

            // Expect
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(otherResourceCount.GetAmount(resource), resourceCount.GetAmount(resource));
            }
        }
        #endregion

        #region Tests for ApplyAmountIncrease()
        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ApplyAmountIncrease_SetsAmount(RechargeableResourceEnum resource)
        {
            // Given
            int initialAmount = 2;
            int addedAmount = 5;
            ResourceCount resourceCount = new ResourceCount();

            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(loopResource, initialAmount);
            }

            // When
            resourceCount.ApplyAmountIncrease(resource, addedAmount);

            // Expect
            Assert.Equal(initialAmount + addedAmount, resourceCount.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != resource)
                {
                    Assert.Equal(initialAmount, resourceCount.GetAmount(otherResource));
                }
            }
        }
        #endregion

        #region Tests for ApplyAmountReduction()
        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void ApplyAmountReduction_Ammo_SetsAmount(RechargeableResourceEnum resource)
        {
            // Given
            int initialAmount = 5;
            int removedAmount = 2;
            ResourceCount resourceCount = new ResourceCount();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(loopResource, initialAmount);
            }

            // When
            resourceCount.ApplyAmountReduction(resource.ToConsumableResource(), removedAmount);

            // Expect
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
            // Given
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

            // When
            resourceCount.ApplyAmountReduction(ConsumableResourceEnum.Energy, removedAmount);

            // Expect
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
            // Given
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

            // When
            resourceCount.ApplyAmountReduction(ConsumableResourceEnum.Energy, removedAmount);

            // Expect
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
            // Given
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 10);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 10);

            // When
            resourceCount.ApplyAmountReduction(ConsumableResourceEnum.Energy, 12);

            // Expect
            Assert.Equal(1, resourceCount.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(7, resourceCount.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Fact]
        public void ApplyAmountReduction_MixedEnergy_ConsumesReservesBeforeGoingTo0Regular()
        {
            // Given
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 10);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 10);

            // When
            resourceCount.ApplyAmountReduction(ConsumableResourceEnum.Energy, 19);

            // Expect
            Assert.Equal(1, resourceCount.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(0, resourceCount.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Fact]
        public void ApplyAmountReduction_MixedEnergy_ConsumesReservesBeforeGoingToNegativeRegular()
        {
            // Given
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 10);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 10);

            // When
            resourceCount.ApplyAmountReduction(ConsumableResourceEnum.Energy, 22);

            // Expect
            Assert.Equal(-2, resourceCount.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(0, resourceCount.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }
        #endregion

        #region Tests for IsResourceAvailable()
        [Theory]
        [MemberData(nameof(ConsumableResourceValues))]
        public void IsResourceAvailable_Requesting0_ReturnsTrue(ConsumableResourceEnum resource)
        {
            // Given
            ResourceCount resourceCount = new ResourceCount();

            // When
            bool result = resourceCount.IsResourceAvailable(resource, 0);

            // Expect
            Assert.True(result);
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingExactPresentAmount_Ammo_ReturnsTrue(RechargeableResourceEnum resource)
        {
            // Given
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amount);

            // When
            bool result = resourceCount.IsResourceAvailable(resource.ToConsumableResource(), amount);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingExactPresentAmount_Energy_ReturnsFalse()
        {
            // Given
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amount);

            // When
            bool result = resourceCount.IsResourceAvailable(ConsumableResourceEnum.Energy, amount);

            // Expect
            // X energy is not available to spend if you have exactly X energy, because you'd die
            Assert.False(result);
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Ammo_ReturnsTrue(RechargeableResourceEnum resource)
        {
            // Given
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amountToRequest + 1);

            // When
            bool result = resourceCount.IsResourceAvailable(resource.ToConsumableResource(), amountToRequest);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Energy_ReturnsTrue()
        {
            // Given
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amountToRequest + 1);

            // When
            bool result = resourceCount.IsResourceAvailable(ConsumableResourceEnum.Energy, amountToRequest);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_EnergyMixOfReserveAndNormal_ReturnsTrue()
        {
            // Given
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 3);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 3);

            // When
            bool result = resourceCount.IsResourceAvailable(ConsumableResourceEnum.Energy, 5);

            // Expect
            Assert.True(result);
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Ammo_ReturnsFalse(RechargeableResourceEnum resource)
        {
            // Given
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amountToRequest - 1);

            // When
            bool result = resourceCount.IsResourceAvailable(resource.ToConsumableResource(), amountToRequest);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Energy_ReturnsFalse()
        {
            // Given
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amountToRequest - 1);

            // When
            bool result = resourceCount.IsResourceAvailable(ConsumableResourceEnum.Energy, amountToRequest);

            // Expect
            Assert.False(result);
        }
        #endregion

        #region Tests for Clone()
        [Fact]
        public void Clone_CopiesCorrectly()
        {
            // Given
            ResourceCount resourceCount = new ResourceCount();
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(resource, value++);
            }

            // When
            ResourceCount clone = resourceCount.Clone();

            // Expect
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(resourceCount.GetAmount(resource), clone.GetAmount(resource));
            }
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            // Given
            ResourceCount resourceCount = new ResourceCount();

            // When
            ResourceCount clone = resourceCount.Clone();

            // Subsequently given
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                clone.ApplyAmount(resource, value++);
            }

            // Expect
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(0, resourceCount.GetAmount(resource));
            }
        }
        #endregion
    }
}
