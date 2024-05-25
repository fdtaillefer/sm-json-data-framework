using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Options
{
    public class LogicalOptionsTest
    {
        // Use a static model to build it only once.
        private static SuperMetroidModel Model { get; set; } = StaticTestObjects.UnmodifiableModel;

        #region Tests for NumberOfTries(Tech)

        [Fact]
        public void NumberOfTries_TechWithRegisteredNumber_ReturnsRegisteredValue()
        {
            // Given
            Tech tech = Model.Techs["canIBJ"];
            int expected = 5;
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterTechTries(tech.Name, expected);

            // When
            int result = logicalOptions.NumberOfTries(tech);

            // Expect
            Assert.Equal(expected, result);
        }

        [Fact]
        public void NumberOfTries_UnalteredTech_ReturnsDefaultNumber()
        {
            // Given
            Tech tech = Model.Techs["canIBJ"];
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            int result = logicalOptions.NumberOfTries(tech);

            // Expect
            Assert.Equal(LogicalOptions.DefaultNumberOfTries, result);
        }

        #endregion

        #region Tests for NumberOfTries(Helper)

        [Fact]
        public void NumberOfTries_HelperWithRegisteredTries_ReturnsRegisteredValue()
        {
            // Given
            Helper helper = Model.Helpers["h_canBombThings"];
            int expected = 5;
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterHelperTries(helper.Name, expected);

            // When
            int result = logicalOptions.NumberOfTries(helper);

            // Expect
            Assert.Equal(expected, result);
        }

        [Fact]
        public void NumberOfTries_UnalteredHelper_ReturnsDefaultNumber()
        {
            // Given
            Helper helper = Model.Helpers["h_canBombThings"];
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            int result = logicalOptions.NumberOfTries(helper);

            // Expect
            Assert.Equal(LogicalOptions.DefaultNumberOfTries, result);
        }

        #endregion

        #region Tests for NumberOfTries(Strat)

        [Fact]
        public void NumberOfTries_StratWithRegisteredNumber_ReturnsRegisteredValue()
        {
            // Given
            Strat strat = Model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).LinksTo[3].Strats["Ceiling E-Tank Dboost"];
            int expected = 5;
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterStratTries(strat.Name, expected);

            // When
            int result = logicalOptions.NumberOfTries(strat);

            // Expect
            Assert.Equal(expected, result);
        }

        [Fact]
        public void NumberOfTries_NonNotableStrat_ReturnsDefaultNumber()
        {
            // Given
            Strat strat = Model.GetNodeInRoom("Landing Site", 5).LinksTo[2].Strats["Base"];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterStratTries(strat.Name, 5);

            // When
            int result = logicalOptions.NumberOfTries(strat);

            // Expect
            Assert.Equal(LogicalOptions.DefaultNumberOfTries, result);
        }

        [Fact]
        public void NumberOfTries_UnalteredStrat_ReturnsDefaultNumber()
        {
            // Given
            Strat strat = Model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).LinksTo[3].Strats["Ceiling E-Tank Dboost"];
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            int result = logicalOptions.NumberOfTries(strat);

            // Expect
            Assert.Equal(LogicalOptions.DefaultNumberOfTries, result);
        }

        #endregion

        #region Tests for IsTechEnabled()

        [Fact]
        public void IsTechEnabled_TechsEnabledByDefaultAndNoMention_ReturnsTrue()
        {
            // Given
            Tech tech = Model.Techs["canIBJ"];
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            bool result = logicalOptions.IsTechEnabled(tech);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsTechEnabled_ExplicitlyDisabled_ReturnsFalse()
        {
            // Given
            Tech tech = Model.Techs["canIBJ"];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledTech(tech.Name);

            // When
            bool result = logicalOptions.IsTechEnabled(tech);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsTechEnabled_Reenabled_ReturnsTrue()
        {
            // Given
            Tech tech = Model.Techs["canIBJ"];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledTech(tech.Name);
            logicalOptions.UnregisterDisabledTech(tech.Name);

            // When
            bool result = logicalOptions.IsTechEnabled(tech);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsTechEnabled_TechsDisabledByDefaultButExplicitlyEnabled_ReturnsTrue()
        {
            // Given
            Tech tech = Model.Techs["canIBJ"];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TechsEnabledByDefault = false;
            logicalOptions.RegisterEnabledTech(tech.Name);

            // When
            bool result = logicalOptions.IsTechEnabled(tech);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsTechEnabled_TechsDisabledByDefaultReDisabled_ReturnsFalse()
        {
            // Given
            Tech tech = Model.Techs["canIBJ"];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TechsEnabledByDefault = false;
            logicalOptions.RegisterEnabledTech(tech.Name);
            logicalOptions.UnregisterEnabledTech(tech.Name);

            // When
            bool result = logicalOptions.IsTechEnabled(tech);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsTechEnabled_TechsDisabledByDefaultAndNoMention_ReturnsFalse()
        {
            // Given
            Tech tech = Model.Techs["canIBJ"];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.TechsEnabledByDefault = false;

            // When
            bool result = logicalOptions.IsTechEnabled(tech);

            // Expect
            Assert.False(result);
        }

        #endregion

        #region Tests for IsStratEnabled()

        [Fact]
        public void IsStratEnabled_NoMention_ReturnsTrue()
        {
            // Given
            Strat strat = Model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).LinksTo[3].Strats["Ceiling E-Tank Dboost"];
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            bool result = logicalOptions.IsStratEnabled(strat);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsStratEnabled_ExplicitlyDisabled_ReturnsFalse()
        {
            // Given
            Strat strat = Model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).LinksTo[3].Strats["Ceiling E-Tank Dboost"];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledStrat(strat.Name);

            // When
            bool result = logicalOptions.IsStratEnabled(strat);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsStratEnabled_Reenabled_ReturnsTrue()
        {
            // Given
            Strat strat = Model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).LinksTo[3].Strats["Ceiling E-Tank Dboost"];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledStrat(strat.Name);
            logicalOptions.UnregisterDisabledStrat(strat.Name);

            // When
            bool result = logicalOptions.IsStratEnabled(strat);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsStratEnabled_NonNotableDisabled_ReturnsTrue()
        {
            // Given
            Strat strat = Model.GetNodeInRoom("Landing Site", 5).LinksTo[2].Strats["Base"];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledStrat(strat.Name);

            // When
            bool result = logicalOptions.IsStratEnabled(strat);

            // Expect
            Assert.True(result);
        }

        #endregion

        #region Tests for IsItemInGame()

        [Fact]
        public void IsItemInGame_NoMention_ReturnsTrue()
        {
            // Given
            Item item = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            bool result = logicalOptions.IsItemInGame(item);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsItemInGame_ExplicitlyRemoved_ReturnsFalse()
        {
            // Given
            Item item = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem(item.Name);

            // When
            bool result = logicalOptions.IsItemInGame(item);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsItemInGame_Readded_ReturnsTrue()
        {
            // Given
            Item item = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem(item.Name);
            logicalOptions.UnregisterRemovedItem(item.Name);

            // When
            bool result = logicalOptions.IsItemInGame(item);

            // Expect
            Assert.True(result);
        }

        #endregion

        #region Tests for IsSpeedBoosterInGame()

        [Fact]
        public void IsSpeedBoosterInGame_NoMention_ReturnsTrue()
        {
            // Given
            Item item = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            bool result = logicalOptions.IsSpeedBoosterInGame();

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsSpeedBoosterInGame_ExplicitlyRemoved_ReturnsFalse()
        {
            // Given
            Item item = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem(item.Name);

            // When
            bool result = logicalOptions.IsSpeedBoosterInGame();

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsSpeedBoosterInGame_Readded_ReturnsTrue()
        {
            // Given
            Item item = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem(item.Name);
            logicalOptions.UnregisterRemovedItem(item.Name);

            // When
            bool result = logicalOptions.IsSpeedBoosterInGame();

            // Expect
            Assert.True(result);
        }

        #endregion

        #region Tests for IsGameFlagEnabled()

        [Fact]
        public void IsGameFlagEnabled_NoMention_ReturnsTrue()
        {
            // Given
            GameFlag flag = Model.GameFlags["f_ZebesAwake"];
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            bool result = logicalOptions.IsGameFlagEnabled(flag);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsGameFlagEnabled_ExplicitlyDisabled_ReturnsFalse()
        {
            // Given
            GameFlag flag = Model.GameFlags["f_ZebesAwake"];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledGameFlag(flag.Name);

            // When
            bool result = logicalOptions.IsGameFlagEnabled(flag);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsGameFlagEnabled_Reenabled_ReturnsTrue()
        {
            // Given
            GameFlag flag = Model.GameFlags["f_ZebesAwake"];
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledGameFlag(flag.Name);
            logicalOptions.UnregisterDisabledGameFlag(flag.Name);

            // When
            bool result = logicalOptions.IsGameFlagEnabled(flag);

            // Expect
            Assert.True(result);
        }

        #endregion

        #region Tests for Clone()

        [Fact]
        public void Clone_CopiesCorectly()
        {
            // Given
            Strat strat = Model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).LinksTo[3].Strats["Ceiling E-Tank Dboost"];
            Tech disabledTech = Model.Techs["canWalljump"];
            Tech enabledTech = Model.Techs["canIBJ"];
            Item item = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            GameFlag flag = Model.GameFlags["f_ZebesAwake"];
            Helper helper = Model.Helpers["h_canBombThings"];

            LogicalOptions logicalOptions = new LogicalOptions();

            int expectedAcidLeniencyMultiplier = 2;
            int expectedHeatLeniencyMultiplier = 3;
            int expectedLavaLeniencyMultiplier = 4;
            logicalOptions.AcidLeniencyMultiplier = expectedAcidLeniencyMultiplier;
            logicalOptions.HeatLeniencyMultiplier= expectedHeatLeniencyMultiplier;
            logicalOptions.LavaLeniencyMultiplier= expectedLavaLeniencyMultiplier;
            logicalOptions.RegisterDisabledStrat(strat.Name);
            logicalOptions.RegisterEnabledTech(enabledTech.Name);
            logicalOptions.RegisterDisabledTech(disabledTech.Name);
            logicalOptions.RegisterRemovedItem(item.Name);
            logicalOptions.RegisterDisabledGameFlag(flag.Name);

            int expectedHelperTries = 5;
            int expectedStratTries = 6;
            int expectedTechTries = 7;
            logicalOptions.RegisterHelperTries(helper.Name, expectedHelperTries);
            logicalOptions.RegisterStratTries(strat.Name, expectedStratTries);
            logicalOptions.RegisterTechTries(enabledTech.Name, expectedTechTries);

            bool expectedShineChargesWithStutter = true;
            bool expectedTechsEnabledByDefault = false;
            int expectedTilesSavedWithStutter = 1;
            int expectedTilesToShineCharge = 20;
            logicalOptions.ShineChargesWithStutter = expectedShineChargesWithStutter;
            logicalOptions.TechsEnabledByDefault = expectedTechsEnabledByDefault;
            logicalOptions.TilesSavedWithStutter = expectedTilesSavedWithStutter;
            logicalOptions.TilesToShineCharge = expectedTilesToShineCharge;

            decimal expectedMinimumEnergyRatePerSecond = 2.5M;
            decimal expectedSafetyMarginPercent = 3.5M;
            Dictionary<ConsumableResourceEnum, decimal> minimumRatesPerSecond = new Dictionary<ConsumableResourceEnum, decimal> {
                { ConsumableResourceEnum.Energy, expectedMinimumEnergyRatePerSecond }
            };
            logicalOptions.InternalSpawnerFarmingOptions = new SpawnerFarmingOptions(minimumRatesPerSecond);
            logicalOptions.InternalSpawnerFarmingOptions.SafetyMarginPercent = expectedSafetyMarginPercent;

            // When
            LogicalOptions clone = logicalOptions.Clone();

            // Expect
            Assert.Equal(expectedAcidLeniencyMultiplier, clone.AcidLeniencyMultiplier);
            Assert.Equal(expectedHeatLeniencyMultiplier, clone.HeatLeniencyMultiplier);
            Assert.Equal(expectedLavaLeniencyMultiplier, clone.LavaLeniencyMultiplier);

            Assert.Contains(strat.Name, clone.DisabledStrats);
            Assert.Contains(enabledTech.Name, clone.EnabledTechs);
            Assert.Contains(disabledTech.Name, clone.DisabledTechs);
            Assert.Contains(item.Name, clone.RemovedItems);
            Assert.Contains(flag.Name, clone.RemovedGameFlags);

            Assert.Equal(expectedHelperTries, clone.NumberOfTries(helper));
            Assert.Equal(expectedStratTries, clone.NumberOfTries(strat));
            Assert.Equal(expectedTechTries, clone.NumberOfTries(enabledTech));

            Assert.Equal(expectedShineChargesWithStutter, clone.ShineChargesWithStutter);
            Assert.Equal(expectedTechsEnabledByDefault, clone.TechsEnabledByDefault);
            Assert.Equal(expectedTilesSavedWithStutter, clone.TilesSavedWithStutter);
            Assert.Equal(expectedTilesToShineCharge, clone.TilesToShineCharge);

            Assert.Equal(expectedMinimumEnergyRatePerSecond, clone.SpawnerFarmingOptions.MinimumRatesPerSecond[ConsumableResourceEnum.Energy]);
            Assert.Equal(expectedSafetyMarginPercent, clone.SpawnerFarmingOptions.SafetyMarginPercent);
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            LogicalOptions clone = logicalOptions.Clone();

            // Subsequently given
            // Modifications to clone
            Strat strat = Model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 1).LinksTo[3].Strats["Ceiling E-Tank Dboost"];
            Tech disabledTech = Model.Techs["canWalljump"];
            Tech enabledTech = Model.Techs["canIBJ"];
            Item item = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            GameFlag flag = Model.GameFlags["f_ZebesAwake"];
            Helper helper = Model.Helpers["h_canBombThings"];

            clone.AcidLeniencyMultiplier = 2;
            clone.HeatLeniencyMultiplier = 3;
            clone.LavaLeniencyMultiplier = 4;
            clone.RegisterDisabledStrat(strat.Name);
            clone.RegisterEnabledTech(enabledTech.Name);
            clone.RegisterDisabledTech(disabledTech.Name);
            clone.RegisterRemovedItem(item.Name);
            clone.RegisterDisabledGameFlag(flag.Name);

            clone.RegisterHelperTries(helper.Name, 5);
            clone.RegisterStratTries(strat.Name, 6);
            clone.RegisterTechTries(enabledTech.Name, 7);

            clone.ShineChargesWithStutter = true;
            clone.TechsEnabledByDefault = false;
            clone.TilesSavedWithStutter = 1;
            clone.TilesToShineCharge = 20;

            Dictionary<ConsumableResourceEnum, decimal> minimumRatesPerSecond = new Dictionary<ConsumableResourceEnum, decimal> {
                { ConsumableResourceEnum.Energy, 2.5M }
            };
            clone.InternalSpawnerFarmingOptions = new SpawnerFarmingOptions(minimumRatesPerSecond);
            clone.InternalSpawnerFarmingOptions.SafetyMarginPercent = 3.5M;

            // Expect
            // Untouched original (default) values in logicalOptions
            Assert.Equal(1, logicalOptions.AcidLeniencyMultiplier);
            Assert.Equal(1, logicalOptions.HeatLeniencyMultiplier);
            Assert.Equal(1, logicalOptions.LavaLeniencyMultiplier);

            Assert.Empty(logicalOptions.DisabledStrats);
            Assert.Empty(logicalOptions.EnabledTechs);
            Assert.Empty(logicalOptions.DisabledTechs);
            Assert.Empty(logicalOptions.RemovedItems);
            Assert.Empty(logicalOptions.RemovedGameFlags);

            Assert.Equal(LogicalOptions.DefaultNumberOfTries, logicalOptions.NumberOfTries(helper));
            Assert.Equal(LogicalOptions.DefaultNumberOfTries, logicalOptions.NumberOfTries(strat));
            Assert.Equal(LogicalOptions.DefaultNumberOfTries, logicalOptions.NumberOfTries(enabledTech));

            Assert.False(logicalOptions.ShineChargesWithStutter);
            Assert.True(logicalOptions.TechsEnabledByDefault);
            Assert.Equal(LogicalOptions.DefaultTilesSavedWithStutter, logicalOptions.TilesSavedWithStutter);
            Assert.Equal(LogicalOptions.DefaultTilesToShineCharge, logicalOptions.TilesToShineCharge);

            Assert.Equal(10, logicalOptions.SpawnerFarmingOptions.MinimumRatesPerSecond[ConsumableResourceEnum.Energy]);
            Assert.Equal(10, logicalOptions.SpawnerFarmingOptions.SafetyMarginPercent);
        }

        #endregion

    }
}
