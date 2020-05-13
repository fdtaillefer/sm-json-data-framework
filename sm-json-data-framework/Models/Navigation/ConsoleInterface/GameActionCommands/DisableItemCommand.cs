using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands
{
    /// <summary>
    /// A console interface command, to disable an item.
    /// </summary>
    public class DisableItemCommand: ConsoleCommand, NavigationActionCommand
    {
        public bool OutputAnything { get; set; } = true;

        public bool OutputEffects { get; set; } = true;

        public bool OutputDetails { get; set; } = true;

        public DisableItemCommand()
        {
            Regex disableRegex = new Regex(@"^d [a-zA-Z]+");

            Name = "Disable item";
            SampleFormat = "d ItemName";
            Description = "Disables the item with the given name";
            ValidCommand = command => disableRegex.IsMatch(command.ToLower());
            Execution = (navigator, command) => {
                string itemName = disableRegex.Match(command).Value.Substring(2);
                AbstractNavigationAction action = navigator.DisableItem(itemName);
                if (OutputAnything)
                {
                    action.OutputToConsole(OutputEffects, OutputDetails);
                }
                return true;
            };
        }
    }
}
