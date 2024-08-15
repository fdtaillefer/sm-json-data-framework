using sm_json_data_framework.EnergyManagement;
using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Tests.TestTools;
using sm_json_data_framework.Tests.TestTools.AlteredRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace sm_json_data_framework.Tests.EnergyManagement
{
    public class EnergyManagerTest
    {
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;

        private static SuperMetroidModel UselessSuitsModel() => BuiltUselessSuitsModel;
        private static SuperMetroidModel BuiltUselessSuitsModel = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel, new UselessSuitsSuperMetroidRules()).Finalize();

        private static SuperMetroidModel NoMidTaskReservesModel() => BuiltNoMidTaskReservesModel;
        private static SuperMetroidModel BuiltNoMidTaskReservesModel => BuildNoMidTaskReservesModel();
        private static SuperMetroidModel BuildNoMidTaskReservesModel()
        {
            LogicalOptions noMidTaskReservesOptions = new LogicalOptions();
            noMidTaskReservesOptions.RegisterDisabledTech(SuperMetroidModel.USE_RESERVES_FOR_SHINESPARK_TECH_NAME);
            return StaticTestObjects.UnfinalizedModel.Finalize(noMidTaskReservesOptions);
        }

        #region Tests for CalculatePunctualEnemyDamageEnergyVariation()

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_EnoughRegularEnergy_UsesRegularEnergy()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 78);

            ReadOnlyResourceCount expected = new ResourceCount().ApplyAmountReduction(RechargeableResourceEnum.RegularEnergy, 20);
            ReadOnlyResourceCount result = 
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 1, canActBeforeFirstHit: false);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_MultipleHits_AppliesCorrectReduction()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyRefillResources();

            ReadOnlyResourceCount expected = new ResourceCount().ApplyAmountReduction(RechargeableResourceEnum.RegularEnergy, 40);
            ReadOnlyResourceCount result = 
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 2, canActBeforeFirstHit: false);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_SuitReduction_AppliesCorrectReduction()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyRefillResources();

            ReadOnlyResourceCount expected = new ResourceCount().ApplyAmountReduction(RechargeableResourceEnum.RegularEnergy, 10);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 1, canActBeforeFirstHit: false);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_WithSuitButNoReduction_AppliesNoReduction()
        {
            SuperMetroidModel model = UselessSuitsModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyRefillResources();

            ReadOnlyResourceCount expected = new ResourceCount().ApplyAmountReduction(RechargeableResourceEnum.RegularEnergy, 20);
            ReadOnlyResourceCount result = 
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 1, canActBeforeFirstHit: false);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_NotEnoughRegularEnergy_NoReserves_Fails()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 79); ;

            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 1, canActBeforeFirstHit: false);

            Assert.Null(result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_NotEnoughRegularEnergy_PreUseReserves_UsesReserves()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 95);

            // Start at 4. We need to use 17 reserve to go to 21, but leeway makes us use 20 more (37) bringing us to 41.
            // So the hit brings us down to 21.
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountIncrease(RechargeableResourceEnum.RegularEnergy, 17)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 37);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 1, canActBeforeFirstHit: true);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_NotEnoughRegularEnergy_CantPreUseReserves_NoOverflowButDoubleHit_UsesCorrectAmount()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyAddResource(RechargeableResourceEnum.ReserveEnergy, 41)
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 95);

            // Start at 4. Hit brings us down to 0 then auto-reserves kick in brings us to 41.
            // Then iframes run out (or well, there's 59 left and logical options say we need 60 t0 avoid a double hit) and a double hit brings us down to 21.
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountIncrease(RechargeableResourceEnum.RegularEnergy, 17)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 41);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 1, canActBeforeFirstHit: false);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_NotEnoughRegularEnergy_CantPreUseReserves_NoOverflowNoDoubleHit_UsesCorrectAmount()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyAddResource(RechargeableResourceEnum.ReserveEnergy, 40)
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 95);

            // Start at 4. Hit brings us down to 0 then auto-reserves kick in brings us to 40.
            // Then with 60 iframes left logical options say we can avoid a double hit.
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountIncrease(RechargeableResourceEnum.RegularEnergy, 36)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 40);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 1, canActBeforeFirstHit: false);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_NotEnoughRegularEnergyForAllHits_UseReservesOnSecondHit_UsesCorrectAmount()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 75);

            // Start at 24. First hit brings us down to 4. For the second hit we need to use 17 reserve to go to 21, but leeway makes us use 20 more (37) bringing us to 41.
            // then second hit brings us down to 21.
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountReduction(RechargeableResourceEnum.RegularEnergy, 3)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 37);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 2, canActBeforeFirstHit: true);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_NoMidTaskReserveUse_UseReservesOnSecondHit_UsesCorrectAmount()
        {
            SuperMetroidModel model = NoMidTaskReservesModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 175);

            // Start at 24. First hit brings us down to 4. For the second hit we are not allowed to use reserves, so we auto-reserve and take a double hit.
            // second hit brings us down to 80.
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountIncrease(RechargeableResourceEnum.RegularEnergy, 56)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 100);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 2, canActBeforeFirstHit: true);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_NotEnoughPreReservesToSurvive_NoDoubleHit_SurvivesAndUsesCorrectAmount()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyAddResource(RechargeableResourceEnum.ReserveEnergy, 10)
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 95);

            // Start at 4 energy. Reserves don't get pre-used because they don't allow us to survive the hit
            // But auto-reserves don't trigger an auto-hit and allow us to make it out with 10 energy.
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountIncrease(RechargeableResourceEnum.RegularEnergy, 6)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 10);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Mini-Kraid"].Attacks["contact"], 1, canActBeforeFirstHit: true);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_DoubleHitCausesDeath_Fails()
        {
            SuperMetroidModel model = NoMidTaskReservesModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources();

            // Stat at 199. First hit takes us to 99. We cannot use reserves mid-task so second hit brings us down to 0.
            // Auto-reserves trigger but cause a double hit and we die by exact damage.
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Mini-Kraid"].Attacks["contact"], 2, canActBeforeFirstHit: true);

            Assert.Null(result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_AutoReservesWastingEnergy_UsesCorrectAmount()
        {
            SuperMetroidModel model = NoMidTaskReservesModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources();

            // Stat at 199. First hit takes us to 99. We cannot use reserves mid-task so second hit brings us down to 0.
            // Auto-reserves trigger and bring us to 199, wasting the rest. A double hit triggers and leaves us at 99
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountReduction(RechargeableResourceEnum.RegularEnergy, 100)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 300);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Mini-Kraid"].Attacks["contact"], 2, canActBeforeFirstHit: true);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_PreReserveWithMultipleHitsLeft_UsesCorrectAmount()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 175);

            // Start at 24. First hit takes us to 4. Pre-reserve aiming at 61 and overshooting to 81 (using 77 reserve).
            // The next three hits bring us down to 21.
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountReduction(RechargeableResourceEnum.RegularEnergy, 3)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 77);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 4, canActBeforeFirstHit: true);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_AutoReserveWithMultipleHitsLeft_UsesCorrectAmount()
        {
            SuperMetroidModel model = NoMidTaskReservesModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 175);

            // Start at 24. First hit takes us to 4. Second hit brings us down to 0.
            // Auto-Reserves bring us up to 199 then double hit takes us down to 179. Then the next 2 hits bring us to 139.
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountIncrease(RechargeableResourceEnum.RegularEnergy, 115)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 300);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 4, canActBeforeFirstHit: true);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_PreReserveWithinLeewayOfConflict_UsesCorrectAmount()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 75);

            // Start at 24. First hit takes us to 4.
            // Pre-reserves should then aim at 61, and end up at 81 with leeway
            // This is higher than aiming for max would leave us at after leeway (79) but it should win out as our "worst case" here.
            // Then the 3 hits take us to 21
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountReduction(RechargeableResourceEnum.RegularEnergy, 3)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 77);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 4, canActBeforeFirstHit: true);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_PreReserveWouldHitMax_AimsForMaxWithLeewayInstead()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyRefillResources()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 75);

            // Start at 24. First hit takes us to 4.
            // Pre-reserves should then aim at 81, but that would hit 101 with leeway
            // So instead it will aim at 99 and get to 79 (with the attitude of "we'll adjust later if needed")
            // Then 3 hits take us to 19 so we need to pre-reserve again aiming for 21 and ending at 41 with leeway.
            // And finally the 5th hit takes us to 21.
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountReduction(RechargeableResourceEnum.RegularEnergy, 3)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 97);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 5, canActBeforeFirstHit: true);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculatePunctualEnemyDamageEnergyVariation_PreReserveWouldHitMaxButNotEnoughReservesForMax_DoesNotAimForMax()
        {
            SuperMetroidModel model = ReusableModel();
            EnergyManager energyManager = new EnergyManager(model.AppliedLogicalOptions, model.Rules);

            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.RESERVE_TANK_NAME)
                .ApplyAddResource(RechargeableResourceEnum.ReserveEnergy, 90)
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 75);

            // Start at 24. First hit takes us to 4.
            // Pre-reserves should then aim at 81, which would hit 101 with leeway, but emptying reserves is found to be safe (empties before hitting max) so that happens instead
            // So we empty reserves to 94, then 4 hits takes us to 14.
            ReadOnlyResourceCount expected = new ResourceCount()
                .ApplyAmountReduction(RechargeableResourceEnum.RegularEnergy, 10)
                .ApplyAmountReduction(RechargeableResourceEnum.ReserveEnergy, 90);
            ReadOnlyResourceCount result =
                energyManager.CalculatePunctualEnemyDamageEnergyVariation(inGameState, model.Enemies["Alcoon"].Attacks["fireball"], 5, canActBeforeFirstHit: true);

            Assert.Equal(expected, result);
        }

        #endregion

        #region Tests for CalculatePunctualEnvironmentDamageEnergyVariation()

        #endregion

        #region Tests for CalculateDamageOverTimeEnergyVariation()

        #endregion

        #region Tests for CalculateShinesparkEnergyVariation()

        #endregion
    }
}
