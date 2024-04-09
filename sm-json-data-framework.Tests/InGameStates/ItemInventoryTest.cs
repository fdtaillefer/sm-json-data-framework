using sm_json_data_framework.Models;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.InGameStates
{
    public class ItemInventoryTest
    {
        /// <summary>
        /// Returns a series of items of different kinds, compatible for use with <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> DifferentObjectTypeSamples()
        {
            return new List<object[]> { 
                // Non-consumable item
                new object[] { Model.Items[SuperMetroidModel.VARIA_SUIT_NAME] },
                // Expansion item
                new object[] { Model.Items[SuperMetroidModel.MISSILE_NAME] },
                // Not even an in-game item
                new object[] { Model.Items[SuperMetroidModel.POWER_BEAM_NAME] }
            };
        }

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
        /// Returns all values of <see cref="RechargeableResourceEnum"/> combined with the <see cref="ExpansionItem"/> that adds to the max amount of that resource,
        /// in a format that can be used by <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> RechargeableResourceValuesWithExpansionItem()
        {

            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                yield return new object[] { resource,
                    resource switch
                    {
                        RechargeableResourceEnum.RegularEnergy => Model.Items[SuperMetroidModel.ENERGY_TANK_NAME],
                        RechargeableResourceEnum.ReserveEnergy => Model.Items[SuperMetroidModel.RESERVE_TANK_NAME],
                        RechargeableResourceEnum.Missile=> Model.Items[SuperMetroidModel.MISSILE_NAME],
                        RechargeableResourceEnum.Super=> Model.Items[SuperMetroidModel.SUPER_NAME],
                        RechargeableResourceEnum.PowerBomb=> Model.Items[SuperMetroidModel.POWER_BOMB_NAME],
                        _ => throw new NotImplementedException() }
                };
            }
        }

        // Use a static model to read it only once.
        private static SuperMetroidModel Model { get; set; } = ModelReader.ReadModel();

        [Fact]
        public void ApplyAddItem_NonConsumableItem_AddsItem()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(varia);

            Assert.Contains(varia.Name, inventory.NonConsumableItems.Keys);
            Assert.Same(varia, inventory.NonConsumableItems[varia.Name]);
            Assert.Empty(inventory.ExpansionItems);
        }

        [Fact]
        public void ApplyAddItem_ExpansionItem_AddsItem()
        {
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(missilePack);

            Assert.Contains(missilePack.Name, inventory.ExpansionItems.Keys);
            Assert.Same(missilePack, inventory.ExpansionItems[missilePack.Name].item);
            Assert.Equal(1, inventory.ExpansionItems[missilePack.Name].count);
            Assert.Empty(inventory.NonConsumableItems);
        }

        [Fact]
        public void ApplyAddItem_ExpansionItem_ItemAlreadyPresent_AddsToCount()
        {
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(missilePack);

            inventory.ApplyAddItem(missilePack);

            Assert.Contains(missilePack.Name, inventory.ExpansionItems.Keys);
            Assert.Same(missilePack, inventory.ExpansionItems[missilePack.Name].item);
            Assert.Equal(2, inventory.ExpansionItems[missilePack.Name].count);
            Assert.Empty(inventory.NonConsumableItems);
        }

        [Fact]
        public void ApplyDisableItem_NonConsumableItem_DisablesItem()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(varia);

            inventory.ApplyDisableItem(varia);
            Assert.Contains(varia.Name, inventory.DisabledItemNames);
        }

        [Fact]
        public void ApplyDisableItem_NonConsumableItem_ItemNotInInventory_DoesNothing()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            inventory.ApplyDisableItem(varia);
            Assert.Empty(inventory.DisabledItemNames);
        }

        [Fact]
        public void ApplyDisableItem_ExpansionItem_DoesNothing()
        {
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(missilePack);

            inventory.ApplyDisableItem(missilePack);
            Assert.Empty(inventory.DisabledItemNames);
        }

        [Fact]
        public void ApplyEnableItem_NonConsumableItem_EnablesItem()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(varia);
            inventory.ApplyDisableItem(varia);

            inventory.ApplyEnableItem(varia);
            Assert.Empty(inventory.DisabledItemNames);
        }

        [Fact]
        public void ApplyEnableItem_NonConsumableItem_ItemNotDisabled_DoesNothing()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(varia);

            inventory.ApplyEnableItem(varia);
            Assert.Empty(inventory.DisabledItemNames);
        }

        [Fact]
        public void ApplyEnableItem_NonConsumableItem_ItemNotInInventory_DoesNothing()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            inventory.ApplyEnableItem(varia);
            Assert.Empty(inventory.DisabledItemNames);
        }

        [Fact]
        public void ExceptIn_BuildsProperDifference()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item gravity = Model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            Item speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            Item superPack = Model.Items[SuperMetroidModel.SUPER_NAME];
            Item powerBombPack = Model.Items[SuperMetroidModel.POWER_BOMB_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddItem(varia)
                .ApplyAddItem(gravity)
                .ApplyAddItem(missilePack)
                .ApplyAddItem(missilePack)
                .ApplyAddItem(missilePack)
                .ApplyAddItem(superPack);

            ItemInventory otherInventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddItem(speedBooster)
                .ApplyAddItem(gravity)
                .ApplyAddItem(missilePack)
                .ApplyAddItem(superPack)
                .ApplyAddItem(superPack)
                .ApplyAddItem(superPack)
                .ApplyAddItem(powerBombPack);

            ItemInventory result = inventory.ExceptIn(otherInventory);
            Assert.Contains(varia.Name, result.NonConsumableItems.Keys);
            Assert.DoesNotContain(gravity.Name, result.NonConsumableItems.Keys);
            Assert.DoesNotContain(speedBooster.Name, result.NonConsumableItems.Keys);
            Assert.Contains(missilePack.Name, result.ExpansionItems.Keys);
            Assert.Equal(2, result.ExpansionItems[missilePack.Name].count);
            Assert.DoesNotContain(superPack.Name, result.ExpansionItems.Keys);
            Assert.DoesNotContain(powerBombPack.Name, result.ExpansionItems.Keys);
        }

        [Fact]
        public void ExceptIn_ReturnsSeparateState()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            ItemInventory otherInventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            ItemInventory result = inventory.ExceptIn(otherInventory);
            inventory.ApplyAddItem(varia).ApplyAddItem(missilePack).ApplyDisableItem(varia);
            otherInventory.ApplyAddItem(varia).ApplyAddItem(missilePack).ApplyDisableItem(varia);

            Assert.Empty(result.NonConsumableItems);
            Assert.Empty(result.ExpansionItems);
            Assert.Empty(result.DisabledItemNames);
        }

        [Theory]
        [MemberData(nameof(DifferentObjectTypeSamples))]
        public void HasItem_ItemIsPresent_ReturnsTrue(Item item)
        {
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddItem(item);
            Assert.True(inventory.HasItem(item));
        }

        [Theory]
        [MemberData(nameof(DifferentObjectTypeSamples))]
        public void HasItem_ItemNotPresent_ReturnsFalse(Item item)
        {
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            Assert.False(inventory.HasItem(item));
        }

        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ResourceMaximums_RechargeableResource_NoExpansions_ReturnsBaseValue(RechargeableResourceEnum resource)
        {
            int amount = 5;
            ItemInventory inventory = new ItemInventory(new ResourceCount().ApplyAmount(resource, amount));

            Assert.Equal(amount, inventory.ResourceMaximums.GetAmount(resource));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void ResourceMaximums_ConsumableAmmo_NoExpansions_ReturnsBaseValue(RechargeableResourceEnum resource)
        {
            int amount = 5;
            ItemInventory inventory = new ItemInventory(new ResourceCount().ApplyAmount(resource, amount));

            Assert.Equal(amount, inventory.ResourceMaximums.GetAmount(resource.ToConsumableResource()));
        }

        [Fact]
        public void ResourceMaximums_ConsumableEnergy_NoExpansions_ReturnsBaseValuesSum()
        {
            ItemInventory inventory = new ItemInventory(
                new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2).ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 4)
            );

            Assert.Equal(6, inventory.ResourceMaximums.GetAmount(ConsumableResourceEnum.ENERGY));
        }

        [Theory]
        [MemberData(nameof(RechargeableResourceValuesWithExpansionItem))]
        public void ResourceMaximums_RechargeableResource_WithExpansions_ReturnsSumOfBaseAndExpansion(RechargeableResourceEnum resource, ExpansionItem item)
        {
            int baseAmount = 5;
            ItemInventory inventory = new ItemInventory(new ResourceCount().ApplyAmount(resource, baseAmount))
                .ApplyAddItem(item).ApplyAddItem(item);
            Assert.Equal(baseAmount + 2*item.ResourceAmount, inventory.ResourceMaximums.GetAmount(resource));

        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile, SuperMetroidModel.MISSILE_NAME)]
        [InlineData(RechargeableResourceEnum.Super, SuperMetroidModel.SUPER_NAME)]
        [InlineData(RechargeableResourceEnum.PowerBomb, SuperMetroidModel.POWER_BOMB_NAME)]
        public void ResourceMaximums_ConsumableAmmo_WithExpansions_ReturnsSumOfBaseAndExpansion(RechargeableResourceEnum resource, string itemName)
        {
            int baseAmount = 5;
            ExpansionItem item = (ExpansionItem) Model.Items[itemName];
            ItemInventory inventory = new ItemInventory(new ResourceCount().ApplyAmount(resource, baseAmount))
                .ApplyAddItem(item).ApplyAddItem(item);
            Assert.Equal(baseAmount + 2 * item.ResourceAmount, inventory.ResourceMaximums.GetAmount(resource.ToConsumableResource()));
        }

        [Fact]
        public void ResourceMaximums_Consumableenergy_WithExpansions_ReturnsSumOfBaseAndExpansionForBothEnergyTypes()
        {
            ExpansionItem etank = (ExpansionItem)Model.Items[SuperMetroidModel.ENERGY_TANK_NAME];
            ExpansionItem reserve = (ExpansionItem)Model.Items[SuperMetroidModel.RESERVE_TANK_NAME];
            ItemInventory inventory = new ItemInventory(
                new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, 2).ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 4))
                .ApplyAddItem(etank)
                .ApplyAddItem(etank)
                .ApplyAddItem(reserve);
            // 2 + 4 + 100*3
            Assert.Equal(306, inventory.ResourceMaximums.GetAmount(ConsumableResourceEnum.ENERGY));
        }

        [Fact]
        public void ContainsAnyInGameItem_HasNonConsumableItem_ReturnsTrue()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()).ApplyAddItem(varia);

            Assert.True(inventory.ContainsAnyInGameItem());
        }

        [Fact]
        public void ContainsAnyInGameItem_HasExpansionItem_ReturnsTrue()
        {
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()).ApplyAddItem(missilePack);

            Assert.True(inventory.ContainsAnyInGameItem());
        }

        [Fact]
        public void ContainsAnyInGameItem_HasOnlyNonGameItem_ReturnsFalse()
        {
            Item powerBeam = Model.Items[SuperMetroidModel.POWER_BEAM_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()).ApplyAddItem(powerBeam);

            Assert.False(inventory.ContainsAnyInGameItem());
        }

        [Fact]
        public void IsItemDisabled_ItemNotPresent_ReturnsFalse()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            Assert.False(inventory.IsItemDisabled(varia));
        }

        [Fact]
        public void IsItemDisabled_ItemPresentButEnabled_ReturnsFalse()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()).ApplyAddItem(varia);

            Assert.False(inventory.IsItemDisabled(varia));
        }

        [Fact]
        public void IsItemDisabled_ItemPresentAndDisabled_ReturnsTrue()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(varia).ApplyDisableItem(varia);

            Assert.True(inventory.IsItemDisabled(varia));
        }

        [Fact]
        public void Clone_CopiesCorrectly()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];

            ResourceCount resourceCount = new ResourceCount();
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(resource, value++);
            }
            ItemInventory inventory = new ItemInventory(resourceCount);
            inventory.ApplyAddItem(varia)
                .ApplyAddItem(speedBooster).ApplyDisableItem(speedBooster)
                .ApplyAddItem(missilePack).ApplyAddItem(missilePack);

            ItemInventory clone = inventory.Clone();
            Assert.Equal(2, clone.NonConsumableItems.Count);
            Assert.Contains(varia.Name, clone.NonConsumableItems.Keys);
            Assert.Contains(speedBooster.Name, clone.NonConsumableItems.Keys);
            Assert.Contains(speedBooster.Name, clone.DisabledItemNames);
            Assert.Single(clone.ExpansionItems);
            Assert.Contains(missilePack.Name, clone.ExpansionItems.Keys);
            Assert.Equal(2, clone.ExpansionItems[missilePack.Name].count);
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(resourceCount.GetAmount(resource), clone.BaseResourceMaximums.GetAmount(resource));
            }
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            ItemInventory clone = inventory.Clone();
            inventory.ApplyAddItem(varia)
                .ApplyAddItem(speedBooster).ApplyDisableItem(speedBooster)
                .ApplyAddItem(missilePack).ApplyAddItem(missilePack);

            Assert.Empty(clone.NonConsumableItems);
            Assert.Empty(clone.ExpansionItems);
            Assert.Empty(clone.DisabledItemNames);
        }

        [Fact]
        public void WithResourceMaximums_CopiesCorrectly()
        {
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];

            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            inventory.ApplyAddItem(varia)
                .ApplyAddItem(speedBooster).ApplyDisableItem(speedBooster)
                .ApplyAddItem(missilePack).ApplyAddItem(missilePack);
            ResourceCount resourceCount = new ResourceCount();
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(resource, value++);
            }
            ItemInventory clone = inventory.WithBaseResourceMaximums(resourceCount);

            Assert.Equal(2, clone.NonConsumableItems.Count);
            Assert.Contains(varia.Name, clone.NonConsumableItems.Keys);
            Assert.Contains(speedBooster.Name, clone.NonConsumableItems.Keys);
            Assert.Contains(speedBooster.Name, clone.DisabledItemNames);
            Assert.Single(clone.ExpansionItems);
            Assert.Contains(missilePack.Name, clone.ExpansionItems.Keys);
            Assert.Equal(2, clone.ExpansionItems[missilePack.Name].count);
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                Assert.Equal(resourceCount.GetAmount(resource), clone.BaseResourceMaximums.GetAmount(resource));
            }
        }

        [Fact]
        public void WithResourceMaximums_SeparatesState()
        {
            ResourceCount resourceCount = new ResourceCount();
            int value = 1;
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceCount.ApplyAmount(resource, value++);
            }
            Item varia = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item speedBooster = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            Item missilePack = Model.Items[SuperMetroidModel.MISSILE_NAME];
            ItemInventory inventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());

            ItemInventory clone = inventory.WithBaseResourceMaximums(resourceCount);
            inventory.ApplyAddItem(varia)
                .ApplyAddItem(speedBooster).ApplyDisableItem(speedBooster)
                .ApplyAddItem(missilePack).ApplyAddItem(missilePack);

            Assert.Empty(clone.NonConsumableItems);
            Assert.Empty(clone.ExpansionItems);
            Assert.Empty(clone.DisabledItemNames);
        }
    }
}
