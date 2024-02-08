using System;
using System.Reflection;

namespace EmbraceSDK.EditorView
{
    public static class ReflectionUtil
    {
        /// <summary>
        /// Gets the public and private instance fields declared in a specific type.  Does not include inherited fields.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FieldInfo[] GetDeclaredInstanceFields(Type type)
        {
            return type.GetFields(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly |
                BindingFlags.Instance
            );
        }

        /// <summary>
        /// Gets the underlying object reference for a reflected member.
        /// </summary>
        /// <param name="member">The reflected MemberInfo object</param>
        /// <param name="instance">The instance the member belongs to</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static object GetMemberObject(MemberInfo member, object instance)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).GetValue(instance);
                case MemberTypes.Property:
                    return ((PropertyInfo)member).GetValue(instance);
                default:
                    throw new ArgumentException
                    (
                        "Input MemberInfo must be if type FieldInfo, or PropertyInfo"
                    );
            }
        }

        /// <summary>
        /// Checks if a type is a subclass of a generic type
        /// </summary>
        /// <param name="toCheck">The type to evaluate</param>
        /// <param name="generic">The generic type to compare against (e.g. List<> or Dictionary<,>)</param>
        /// <returns></returns>
        public static bool IsSubclassOfRawGeneric(Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var currentType = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == currentType)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Determines if a configuration instance contains boolean overrides
        /// </summary>
        /// <returns></returns>
        public static bool HasBooleanOverrides(Type type, object instance)
        {
            var hasOverrides = false;

            var fields = GetDeclaredInstanceFields(type);

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var boolAttr = field.GetCustomAttribute<OverrideBooleanAttribute>();

                //If the boolean field is missing the attribute it's considered an override.
                if (boolAttr == null)
                {
                    hasOverrides |= field.FieldType == typeof(bool);
                    continue;
                }

                // If a boolean field has the OverrideBooleanAttribute applied, it's considered an override
                // only if its value differs from the default value specified in the attribute.
                var boolValue = (bool)GetMemberObject(field, instance);
                hasOverrides |= boolAttr.defaultValue != boolValue;
            }

            return hasOverrides;
        }
    }
}