using sm_json_data_framework.Models.Raw.Rooms.Nodes;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Rooms.Nodes
{
    /// <summary>
    /// Contains the address of a door that is an in-game twin of another door. Both doors lead to the same destination.
    /// This is mainly a way to represent Pants Room vs. East Pants Room.
    /// </summary>
    public class TwinDoorAddress : AbstractModelElement<UnfinalizedTwinDoorAddress, TwinDoorAddress>
    {
        public TwinDoorAddress(UnfinalizedTwinDoorAddress sourceElement, Action<TwinDoorAddress> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {
            RoomAddress = sourceElement.RoomAddress;
            DoorAddress = sourceElement.DoorAddress;
        }

        /// <summary>
        /// The in-game address of the room in which this twin is found.
        /// </summary>
        public string RoomAddress { get; }

        /// <summary>
        /// The in-game address of the this twin door.
        /// </summary>
        public string DoorAddress { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }

        public override bool CalculateLogicallyRelevant(SuperMetroidRules rules)
        {
            // A TwinDoorAddress is just data with no logical implications.
            return false;
        }
    }

    public class UnfinalizedTwinDoorAddress: AbstractUnfinalizedModelElement<UnfinalizedTwinDoorAddress, TwinDoorAddress>
    {
        public string RoomAddress { get; set; }

        public string DoorAddress { get; set; }

        public UnfinalizedTwinDoorAddress()
        {

        }

        public UnfinalizedTwinDoorAddress(RawTwinDoorAddress rawTwinAddress)
        {
            DoorAddress = rawTwinAddress.DoorAddress;
            RoomAddress = rawTwinAddress.RoomAddress;
        }

        protected override TwinDoorAddress CreateFinalizedElement(UnfinalizedTwinDoorAddress sourceElement, Action<TwinDoorAddress> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new TwinDoorAddress(sourceElement, mappingsInsertionCallback);
        }
    }
}
