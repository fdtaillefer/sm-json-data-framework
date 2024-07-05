using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Utils;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Enemies;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.SubObjects
{
    public class EnemyKillTest
    {
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();
        private static UnfinalizedSuperMetroidModel UnfinalizedModelForModification() => new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation
            SuperMetroidModel model = ReusableModel();

            // Expect
            EnemyKill enemyKillWithExplicitWeapons = model.Rooms["Morph Ball Room"].Links[1].To[6].Strats["Quick Sidehopper Kill"].Obstacles["C"]
                .Requires.LogicalElement<Or>(0).LogicalRequirements.LogicalElement<EnemyKill>(0);
            Assert.Equal(2, enemyKillWithExplicitWeapons.GroupedEnemies.Count);
            Assert.Equal(2, enemyKillWithExplicitWeapons.GroupedEnemies[0].Count);
            Assert.Equal(1, enemyKillWithExplicitWeapons.GroupedEnemies[1].Count);
            Assert.Same(model.Enemies["Sidehopper"], enemyKillWithExplicitWeapons.GroupedEnemies[0][0]);
            Assert.Same(model.Enemies["Sidehopper"], enemyKillWithExplicitWeapons.GroupedEnemies[0][1]);
            Assert.Same(model.Enemies["Sidehopper"], enemyKillWithExplicitWeapons.GroupedEnemies[1][0]);
            Assert.Equal(5, enemyKillWithExplicitWeapons.ExplicitWeapons.Count);
            Assert.Same(model.Weapons["Missile"], enemyKillWithExplicitWeapons.ExplicitWeapons["Missile"]);
            Assert.Same(model.Weapons["Super"], enemyKillWithExplicitWeapons.ExplicitWeapons["Super"]);
            Assert.Same(model.Weapons["PowerBomb"], enemyKillWithExplicitWeapons.ExplicitWeapons["PowerBomb"]);
            Assert.Same(model.Weapons["ScrewAttack"], enemyKillWithExplicitWeapons.ExplicitWeapons["ScrewAttack"]);
            Assert.Same(model.Weapons["Plasma"], enemyKillWithExplicitWeapons.ExplicitWeapons["Plasma"]);
            Assert.Equal(5, enemyKillWithExplicitWeapons.ValidWeapons.Count);
            Assert.Same(model.Weapons["Missile"], enemyKillWithExplicitWeapons.ValidWeapons["Missile"]);
            Assert.Same(model.Weapons["Super"], enemyKillWithExplicitWeapons.ValidWeapons["Super"]);
            Assert.Same(model.Weapons["PowerBomb"], enemyKillWithExplicitWeapons.ValidWeapons["PowerBomb"]);
            Assert.Same(model.Weapons["ScrewAttack"], enemyKillWithExplicitWeapons.ValidWeapons["ScrewAttack"]);
            Assert.Same(model.Weapons["Plasma"], enemyKillWithExplicitWeapons.ValidWeapons["Plasma"]);
            Assert.Empty(enemyKillWithExplicitWeapons.ExcludedWeapons);
            Assert.Empty(enemyKillWithExplicitWeapons.FarmableAmmo);

            EnemyKill enemyKillWithDifferentEnemies = model.Rooms["Pink Brinstar Hopper Room"].Links[1].To[1].Strats["Fast Weapon Kill"].Obstacles["A"]
                .Requires.LogicalElement<EnemyKill>(0);
            Assert.Equal(1, enemyKillWithDifferentEnemies.GroupedEnemies.Count);
            Assert.Equal(3, enemyKillWithDifferentEnemies.GroupedEnemies[0].Count);
            Assert.Same(model.Enemies["Sidehopper"], enemyKillWithDifferentEnemies.GroupedEnemies[0][0]);
            Assert.Same(model.Enemies["Sm. Sidehopper"], enemyKillWithDifferentEnemies.GroupedEnemies[0][1]);
            Assert.Same(model.Enemies["Sm. Sidehopper"], enemyKillWithDifferentEnemies.GroupedEnemies[0][2]);

            EnemyKill enemyKillWithExcludedWeapons = model.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Kill the Beetoms"]
                .Requires.LogicalElement<EnemyKill>(0);
            List<Weapon> expectedValidWeapons = model.Weapons.Values.Where(weapon => !weapon.Situational)
                .Except(model.Enemies["Beetom"].InvulnerableWeapons.Values, ObjectReferenceEqualityComparer<Weapon>.Default)
                .Except(enemyKillWithExcludedWeapons.ExcludedWeapons.Values, ObjectReferenceEqualityComparer<Weapon>.Default)
                .ToList();
            Assert.Empty(enemyKillWithExcludedWeapons.ExplicitWeapons);
            Assert.Equal(2, enemyKillWithExcludedWeapons.ExcludedWeapons.Count);
            Assert.Same(model.Weapons["Bombs"], enemyKillWithExcludedWeapons.ExcludedWeapons["Bombs"]);
            Assert.Same(model.Weapons["PowerBomb"], enemyKillWithExcludedWeapons.ExcludedWeapons["PowerBomb"]);
            Assert.Equal(expectedValidWeapons.Count, enemyKillWithExcludedWeapons.ValidWeapons.Count);
            foreach(Weapon weapon in expectedValidWeapons){
                Assert.Same(weapon, enemyKillWithExcludedWeapons.ValidWeapons[weapon.Name]);
            }

            EnemyKill enemyKillWithFarmableAmmo = model.Locks["Draygon Fight"].UnlockStrats["Gravity Draygon"]
                .Requires.LogicalElement<EnemyKill>(0);
            Assert.Equal(2, enemyKillWithFarmableAmmo.FarmableAmmo.Count);
            Assert.Contains(AmmoEnum.Missile, enemyKillWithFarmableAmmo.FarmableAmmo);
            Assert.Contains(AmmoEnum.Super, enemyKillWithFarmableAmmo.FarmableAmmo);

            // If there was an EnemyKill without explicitWeapons and with mixed enemies that have different immunities,
            // we would use it to test its list of valid weapons.
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_PossibleForFree_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyKill enemyKill = model.Rooms["Warehouse Energy Tank Room"].Links[1].To[2].Strats["Kill the Beetoms"].Obstacles["A"]
                .Requires.LogicalElement<EnemyKill>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("ScrewAttack");

            // When
            ExecutionResult result = enemyKill.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectKilledEnemy("Beetom", ("ScrewAttack", 1))
                .ExpectKilledEnemy("Beetom", ("ScrewAttack", 1))
                .ExpectKilledEnemy("Beetom", ("ScrewAttack", 1))
                .ExpectKilledEnemy("Beetom", ("ScrewAttack", 1))
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_PossibleForFreeOrForAmmo_SucceedsForFree()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyKill enemyKill = model.Rooms["Warehouse Energy Tank Room"].Links[1].To[2].Strats["Kill the Beetoms"].Obstacles["A"]
                .Requires.LogicalElement<EnemyKill>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("ScrewAttack")
                .ApplyAddItem("PowerBomb")
                .ApplyRefillResources();

            // When
            ExecutionResult result = enemyKill.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectKilledEnemy("Beetom", ("ScrewAttack", 1))
                .ExpectKilledEnemy("Beetom", ("ScrewAttack", 1))
                .ExpectKilledEnemy("Beetom", ("ScrewAttack", 1))
                .ExpectKilledEnemy("Beetom", ("ScrewAttack", 1))
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_PossibleForAmmo_ConsumesAmmo()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyKill enemyKill = model.Rooms["Warehouse Energy Tank Room"].Links[1].To[2].Strats["Kill the Beetoms"].Obstacles["A"]
                .Requires.LogicalElement<EnemyKill>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Missile")
                .ApplyRefillResources();

            // When
            ExecutionResult result = enemyKill.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectKilledEnemy("Beetom", ("Missile", 1))
                .ExpectKilledEnemy("Beetom", ("Missile", 1))
                .ExpectKilledEnemy("Beetom", ("Missile", 1))
                .ExpectKilledEnemy("Beetom", ("Missile", 1))
                .ExpectResourceVariation(RechargeableResourceEnum.Missile, -4)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_PossibleForAoeWeapon_DoesAoeKill()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyKill enemyKill = model.Rooms["Warehouse Energy Tank Room"].Links[1].To[2].Strats["Kill the Beetoms"].Obstacles["A"]
                .Requires.LogicalElement<EnemyKill>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Morph")
                .ApplyAddItem("PowerBomb")
                .ApplyRefillResources();

            // When
            ExecutionResult result = enemyKill.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectKilledEnemy("Beetom", ("PowerBomb", 1))
                .ExpectKilledEnemy("Beetom", ("PowerBomb", 1))
                .ExpectKilledEnemy("Beetom", ("PowerBomb", 1))
                .ExpectKilledEnemy("Beetom", ("PowerBomb", 1))
                .ExpectResourceVariation(RechargeableResourceEnum.PowerBomb, -1)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_NoUsableValidWeapon_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyKill enemyKill = model.Rooms["Warehouse Energy Tank Room"].Links[1].To[2].Strats["Kill the Beetoms"].Obstacles["A"]
                .Requires.LogicalElement<EnemyKill>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Bombs"); // Can't use bombs without morph anyway

            // When
            ExecutionResult result = enemyKill.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_NotEnoughAmmo_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyKill enemyKill = model.Rooms["Warehouse Energy Tank Room"].Links[1].To[2].Strats["Kill the Beetoms"].Obstacles["A"]
                .Requires.LogicalElement<EnemyKill>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Missile")
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Missile, 2);

            // When
            ExecutionResult result = enemyKill.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_CantUseExplicitWeapon_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyKill enemyKill = model.Rooms["Etecoon Energy Tank Room"].Links[4].To[3].Strats["Bomb the Beetoms"]
                .Requires.LogicalElement<EnemyKill>(0, enemyKill => enemyKill.ExplicitWeapons.ContainsKey("Bombs"));
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Bombs")
                .ApplyAddItem("ScrewAttack")
                .ApplyAddItem("Missile")
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Missile, 2);

            // When
            ExecutionResult result = enemyKill.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_OnlyHasExcludedWeapon_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnemyKill enemyKill = model.Rooms["Etecoon Energy Tank Room"].Links[3].To[4].Strats["Kill the Beetoms"]
                .Requires.LogicalElement<EnemyKill>(0, enemyKill => enemyKill.ExcludedWeapons.ContainsKey("Bombs"));
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Bombs")
                .ApplyAddItem("Morph");

            // When
            ExecutionResult result = enemyKill.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_MixGroupWithPartialAmmoRequirement_KillsWithMultipleWeapons()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = UnfinalizedModelForModification();
            UnfinalizedEnemyKill newEnemyKill = new UnfinalizedEnemyKill();
            newEnemyKill.ExplicitWeapons = new List<UnfinalizedWeapon>();
            newEnemyKill.ExcludedWeapons = new List<UnfinalizedWeapon>();
            newEnemyKill.GroupedEnemyNames.Add(new List<string> { "Reo", "Beetom" });
            newEnemyKill.GroupedEnemies = new List<IList<UnfinalizedEnemy>> { new List<UnfinalizedEnemy> { unfinalizedModel.Enemies["Reo"], unfinalizedModel.Enemies["Beetom"] } };
            unfinalizedModel.Rooms["Landing Site"].Links[5].To[2].Strats["Base"].Requires.LogicalElements.Add(newEnemyKill);

            SuperMetroidModel model = unfinalizedModel.Finalize();
            EnemyKill enemyKill = model.Rooms["Landing Site"].Links[5].To[2].Strats["Base"]
                .Requires.LogicalElement<EnemyKill>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Missile")
                .ApplyRefillResources();

            // When
            ExecutionResult result = enemyKill.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectKilledEnemy("Beetom", ("Missile", 1))
                .ExpectKilledEnemy("Reo", (SuperMetroidModel.POWER_BEAM_NAME, 3))
                .ExpectResourceVariation(RechargeableResourceEnum.Missile, -1)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_MixGroupWithPartialUnkillable_Fails()
        {
            // Given
            UnfinalizedSuperMetroidModel unfinalizedModel = UnfinalizedModelForModification();
            UnfinalizedEnemyKill newEnemyKill = new UnfinalizedEnemyKill();
            newEnemyKill.ExplicitWeapons = new List<UnfinalizedWeapon>();
            newEnemyKill.ExcludedWeapons = new List<UnfinalizedWeapon>();
            newEnemyKill.GroupedEnemyNames.Add(new List<string> { "Reo", "Beetom" });
            newEnemyKill.GroupedEnemies = new List<IList<UnfinalizedEnemy>> { new List<UnfinalizedEnemy> { unfinalizedModel.Enemies["Reo"], unfinalizedModel.Enemies["Beetom"] } };
            unfinalizedModel.Rooms["Landing Site"].Links[5].To[2].Strats["Base"].Requires.LogicalElements.Add(newEnemyKill);

            SuperMetroidModel model = unfinalizedModel.Finalize();
            EnemyKill enemyKill = model.Rooms["Landing Site"].Links[5].To[2].Strats["Base"]
                .Requires.LogicalElement<EnemyKill>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyRefillResources();

            // When
            ExecutionResult result = enemyKill.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["Spazer"])
                )
                .Build();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["ReserveTank"], 4);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            EnemyKill impossibleEnemyKill = model.Rooms["Morph Ball Room"].Links[1].To[6].Strats["PB Sidehopper Kill with Bomb Blocks"].Obstacles["C"].Requires.LogicalElement<EnemyKill>(0);
            Assert.True(impossibleEnemyKill.LogicallyRelevant);
            Assert.True(impossibleEnemyKill.LogicallyNever);
            Assert.False(impossibleEnemyKill.LogicallyAlways);
            Assert.False(impossibleEnemyKill.LogicallyFree);

            EnemyKill possibleEnemyKill = model.Rooms["Green Brinstar Beetom Room"].Links[1].To[2].Strats["Kill the Beetoms"].Requires.LogicalElement<EnemyKill>(0);
            Assert.True(possibleEnemyKill.LogicallyRelevant);
            Assert.False(possibleEnemyKill.LogicallyNever);
            Assert.False(possibleEnemyKill.LogicallyAlways);
            Assert.False(possibleEnemyKill.LogicallyFree);

            EnemyKill freeEnemyKill = model.Rooms["Baby Kraid Room"].Links[1].To[2].Strats["Kill the Enemies"].Obstacles["A"].Requires.LogicalElement<EnemyKill>(0);
            Assert.True(freeEnemyKill.LogicallyRelevant);
            Assert.False(freeEnemyKill.LogicallyNever);
            Assert.True(freeEnemyKill.LogicallyAlways);
            Assert.True(freeEnemyKill.LogicallyFree);
        }

        #endregion
    }
}
