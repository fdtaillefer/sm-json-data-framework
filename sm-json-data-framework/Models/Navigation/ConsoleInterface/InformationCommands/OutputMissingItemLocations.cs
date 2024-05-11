using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.InformationCommands
{
    /// <summary>
    /// A console interface command, to output all item locations that haven't been picked up.
    /// </summary>
    public class OutputMissingItemLocations: ConsoleCommand
    {
        public OutputMissingItemLocations()
        {
            Name = "Status - Missing Item Locations";
            SampleFormat = "smil";
            Description = "Outputs the list of item locations that haven't been picked up yet";
            ValidCommand = str => str.ToLower() == "smil";
            Execution = (navigator, command) =>
            {
                IEnumerable<UnfinalizedRoomNode>
                           missingItemNodes = navigator.GameModel.Nodes.Values
                           .Where(node => node.NodeType == NodeTypeEnum.Item)
                           .Except(navigator.CurrentInGameState.TakenItemLocations.Values)
                           .OrderBy(node => node.Name);

                // Output missing item location count
                Console.WriteLine($"Number of pickups not obtained: {missingItemNodes.Count()}");

                // Output missing item locations
                // Output missing non-consumable items
                foreach (UnfinalizedRoomNode node in missingItemNodes)
                {
                    Console.WriteLine($"Missing item location '{node.Name}' in room '{node.Room.Name}' (containing item '{node.NodeItemName}')");
                }

                return true;
            };
        }
    }
}
