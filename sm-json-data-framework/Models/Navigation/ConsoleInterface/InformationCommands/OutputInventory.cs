using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.InformationCommands
{
    /// <summary>
    /// A console interface command, to output all items in the current inventory.
    /// </summary>
    public class OutputInventory: ConsoleCommand
    {
        public OutputInventory()
        {
            Name = "Status - Inventory";
            SampleFormat = "si";
            Description = "Outputs current items in inventory";
            ValidCommand = str => str.ToLower() == "si";
            Execution = (navigator, command) =>
            {
                // Output pickup count
                int pickupsCount = navigator.CurrentInGameState.TakenItemLocations.Count;
                Console.WriteLine($"Number of pickups obtained: {pickupsCount}");

                // Output expansion items with counts
                foreach (var (item, count) in navigator.CurrentInGameState.Inventory.ExpansionItems.Values)
                {
                    Console.WriteLine($"Has item '{item.Name}' X {count}");
                }
                // Output non consumable items
                foreach (Item item in navigator.CurrentInGameState.Inventory.NonConsumableItems.Values)
                {
                    Console.WriteLine($"Has item '{item.Name}'{(navigator.CurrentInGameState.Inventory.IsItemDisabled(item.Name) ? " (disabled)" : "")}");
                }

                return true;
            };
        }
    }
}
