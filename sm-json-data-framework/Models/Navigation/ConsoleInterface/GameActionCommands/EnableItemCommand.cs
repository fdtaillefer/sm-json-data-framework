using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands
{
    /// <summary>
    /// A console interface command, to enable an item.
    /// </summary>
    public class EnableItemCommand : ConsoleCommand, NavigationActionCommand
    {
        public bool OutputAnything { get; set; } = true;

        public bool OutputEffects { get; set; } = true;

        public bool OutputDetails { get; set; } = true;

        public EnableItemCommand()
        {
            Regex enableRegex = new Regex(@"^e [a-zA-Z]+");

            Name = "Enable item";
            string commandIdentifier = "e";
            SampleFormat = $"{commandIdentifier} ItemName";
            Description = "Enables the item with the given name";
            ValidCommand = command => enableRegex.IsMatch(command.ToLower());
            Execution = (navigator, command) => {
                string itemName = enableRegex.Match(command).Value.Substring(commandIdentifier.Length + 1);
                AbstractNavigationAction action = navigator.EnableItem(itemName);
                if (OutputAnything)
                {
                    action.OutputToConsole(OutputEffects, OutputDetails);
                }
                return true;
            };
        }
    }
}
