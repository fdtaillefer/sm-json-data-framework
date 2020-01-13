using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Rooms
{
    public class Strat
    {
        public string Name { get; set; }

        public bool Notable { get; set; }

        public LogicalRequirements Requires { get; set; }

        public IEnumerable<StratObstacle> Obstacles { get; set; } = Enumerable.Empty<StratObstacle>();

        public IEnumerable<StratFailure> Failures { get; set; } = Enumerable.Empty<StratFailure>();

        public IEnumerable<string> StratProperties { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Initializes additional properties in this Strat, which wouldn't be initialized by simply parsing a rooms json file.
        /// All such properties are identified in their own documentation and should not be read if this method isn't called.
        /// </summary>
        /// <param name="model">The model to use to initialize the additional properties</param>
        /// <param name="room">The room in which this Strat is</param>
        public void Initialize(SuperMetroidModel model, Room room)
        {
            foreach(StratFailure failure in Failures)
            {
                failure.Initialize(model, room);
            }

            foreach(StratObstacle obstacle in Obstacles)
            {
                obstacle.Initialize(model, room);
            }
        }

        /// <summary>
        /// Goes through all logical elements within this Strat (and all LogicalRequirements within any of them),
        /// attempting to initialize any property that is an object referenced by another property(which is its identifier).
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <param name="room">The room in which this strat is</param>
        /// <returns>A sequence of strings describing references that could not be initialized properly.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(Requires.InitializeReferencedLogicalElementProperties(model, room));

            foreach(StratObstacle obstacle in Obstacles)
            {
                unhandled.AddRange(obstacle.InitializeReferencedLogicalElementProperties(model, room));
            }

            foreach(StratFailure failure in Failures)
            {
                unhandled.AddRange(failure.InitializeReferencedLogicalElementProperties(model, room));
            }

            return unhandled.Distinct();
        }
    }
}
