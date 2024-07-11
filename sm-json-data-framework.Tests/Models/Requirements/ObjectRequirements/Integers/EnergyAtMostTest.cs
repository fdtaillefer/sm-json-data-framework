using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.Integers
{
    public class EnergyAtMostTest
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
            EnergyAtMost energyAtMost = model.Locks["Ceres Ridley Fight"].UnlockStrats["Base"].Requires.LogicalElement<EnergyAtMost>(0);
            Assert.Equal(29, energyAtMost.Amount);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_EnergyHigherThanAmount_ReducesEnergy()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnergyAtMost energyAtMost = model.Locks["Ceres Ridley Fight"].UnlockStrats["Base"].Requires.LogicalElement<EnergyAtMost>(0);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = energyAtMost.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -70)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_EnergyExactlyAmount_SucceedsForFree()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnergyAtMost energyAtMost = model.Locks["Ceres Ridley Fight"].UnlockStrats["Base"].Requires.LogicalElement<EnergyAtMost>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 70);

            // When
            ExecutionResult result = energyAtMost.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_EnergyLessThanAmount_SucceedsForFree()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            EnergyAtMost energyAtMost = model.Locks["Ceres Ridley Fight"].UnlockStrats["Base"].Requires.LogicalElement<EnergyAtMost>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 95);

            // When
            ExecutionResult result = energyAtMost.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
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
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums();
            baseResouces.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 29);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(baseResouces)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items["PowerBomb"], 10);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            EnergyAtMost freeEnergyAtMost = model.Locks["Ceres Ridley Fight"].UnlockStrats["Base"].Requires.LogicalElement<EnergyAtMost>(0);
            Assert.True(freeEnergyAtMost.LogicallyRelevant);
            Assert.False(freeEnergyAtMost.LogicallyNever);
            Assert.True(freeEnergyAtMost.LogicallyAlways);
            Assert.True(freeEnergyAtMost.LogicallyFree);

            EnergyAtMost possibleEnergyAtMost = model.Rooms["Big Boy Room"].Links[2].To[1].Strats["Get Drained"].Requires.LogicalElement<EnergyAtMost>(0);
            Assert.True(possibleEnergyAtMost.LogicallyRelevant);
            Assert.False(possibleEnergyAtMost.LogicallyNever);
            Assert.True(possibleEnergyAtMost.LogicallyAlways);
            Assert.False(possibleEnergyAtMost.LogicallyFree);
        }

        #endregion
    }
}
