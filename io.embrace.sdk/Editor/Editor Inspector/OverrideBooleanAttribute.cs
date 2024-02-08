using System;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Specifies a default value for boolean fields in an Embrace SDK configuration. The default value will be
    /// compared against the fields value at build time to determine if the property should be included. If this
    /// Attribute is omitted, the boolean field will be unconditionally included in the output configuration file.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OverrideBooleanAttribute : Attribute
    {
        public readonly bool defaultValue;

        /// <summary>
        /// Specify a default value for an Embrace SDK configuration boolean field.
        /// </summary>
        /// <param name="defaultValue">The expected default value of the boolean field</param>
        public OverrideBooleanAttribute(bool defaultValue) => this.defaultValue = defaultValue;
    }
}