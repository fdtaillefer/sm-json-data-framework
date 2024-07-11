using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Requirements.StringRequirements
{
    public class ItemLogicalElementTest
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
            ItemLogicalElement itemLogicalElement = model.Helpers["h_canBombThings"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Morph");
            Assert.Same(model.Items["Morph"], itemLogicalElement.Item);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_ItemNotPresent_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ItemLogicalElement itemLogicalElement = model.Helpers["h_canBombThings"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Morph");
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = itemLogicalElement.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ItemPresent_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ItemLogicalElement itemLogicalElement = model.Helpers["h_canBombThings"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Morph");
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Morph");

            // When
            ExecutionResult result = itemLogicalElement.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("Morph")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_ItemPresentButLogicallyDisabled_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            ItemLogicalElement itemLogicalElement = model.Helpers["h_canBombThings"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Morph");
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Morph");
            model.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Morph");

            // When
            ExecutionResult result = itemLogicalElement.Execute(model, inGameState);

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
            logicalOptions.RegisterRemovedItem("Bombs");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["Morph"])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            ItemLogicalElement freeItemElement = model.Helpers["h_canBombThings"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Morph");
            Assert.True(freeItemElement.LogicallyRelevant);
            Assert.False(freeItemElement.LogicallyNever);
            Assert.True(freeItemElement.LogicallyAlways);
            Assert.True(freeItemElement.LogicallyFree);

            ItemLogicalElement removedItemElement = model.Helpers["h_canUseMorphBombs"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "Bombs");
            Assert.True(removedItemElement.LogicallyRelevant);
            Assert.True(removedItemElement.LogicallyNever);
            Assert.False(removedItemElement.LogicallyAlways);
            Assert.False(removedItemElement.LogicallyFree);

            ItemLogicalElement obtainableItemElement = model.Helpers["h_canUseSpringBall"].Requires.LogicalElement<ItemLogicalElement>(0, element => element.Item.Name == "SpringBall");
            Assert.True(obtainableItemElement.LogicallyRelevant);
            Assert.False(obtainableItemElement.LogicallyNever);
            Assert.False(obtainableItemElement.LogicallyAlways);
            Assert.False(obtainableItemElement.LogicallyFree);
        }

        #endregion
    }
}
