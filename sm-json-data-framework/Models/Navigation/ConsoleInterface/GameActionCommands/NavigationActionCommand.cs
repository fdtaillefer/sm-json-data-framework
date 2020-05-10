using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation.ConsoleInterface.GameActionCommands
{
    /// <summary>
    /// An interface for console interface commands which end up executing an <see cref="AbstractNavigationAction"/>.
    /// Contains options for how much to output to the console when executing.
    /// </summary>
    interface NavigationActionCommand
    {
        public bool OutputAnything { get; set; }

        public bool OutputEffects { get; set; }

        public bool OutputDetails { get; set; }
    }
}
