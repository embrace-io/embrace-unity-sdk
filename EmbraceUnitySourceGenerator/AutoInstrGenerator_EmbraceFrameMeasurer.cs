using System.Text.Json;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace EmbraceUnitySourceGenerator;

[Generator]
public class AutoInstrGenerator_EmbraceFrameMeasurer : ISourceGenerator
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

        var file = context.AdditionalFiles.FirstOrDefault(f => f.Path.Contains("EmbraceConfig.EmbraceUnitySourceGenerator.additionalfile"));

        if (file == null)
        {
            return;
        }

        var text = file.GetText();
        if (text == null)
        {
            return;
        }

        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream("EmbraceUnitySourceGenerator.Templates.EmbraceFrameMeasurer.cs");
        using var reader = new System.IO.StreamReader(stream);
        var source = reader.ReadToEnd();

        using var jsonDoc = JsonDocument.Parse(text.ToString());
        var root = jsonDoc.RootElement;

        if (root.TryGetProperty("Embrace_AutoFPS_TargetFramerate", out var targetFrameRateProp) &&
            root.TryGetProperty("Embrace_AutoFPS_ReportInterval", out var reportIntervalProp))
        {
            source = source.Replace("private float _targetFrameRate = 30f;", $"private float _targetFrameRate = {targetFrameRateProp}f;");
            source = source.Replace("private float _reportInterval = 60f;", $"private float _reportInterval = {reportIntervalProp}f;");
        }

        context.AddSource("EmbraceFrameMeasurer.g.cs", source);
    }
}
