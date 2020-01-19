using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Utils
{
    public static class SuperMetroidUtils
    {
        /// <summary>
        /// Builds and returns a string to identify the node with the provided id in the provided room. This can be used as a key in a dictionary.
        /// </summary>
        /// <param name="roomName">The name of the room in which the node is found</param>
        /// <param name="nodeId">The ID (within the room) of the node</param>
        /// <returns></returns>
        public static string BuildNodeIdentifyingString(string roomName, int nodeId)
        {
            return $"{roomName}_{nodeId}";
        }
    }
}
