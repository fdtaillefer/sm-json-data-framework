using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.Arrays
{
    /// <summary>
    /// Abstract base class for raw logical elements that are composed (in the json) of a single array of elements of the same type.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array</typeparam>
    public abstract class AbstractRawObjectLogicalElementWithArray<T> : AbstractRawObjectLogicalElement
    {
        public AbstractRawObjectLogicalElementWithArray(IList<T> items)
        {
            Value = new(items);
        }

        public List<T> Value { get; set; }
    }
}
