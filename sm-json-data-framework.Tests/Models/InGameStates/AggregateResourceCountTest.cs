using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.InGameStates
{
    public class AggregateResourceCountTest
    {
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

        #region Tests for GetAmount(RechargeableResourceEnum)
        [Theory]
        [MemberData(nameof(ConsumableResourceValues))]
        public void GetAmount_RechargeableResource_ReturnsCorrectValue(RechargeableResourceEnum resource)
        {
            // Given
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(resource, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(resource, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            int result = aggregateResourceCount.GetAmount(resource);

            // Expect
            Assert.Equal(7, result);
        }

        [Theory]
        [MemberData(nameof(ConsumableResourceValues))]
        public void GetAmount_RechargeableResource_AfterInternalReferencesMutated_UsesNewState(RechargeableResourceEnum resource)
        {
            // Given
            ResourceCount resourceCount1 = new ResourceCount();
            ResourceCount resourceCount2 = new ResourceCount();
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);
            resourceCount1.ApplyAmount(resource, 5);
            resourceCount2.ApplyAmount(resource, 2);

            // When
            int result = aggregateResourceCount.GetAmount(resource);

            // Expect
            Assert.Equal(7, result);
        }
        #endregion

        #region Tests for GetAmount(ConsumableResourceEnum)
        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void GetAmount_ConsumableAmmo_ReturnsCorrectValue(RechargeableResourceEnum resource)
        {
            // Given
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(resource, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(resource, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            Assert.Equal(7, aggregateResourceCount.GetAmount(resource.ToConsumableResource()));
            foreach (ConsumableResourceEnum loopResource in Enum.GetValues(typeof(ConsumableResourceEnum)))
            {
                if (loopResource != resource.ToConsumableResource())
                {
                    // When
                    int result = aggregateResourceCount.GetAmount(loopResource);

                    // Expect
                    Assert.Equal(0, result);
                }
            }
        }

        [Fact]
        public void GetAmount_ConsumableEnergy_ReturnsSumOfBothEnergies()
        {
            // Given
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 2);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 6)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 3);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            int result = aggregateResourceCount.GetAmount(ConsumableResourceEnum.Energy);

            // Expect
            Assert.Equal(16, result);
        }
        #endregion

        #region Tests for IsResourceAvailable()
        [Theory]
        [MemberData(nameof(ConsumableResourceValues))]
        public void IsResourceAvailable_Requesting0_ReturnsTrue(ConsumableResourceEnum resource)
        {
            // Given
            ResourceCount resourceCount1 = new ResourceCount();
            ResourceCount resourceCount2 = new ResourceCount();
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            bool result = aggregateResourceCount.IsResourceAvailable(resource, 0);

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
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(resource, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(resource, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            bool result = aggregateResourceCount.IsResourceAvailable(resource.ToConsumableResource(), 7);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingExactPresentAmount_Energy_ReturnsFalse()
        {
            // Given
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            bool result = aggregateResourceCount.IsResourceAvailable(ConsumableResourceEnum.Energy, 7);

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
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(resource, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(resource, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            bool result = aggregateResourceCount.IsResourceAvailable(resource.ToConsumableResource(), 6);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Energy_ReturnsTrue()
        {
            // Given
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            bool result = aggregateResourceCount.IsResourceAvailable(ConsumableResourceEnum.Energy, 6);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_EnergyMixOfReserveAndNormal_ReturnsTrue()
        {
            // Given
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 4);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 3);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            bool result = aggregateResourceCount.IsResourceAvailable(ConsumableResourceEnum.Energy, 13);

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
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(resource, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(resource, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            bool result = aggregateResourceCount.IsResourceAvailable(resource.ToConsumableResource(), 8);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Energy_ReturnsFalse()
        {
            // Given
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            bool result = aggregateResourceCount.IsResourceAvailable(ConsumableResourceEnum.Energy, 8);

            // Expect
            Assert.False(result);
        }
        #endregion

        #region Tests for Clone()
        [Fact]
        public void Clone_CopiesCorrectly()
        {
            // Given
            ResourceCount resourceCount1 = new ResourceCount();
            ResourceCount resourceCount2 = new ResourceCount();

            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount1.ApplyAmount(resource, value++);
                resourceCount2.ApplyAmount(resource, value++);
            }
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            ResourceCount clone = aggregateResourceCount.Clone();

            // Expect
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(resourceCount1.GetAmount(resource) + resourceCount2.GetAmount(resource), clone.GetAmount(resource));
            }
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            // Given
            ResourceCount resourceCount1 = new ResourceCount();
            ResourceCount resourceCount2 = new ResourceCount();
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // When
            ResourceCount clone = aggregateResourceCount.Clone();

            // Subsequently given
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount1.ApplyAmount(resource, value++);
                resourceCount2.ApplyAmount(resource, value++);
            }

            // Expect
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(0, clone.GetAmount(resource));
            }
        }
        #endregion
    }
}
