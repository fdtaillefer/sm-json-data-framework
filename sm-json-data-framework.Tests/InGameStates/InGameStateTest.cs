using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Reading;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.InGameStates
{
    public class InGameStateTest
    {
        // Use a static model to read it only once.
        private static SuperMetroidModel Model { get; set; } = ModelReader.ReadModel();

        [Fact]
        public void ConstructorWithStartConditions_InitializesProperly()
        {
            string startingRoomName = "Business Center";
            int startingNodeId = 1;
            string startingLockName = "Business Center Top Left Green Lock (to Ice Beam Gate)";
            int startingEnergy = 50;
            string maridiaTubeFlag = "f_MaridiaTubeBroken";
            RoomNode variaNode = Model.GetNodeInRoom("Varia Suit Room", 2);
            StartConditions startConditions = new StartConditions
            {
                StartingGameFlags = new GameFlag[] { Model.GameFlags[maridiaTubeFlag] },
                StartingInventory = ItemInventory.CreateVanillaStartingInventory(Model).ApplyAddItem(Model.Items[SuperMetroidModel.VARIA_SUIT_NAME]),
                StartingNode = Model.GetNodeInRoom(startingRoomName, startingNodeId),
                StartingOpenLocks = new NodeLock[] { Model.Locks[startingLockName] },
                StartingTakenItemLocations = new RoomNode[] { variaNode },
                StartingResources = new ResourceCount().ApplyAmountIncrease(RechargeableResourceEnum.RegularEnergy, startingEnergy)
            };

            InGameState inGameState = new InGameState(startConditions);

            Assert.Equal(startingRoomName, inGameState.GetCurrentRoom().Name);
            Assert.Equal(startingNodeId, inGameState.GetCurrentNode().Id);
            Assert.Contains(startingLockName, inGameState.GetOpenedLocksDictionary());
            Assert.True(inGameState.HasVariaSuit());
            Assert.True(inGameState.IsItemLocationTaken(variaNode));
            Assert.Equal(startConditions.BaseResourceMaximums.GetAmount(RechargeableResourceEnum.RegularEnergy),
                inGameState.GetMaxAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(startingEnergy, inGameState.GetCurrentAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.True(inGameState.HasGameFlag(maridiaTubeFlag));
        }
    }
}
