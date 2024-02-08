using Mono.Cecil;

namespace EmbraceSDK.Editor.Weaver
{
    /// <summary>
    /// To create a new weaver, implement this interface in your type and add it to the array of weavers in EmbracePostCompilationProcessor
    /// </summary>
    public interface IEmbraceWeaver
    {
        bool WeaveModule(ModuleDefinition assembly);
    }
}