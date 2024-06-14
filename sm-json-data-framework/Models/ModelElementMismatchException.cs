using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models
{
    /// <summary>
    /// An exception that expresses that an instance did not belong to the expected <see cref="SuperMetroidModel"/>, 
    /// suggesting it belongs to a different one and cannot be used in the context.
    /// </summary>
    public class ModelElementMismatchException: Exception
    {
        public ModelElementMismatchException(RoomNode node) : base($"Node '{node.Name}' has a different instance in context's SuperMetroidModel. " +
                    $"Did you pass a node that belongs to a different SuperMetroidModel instance?")
        {

        }

        public ModelElementMismatchException(Item item): base($"Item '{item.Name}' has a different instance in context's SuperMetroidModel. " +
                        $"Did you pass elements that belong to a different SuperMetroidModel instance?")
        {

        }

        public ModelElementMismatchException(GameFlag gameFlag): base($"Game flag '{gameFlag.Name}' has a different instance in context's SuperMetroidModel. " +
                        $"Did you pass elements that belong to a different SuperMetroidModel instance?")
        {

        }

        public ModelElementMismatchException(NodeLock nodeLock): base($"Node lock '{nodeLock.Name}' has a different instance in context's SuperMetroidModel. " +
                        $"Did you pass elements that belong to a different SuperMetroidModel instance?")
        {

        }
    }
}
