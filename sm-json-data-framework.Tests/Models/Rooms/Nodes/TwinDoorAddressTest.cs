using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.Rooms.Nodes
{
    public class TwinDoorAddressTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            TwinDoorAddress twinDoorAddress = Model.Rooms["East Pants Room"].Nodes[2].TwinDoorAddresses.First();
            Assert.Equal("0x7D646", twinDoorAddress.RoomAddress);
            Assert.Equal("0x001a798", twinDoorAddress.DoorAddress);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            TwinDoorAddress twinDoorAddress = ModelWithOptions.Rooms["East Pants Room"].Nodes[2].TwinDoorAddresses.First();
            Assert.False(twinDoorAddress.LogicallyRelevant);
        }

        #endregion
    }
}
