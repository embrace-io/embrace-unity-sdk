using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace EmbraceUnitySourceGenerator
{
    [Generator]
    public class AutoInstrGenerator_EmbraceProfilerRecorderHelper : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            var symbols = context.ParseOptions.PreprocessorSymbolNames;
            if (!symbols.Contains("EMBRACE_AUTO_INSTRUMENTATION_FPS_CAPTURE"))
            {
                return;
            }

            if (context.Compilation.AssemblyName != "Embrace.SDK")
            {
                return;
            }

            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("EmbraceUnitySourceGenerator.Templates.EmbraceProfilerRecorderHelper.cs");
            using var reader = new System.IO.StreamReader(stream);
            var source = reader.ReadToEnd();
            context.AddSource("EmbraceProfilerRecorderHelper.g.cs", source);
        }
    }
}