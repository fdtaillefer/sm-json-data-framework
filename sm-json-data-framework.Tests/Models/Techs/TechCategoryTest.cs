using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.Techs
{
    public class TechCategoryTest
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
            TechCategory techCategory = model.TechCategories["General"];
            Assert.Equal("General", techCategory.Name);
            Assert.Equal("General configuration techs", techCategory.Description);
            Assert.Equal(5, techCategory.FirstLevelTechs.Count);
            Assert.True(techCategory.FirstLevelTechs.ContainsKey("canHeatRun"));
            Assert.False(techCategory.FirstLevelTechs.ContainsKey("canWaterBreakFree"));
            Assert.Equal(8, techCategory.Techs.Count);
            Assert.True(techCategory.Techs.ContainsKey("canHeatRun"));
            Assert.True(techCategory.Techs.ContainsKey("canWaterBreakFree"));
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterRemovedItem(SuperMetroidModel.SPEED_BOOSTER_NAME);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            TechCategory disabledTechsCategory = model.TechCategories["Speed Booster"];
            Assert.False(disabledTechsCategory.LogicallyRelevant);
            TechCategory normalTechsCategory = model.TechCategories["General"];
            Assert.True(normalTechsCategory.LogicallyRelevant);
        }

        #endregion
    }
}
