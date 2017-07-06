using System;
using System.Collections.Generic;
using System.Text;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Serialization
{
    public enum R2000ErrorCode
    {
        /// <summary>
        /// Success
        /// </summary>
        Success = 0,

        /// <summary>
        /// Unknown argument %s
        /// </summary>
        UnknownArgument = 100,

        /// <summary>
        /// Unknown parameter %s
        /// </summary>
        UnknownParameter = 110,

        /// <summary>
        /// Invalid handle or no handle provided
        /// </summary>
        InvalidHandle = 120,

        /// <summary>
        /// Required argument missing
        /// </summary>
        ArgumentMissing = 130,

        /// <summary>
        /// Invalid value for parameter %s
        /// </summary>
        InvalidValue = 200,

        /// <summary>
        /// Value for parameter %s is out of range
        /// </summary>
        ValueOutOfRange = 210,

        /// <summary>
        /// Write access to read only parameter %s
        /// </summary>
        WriteToReadOnlyParameter = 220,

        /// <summary>
        /// Insufficient memory
        /// </summary>
        OutOfMemory = 230,

        /// <summary>
        /// Ressource still/already in use
        /// </summary>
        RessourceInUse = 240,

        /// <summary>
        /// Internal error while processing command %s
        /// </summary>
        InternalError = 333,

        /// <summary>
        /// Not initialized value
        /// </summary>
        Undefined = -1
    }
}
