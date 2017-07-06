using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Serialization
{
    /// <summary>
    /// Dto for list_parameters
    /// 
    /// Url:
    /// http://*sensor IP address*/cmd/list_parameters
    /// </summary>
    class ParameterList
    {
        public string[] Parameters { get; set; }
        public R2000ErrorCode ErrorCode { get; set; }
        public string ErrorText { get; set; }
    }
}
