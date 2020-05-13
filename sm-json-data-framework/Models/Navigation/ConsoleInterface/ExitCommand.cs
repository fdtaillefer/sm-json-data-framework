using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface
{
    /// <summary>
    /// A console interface command, to quit using the interface.
    /// </summary>
    public class ExitCommand: ConsoleCommand
    {
        public ExitCommand()
        {
            Name = "Exit";
            SampleFormat = "x";
            Description = "Stops console navigation";
            ValidCommand = str => str.ToLower() == "x";
            Execution = (navigator, command) => false;
        }
    }
}
