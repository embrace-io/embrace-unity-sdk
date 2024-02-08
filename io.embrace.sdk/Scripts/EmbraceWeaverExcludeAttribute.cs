using System;

namespace EmbraceSDK
{
    /// <summary>
    /// Add this attribute to an assembly, class, method, or property to have the EmbracePostCompilationProcessor
    /// exclude the target from IL weaving.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class EmbraceWeaverExcludeAttribute : Attribute
    {
    }
}