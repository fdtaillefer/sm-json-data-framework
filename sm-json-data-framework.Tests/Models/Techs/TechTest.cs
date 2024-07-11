using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Techs
{
    public class TechTest
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
            Tech tech = model.Techs["canXRayClimb"];
            Assert.Equal("canXRayClimb", tech.Name);
            Assert.NotNull(tech.Requires);
            Assert.Equal(1, tech.Requires.LogicalElements.Count());
            Assert.NotNull(tech.Requires.LogicalElement<TechLogicalElement>(0));

            Assert.Equal(2, tech.ExtensionTechs.Count);
            Assert.True(tech.ExtensionTechs.ContainsKey("canRightFacingDoorXRayClimb"));
            Assert.True(tech.ExtensionTechs.ContainsKey("canLeftFacingDoorXRayClimb"));
        }

        #endregion

        #region Tests for SelectionWithExtensions()

        [Fact]
        public void SelectWithExtensions_ReturnsSelfAndAllDescendants()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Tech tech = model.Techs["canTrickyUseFrozenEnemies"];

            // When
            IEnumerable<Tech> techs = tech.SelectWithExtensions();

            // Expect
            Assert.True(techs.Contains(tech, ReferenceEqualityComparer.Instance)); // Self
            Assert.True(techs.Contains(model.Techs["canNonTrivialIceClip"], ReferenceEqualityComparer.Instance)); // Child
            Assert.True(techs.Contains(model.Techs["canWallCrawlerIceClip"], ReferenceEqualityComparer.Instance)); // Grandchild
            Assert.False(techs.Contains(model.Techs["canUseFrozenEnemies"], ReferenceEqualityComparer.Instance)); // Parent
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledTech("canPreciseWalljump");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["Morph"])
                    .ApplyAddItem(model.Items["Bombs"])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Tech disabledTech = model.Techs["canPreciseWalljump"];
            Assert.False(disabledTech.LogicallyRelevant);
            Assert.False(disabledTech.LogicallyAlways);
            Assert.False(disabledTech.LogicallyFree);
            Assert.True(disabledTech.LogicallyNever);

            Tech impossibleSubTech = model.Techs["canDelayedWalljump"];
            Assert.False(impossibleSubTech.LogicallyRelevant);
            Assert.False(impossibleSubTech.LogicallyAlways);
            Assert.False(impossibleSubTech.LogicallyFree);
            Assert.True(impossibleSubTech.LogicallyNever);

            Tech nonFreeTech = model.Techs["canGrappleClip"];
            Assert.True(nonFreeTech.LogicallyRelevant);
            Assert.False(nonFreeTech.LogicallyAlways);
            Assert.False(nonFreeTech.LogicallyFree);
            Assert.False(nonFreeTech.LogicallyNever);

            Tech freeTech = model.Techs["canWalljump"];
            Assert.True(freeTech.LogicallyRelevant);
            Assert.True(freeTech.LogicallyAlways);
            Assert.True(freeTech.LogicallyFree);
            Assert.False(freeTech.LogicallyNever);

            Tech freeByStartItemTech = model.Techs["canIBJ"];
            Assert.True(freeByStartItemTech.LogicallyRelevant);
            Assert.True(freeByStartItemTech.LogicallyAlways);
            Assert.True(freeByStartItemTech.LogicallyFree);
            Assert.False(freeByStartItemTech.LogicallyNever);
        }

        #endregion
    }
}
