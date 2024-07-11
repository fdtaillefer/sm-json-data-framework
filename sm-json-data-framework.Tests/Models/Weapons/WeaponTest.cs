using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Weapons
{
    public class WeaponTest
    {
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation
            SuperMetroidModel model = ReusableModel();

            // Expect
            Weapon weapon = model.Weapons["Plasma Shield"];
            Assert.Equal("Plasma Shield", weapon.Name);
            Assert.Equal(39, weapon.Id);
            Assert.Equal(1200, weapon.Damage);
            Assert.Equal(400, weapon.CooldownFrames);
            Assert.NotNull(weapon.UseRequires);
            Assert.Equal(3, weapon.UseRequires.LogicalElements.Count);
            Assert.Equal(3, weapon.UseRequires.LogicalElementsTyped<ItemLogicalElement>().Count());
            Assert.NotNull(weapon.ShotRequires);
            Assert.Equal(1, weapon.ShotRequires.LogicalElements.Count);
            Assert.NotNull(weapon.ShotRequires.LogicalElement<Ammo>(0));
            Assert.True(weapon.Situational);
            Assert.False(weapon.HitsGroup);
            Assert.Equal(2, weapon.Categories.Count);
            Assert.True(weapon.Categories.Contains(WeaponCategoryEnum.All));
            Assert.True(weapon.Categories.Contains(WeaponCategoryEnum.SpecialBeamAttack));
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["PowerBomb"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ReserveTank"], 4);

            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["Wave"])
                )
                .Build();

            logicalOptions.RegisterRemovedItem("Ice");

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Weapon impossibleUseWeapon = model.Weapons["Ice"];
            Assert.False(impossibleUseWeapon.LogicallyRelevant);
            Assert.False(impossibleUseWeapon.LogicallyAlways);
            Assert.False(impossibleUseWeapon.LogicallyFree);
            Assert.True(impossibleUseWeapon.LogicallyNever);

            Weapon impossibleShootWeapon = model.Weapons["Missile"];
            Assert.False(impossibleShootWeapon.LogicallyRelevant);
            Assert.False(impossibleShootWeapon.LogicallyAlways);
            Assert.False(impossibleShootWeapon.LogicallyFree);
            Assert.True(impossibleShootWeapon.LogicallyNever);

            Weapon nonFreeWeapon = model.Weapons["Charge+Wave"];
            Assert.True(nonFreeWeapon.LogicallyRelevant);
            Assert.False(nonFreeWeapon.LogicallyAlways);
            Assert.False(nonFreeWeapon.LogicallyFree);
            Assert.False(nonFreeWeapon.LogicallyNever);

            Weapon freeWeapon = model.Weapons["Wave"];
            Assert.True(freeWeapon.LogicallyRelevant);
            Assert.True(freeWeapon.LogicallyAlways);
            Assert.True(freeWeapon.LogicallyFree);
            Assert.False(freeWeapon.LogicallyNever);
        }

        #endregion
    }
}
