using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.InGameStates
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

        [Theory]
        [MemberData(nameof(ConsumableResourceValues))]
        public void GetAmount_RechargeableResource_ReturnsCorrectValue(RechargeableResourceEnum resource)
        {
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(resource, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(resource, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            Assert.Equal(7, aggregateResourceCount.GetAmount(resource));
        }

        [Theory]
        [MemberData(nameof(ConsumableResourceValues))]
        public void GetAmount_RechargeableResource_AfterInternalReferencesMutated_UsesNewState(RechargeableResourceEnum resource)
        {
            ResourceCount resourceCount1 = new ResourceCount();
            ResourceCount resourceCount2 = new ResourceCount();
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            resourceCount1.ApplyAmount(resource, 5);
            resourceCount2.ApplyAmount(resource, 2);
            Assert.Equal(7, aggregateResourceCount.GetAmount(resource));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void GetAmount_ConsumableAmmo_ReturnsCorrectValue(RechargeableResourceEnum resource)
        {
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
                    Assert.Equal(0, aggregateResourceCount.GetAmount(loopResource));
                }
            }
        }

        [Fact]
        public void GetAmount_ConsumableEnergy_ReturnsSumOfBothEnergies()
        {
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 2);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 6)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 3);

            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            Assert.Equal(16, aggregateResourceCount.GetAmount(ConsumableResourceEnum.ENERGY));
        }

        [Theory]
        [MemberData(nameof(ConsumableResourceValues))]
        public void IsResourceAvailable_Requesting0_ReturnsTrue(ConsumableResourceEnum resource)
        {
            ResourceCount resourceCount1 = new ResourceCount();
            ResourceCount resourceCount2 = new ResourceCount();
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            Assert.True(aggregateResourceCount.IsResourceAvailable(resource, 0));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingExactPresentAmount_Ammo_ReturnsTrue(RechargeableResourceEnum resource)
        {
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(resource, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(resource, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            Assert.True(aggregateResourceCount.IsResourceAvailable(resource.ToConsumableResource(), 7));
        }

        [Fact]
        public void IsResourceAvailable_RequestingExactPresentAmount_Energy_ReturnsFalse()
        {
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            // X energy is not available to spend if you have exactly X energy, because you'd die
            Assert.False(aggregateResourceCount.IsResourceAvailable(ConsumableResourceEnum.ENERGY, 7));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Ammo_ReturnsTrue(RechargeableResourceEnum resource)
        {
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(resource, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(resource, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            Assert.True(aggregateResourceCount.IsResourceAvailable(resource.ToConsumableResource(), 6));
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Energy_ReturnsTrue()
        {
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            Assert.True(aggregateResourceCount.IsResourceAvailable(ConsumableResourceEnum.ENERGY, 6));
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_EnergyMixOfReserveAndNormal_ReturnsTrue()
        {
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 4);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 3);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            Assert.True(aggregateResourceCount.IsResourceAvailable(ConsumableResourceEnum.ENERGY, 13));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Ammo_ReturnsFalse(RechargeableResourceEnum resource)
        {
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(resource, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(resource, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            Assert.False(aggregateResourceCount.IsResourceAvailable(resource.ToConsumableResource(), 8));
        }

        [Fact]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Energy_ReturnsFalse()
        {
            ResourceCount resourceCount1 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 5);
            ResourceCount resourceCount2 = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2);
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            Assert.False(aggregateResourceCount.IsResourceAvailable(ConsumableResourceEnum.ENERGY, 8));
        }

        [Fact]
        public void Clone_CopiesCorrectly()
        {
            ResourceCount resourceCount1 = new ResourceCount();
            ResourceCount resourceCount2 = new ResourceCount();

            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount1.ApplyAmount(resource, value++);
                resourceCount2.ApplyAmount(resource, value++);
            }
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);

            ResourceCount clone = aggregateResourceCount.Clone();
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(resourceCount1.GetAmount(resource) + resourceCount2.GetAmount(resource), clone.GetAmount(resource));
            }
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            ResourceCount resourceCount1 = new ResourceCount();
            ResourceCount resourceCount2 = new ResourceCount();
            AggregateResourceCount aggregateResourceCount = new AggregateResourceCount(resourceCount1, resourceCount2);
            ResourceCount clone = aggregateResourceCount.Clone();

            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount1.ApplyAmount(resource, value++);
                resourceCount2.ApplyAmount(resource, value++);
            }

            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(0, clone.GetAmount(resource));
            }
        }
    }
}
