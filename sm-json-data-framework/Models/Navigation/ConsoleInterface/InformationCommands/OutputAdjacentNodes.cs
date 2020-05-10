using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.InformationCommands
{
    /// <summary>
    /// A console interface command, to output all nodes that have a navigable link from the current node.
    /// </summary>
    public class OutputAdjacentNodes: ConsoleCommand
    {
        public OutputAdjacentNodes()
        {
            Name = "Status - Adjacent Nodes";
            SampleFormat = "san";
            Description = "Outputs nodes that current node links to";
            ValidCommand = str => str == "san";
            Execution = (navigator, command) =>
            {
                IEnumerable<LinkTo> links = navigator.CurrentInGameState.GetCurrentNode().Links.Values;
                foreach (LinkTo link in navigator.CurrentInGameState.GetCurrentNode().Links.Values)
                {
                    Console.WriteLine($"Adjacent node {link.TargetNodeId}: {link.TargetNode.Name}");
                }
                if(!links.Any())
                {
                    Console.WriteLine("No adjacent nodes");
                }

                return true;
            };
        }
    }
}
