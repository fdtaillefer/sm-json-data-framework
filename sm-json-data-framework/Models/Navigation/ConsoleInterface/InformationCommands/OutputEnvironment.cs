using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.InformationCommands
{
    public class OutputEnvironment : ConsoleCommand
    {
        public OutputEnvironment()
        {
            Name = "Status - Current Environment";
            SampleFormat = "env";
            Description = "Outputs environment at current node";
            ValidCommand = str => str.ToLower() == "env";
            Execution = (navigator, command) =>
            {
                Console.WriteLine($"Room is{(navigator.CurrentInGameState.IsHeatedRoom() ? "" : " not")} heated.");
                return true;
            };
        }
    }
}
