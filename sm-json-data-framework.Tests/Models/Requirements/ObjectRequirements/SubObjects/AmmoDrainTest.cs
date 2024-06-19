using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Tests.TestTools;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.SubObjects
{
    public class AmmoDrainTest
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
            AmmoDrain ammoDrain = model.Locks["Mother Brain 2 and 3 Fight"].UnlockStrats["Base"].Requires.LogicalElement<AmmoDrain>(0);
            Assert.Equal(75, ammoDrain.Count);
            Assert.Equal(AmmoEnum.Missile, ammoDrain.AmmoType);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NotEnoughAmmo_NoAmmoAtAll_SucceedsAnyway()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AmmoDrain ammoDrain = model.Locks["Mother Brain 2 and 3 Fight"].UnlockStrats["Base"].Requires
                .LogicalElement<AmmoDrain>(0, ammoDrain => ammoDrain.AmmoType == AmmoEnum.Missile && ammoDrain.Count == 75);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = ammoDrain.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_NotEnoughAmmo_MoreAmmoThanDrain_RemovesFullAmount()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AmmoDrain ammoDrain = model.Locks["Mother Brain 2 and 3 Fight"].UnlockStrats["Base"].Requires
                .LogicalElement<AmmoDrain>(0, ammoDrain => ammoDrain.AmmoType == AmmoEnum.Missile && ammoDrain.Count == 75);
            InGameState inGameState = model.CreateInitialGameState();
            for(int i = 0; i < 20; i++)
            {
                inGameState.ApplyAddItem(SuperMetroidModel.MISSILE_NAME);
            }
            inGameState.ApplyRefillResources();

            // When
            ExecutionResult result = ammoDrain.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.Missile, -75)
                .AssertRespectedBy(result);
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

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            AmmoDrain freeAmmoDrain = model.Locks["Mother Brain 2 and 3 Fight"].UnlockStrats["Base"].Requires.LogicalElement<AmmoDrain>(0);
            Assert.True(freeAmmoDrain.LogicallyRelevant);
            Assert.False(freeAmmoDrain.LogicallyNever);
            Assert.True(freeAmmoDrain.LogicallyAlways);
            Assert.True(freeAmmoDrain.LogicallyFree);

            AmmoDrain nonFreeAmmoDrain = model.Locks["Mother Brain 2 and 3 Fight"].UnlockStrats["Base"].Requires.LogicalElement<AmmoDrain>(1);
            Assert.True(nonFreeAmmoDrain.LogicallyRelevant);
            Assert.False(nonFreeAmmoDrain.LogicallyNever);
            Assert.True(nonFreeAmmoDrain.LogicallyAlways);
            Assert.False(nonFreeAmmoDrain.LogicallyFree);
        }

        #endregion
    }
}
