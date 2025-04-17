using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Resolver to customize JSON serialization of SDK configurations at build time.
    /// </summary>
    public class SDKConfigContractResolver : DefaultContractResolver
    {
        private readonly string[] _skipPropertyNames =
        {
            "name",
            "hideFlags"
        };


        /// <summary>
        /// Strips specific inherited scriptable object fields from the output JSON.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> allProperties = base.CreateProperties(type, memberSerialization);

            var allowedProperties = new List<JsonProperty>();
            foreach (var property in allProperties)
            {
                if (!_skipPropertyNames.Contains(property.PropertyName))
                {
                    allowedProperties.Add(property);
                }
            }

            return allowedProperties;
        }

        /// <summary>
        /// Determine whether to serialize fields based on whether they are set to non-default values.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType == typeof(bool))
            {
                var field = property.DeclaringType.GetField(property.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var boolAttr = field.GetCustomAttribute<OverrideBooleanAttribute>();

                property.ShouldSerialize =
                    instance =>
                    {
                        // If a boolean field has the OverrideBoolean attribute applied, it will be serialized only if
                        // it differs from the default value specified in the attribute. Otherwise,
                        // the field is unconditionally included.
                        var boolValue = (bool)ReflectionUtil.GetMemberObject(member, instance);
                        return boolAttr == null || boolAttr.defaultValue != boolValue;
                    };
            }
            else if (property.PropertyType == typeof(string))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        var stringValue = (string)ReflectionUtil.GetMemberObject(member, instance);
                        return !string.IsNullOrEmpty(stringValue);
                    };
            }
            else if (property.PropertyType == typeof(int))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        var intValue = (int)ReflectionUtil.GetMemberObject(member, instance);
                        return intValue > 0;
                    };
            }
            else if (ReflectionUtil.IsSubclassOfRawGeneric(property.PropertyType, typeof(List<>)))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        var listValue = (IList)ReflectionUtil.GetMemberObject(member, instance);
                        return listValue.Count > 0;
                    };
            }
            else if (property.PropertyType.GetInterfaces().Contains(typeof(IJsonSerializable)))
            {
                // If a field's type implements the IJsonSerializable interface, get a reference
                // to the underlying member and invoke it's ShouldSerialize() method to determine
                // if the field should be written to JSON.
                property.ShouldSerialize =
                    instance =>
                    {
                        var memberObj = ReflectionUtil.GetMemberObject(member, instance);

                        var shouldSerialize =
                            (bool)typeof(IJsonSerializable).InvokeMember(
                                "ShouldSerialize",
                                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public,
                                null,
                                memberObj,
                                null
                            );

                        return shouldSerialize;
                    };
            }
            else
            {
                property.ShouldSerialize = instance => false;
            }

            return property;
        }
    }
}