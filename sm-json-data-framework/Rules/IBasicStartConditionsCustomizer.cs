using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Rules
{
    /// <summary>
    /// An interface that offers a way to modify a <see cref="BasicStartConditions"/> instance after it has been created.
    /// </summary>
    public interface IBasicStartConditionsCustomizer
    {
        public void Customize(BasicStartConditions basicStartConditions);
    }
}
