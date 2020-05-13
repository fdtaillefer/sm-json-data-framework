using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.InformationCommands
{
    /// <summary>
    /// A console interface command, to output all known commands.
    /// </summary>
    public class HelpCommand: ConsoleCommand
    {
        private List<ConsoleCommand> Commands { get; set; }

        public HelpCommand(List<ConsoleCommand> commands)
        {
            Commands = commands;
            Name = "Help";
            SampleFormat = "h";
            Description = "Outputs existing commands";
            ValidCommand = str => str.ToLower() == "h";
            Execution = (navigator, command) => { 
                foreach(ConsoleCommand currentCommand in Commands)
                {
                    Console.WriteLine($"{currentCommand.SampleFormat} ({currentCommand.Name}): {currentCommand.Description}");
                }
                return true;
            };
        }
    }
}
