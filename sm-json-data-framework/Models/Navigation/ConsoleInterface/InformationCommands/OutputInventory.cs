using sm_json_data_framework.Models.Items;
using System;
using System.Collections.Generic;
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
            ValidCommand = str => str == "si";
            Execution = (navigator, command) =>
            {
                foreach (var (item, count) in navigator.CurrentInGameState.GetExpansionItemsDictionary().Values)
                {
                    Console.WriteLine($"Has item '{item.Name}' X {count}");
                }
                foreach (Item item in navigator.CurrentInGameState.GetNonConsumableItemsDictionary().Values)
                {
                    Console.WriteLine($"Has item '{item.Name}'");
                }

                return true;
            };
        }
    }
}
