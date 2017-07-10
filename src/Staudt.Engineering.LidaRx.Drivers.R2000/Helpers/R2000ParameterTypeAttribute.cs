using System;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using Staudt.Engineering.LidaRx.Drivers.R2000.Serialization;

namespace Staudt.Engineering.LidaRx.Drivers.R2000.Helpers
{
    class R2000ParameterInfoAttribute : Attribute
    {
        /// <summary>
        /// What is the access qualifier of this parameter
        /// </summary>
        public R2000ParameterType AccessType { get; private set; }

        /// <summary>
        /// Parameter only present in devices with protocol version &gt;= than
        /// </summary>
        public R2000ProtocolVersion MinProtocolVersion { get; private set; }

        /// <summary>
        /// Parameter only present in devices with protocol version &lt; than
        /// </summary>
        public R2000ProtocolVersion MaxProtocolVersion { get; private set; }


        public R2000ParameterInfoAttribute(
            R2000ParameterType parameterAccessType,
            R2000ProtocolVersion minVersion = R2000ProtocolVersion.Any, 
            R2000ProtocolVersion maxVersion = R2000ProtocolVersion.Any)
        {
            this.AccessType = parameterAccessType;
            this.MinProtocolVersion = minVersion;
            this.MaxProtocolVersion = maxVersion;
        }
    }

    [Flags]
    enum R2000ParameterType
    {
        ReadOnlyStatic,
        ReadOnly,
        ReadWrite,
        Volatile,

        // combine all flags
        All = ReadOnly | ReadOnlyStatic | ReadWrite | Volatile
    }

    /// <summary>
    /// R2000 ethernet protocol version
    /// </summary>
    enum R2000ProtocolVersion
    {
        /// <summary>
        /// V1.00
        /// </summary>
        v100 = 100,

        /// <summary>
        /// V1.01
        /// </summary>
        v101 = 101,

        /// <summary>
        /// V1.02
        /// </summary>
        v102 = 102,

        /// <summary>
        /// Any protocol version
        /// </summary>
        Any
    }

    static class R2000ParameterTypeHelper
    {
        /// <summary>
        /// Get the list of "R2000 parameters" in this type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string[] GetR2000ParametersList(
            this Type type,
            R2000ProtocolVersion protocolVersion,
            R2000ParameterType accessorTypes = R2000ParameterType.All)
        {
            var fieldNames = type.GetRuntimeProperties()
                // get only fields where we find a R2000ParameterTypeAttribute with the required filter
                .Where(field =>
                {
                    var parameterTypeAttribute = field.GetCustomAttributes<R2000ParameterInfoAttribute>();

                    if (!parameterTypeAttribute.Any())
                        return false;

                    var accessorsOk = parameterTypeAttribute.All(x => accessorTypes.HasFlag(x.AccessType));
                    var minVersionOk = parameterTypeAttribute.All(x => x.MinProtocolVersion == R2000ProtocolVersion.Any || x.MinProtocolVersion <= protocolVersion);
                    var maxVersionOk = parameterTypeAttribute.All(x => x.MaxProtocolVersion == R2000ProtocolVersion.Any || x.MaxProtocolVersion >= protocolVersion);

                    return accessorsOk && minVersionOk && maxVersionOk;
                })
                // select either the JsonProperty.PropertyName or the Fields true name as fallback
                .Select(field =>
                {
                    var jsonAttribute = field.GetCustomAttribute<JsonPropertyAttribute>();

                    if (jsonAttribute != null)
                        return jsonAttribute.PropertyName;
                    else
                        return field.Name;
                });

            return fieldNames.ToArray();
        }

        /// <summary>
        /// Convert Major/Minor version info in the ProtocolInformation object to a known R2000ProtocolVersion
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static R2000ProtocolVersion GetProtocolVersion(this ProtocolInformation pi)
        {
            var asInt = pi.VersionMajor * 100 + pi.VersionMinor;
            return (R2000ProtocolVersion)asInt;
        }

    }
}
