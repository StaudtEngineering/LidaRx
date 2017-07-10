using System;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;

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

        public R2000ParameterInfoAttribute(R2000ParameterType parameterAccessQualifier)
        {
            this.AccessType = parameterAccessQualifier;
            this.MinProtocolVersion = R2000ProtocolVersion.Any;
            this.MaxProtocolVersion = R2000ProtocolVersion.Any;
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
        /// All/Any protocol version
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
        public static string[] GetR2000ParametersList(this Type type, R2000ParameterType parameterTypes = R2000ParameterType.All)
        {
            var fieldNames = type.GetRuntimeProperties()
                // get only fields where we find a R2000ParameterTypeAttribute
                .Where(field =>
                {
                    var parameterTypeAttribute = field.GetCustomAttributes<R2000ParameterInfoAttribute>();

                    return parameterTypeAttribute.Any() && parameterTypeAttribute.All(x => parameterTypes.HasFlag(x.AccessType));
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

    }
}
