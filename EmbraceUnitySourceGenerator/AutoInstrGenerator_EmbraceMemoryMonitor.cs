using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace EmbraceUnitySourceGenerator
{
    [Generator]
    public class AutoInstrGenerator_EmbraceMemoryMonitor : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            var symbols = context.ParseOptions.PreprocessorSymbolNames;
            if (!symbols.Contains("EMBRACE_AUTO_INSTRUMENTATION_MEMORY_MONITOR"))
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
            var stream = assembly.GetManifestResourceStream("EmbraceUnitySourceGenerator.Templates.EmbraceMemoryMonitorMonobehaviour.cs");
            using var reader = new System.IO.StreamReader(stream);
            var source = reader.ReadToEnd();

            using var jsonDoc = JsonDocument.Parse(text.ToString());
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("Embrace_AutoMemory_GCBytesReserved", out var gcBytesReservedProp))
            {
                source = source.Replace("GCBytesReserved = 150000000L, // Default - 150MB", $"GCBytesReserved = {gcBytesReservedProp}L,");
            }

            if (root.TryGetProperty("Embrace_AutoMemory_GCBytesUsed", out var gcBytesUsedProp))
            {
                source = source.Replace("GCBytesUsed = 100000000L, // Default - 100MB", $"GCBytesUsed = {gcBytesUsedProp}L,");
            }

            if (root.TryGetProperty("Embrace_AutoMemory_SystemMemoryUsed", out var systemBytesUsedProp))
            {
                source = source.Replace("SystemBytesUsed = 400000000L, // Default - 400MB", $"SystemBytesUsed = {systemBytesUsedProp}L,");
            }

            if (root.TryGetProperty("Embrace_AutoMemory_TotalBytesReserved", out var totalBytesReservedProp))
            {
                source = source.Replace("TotalBytesReserved = 600000000L, // Default - 600MB", $"TotalBytesReserved = {totalBytesReservedProp}L,");
            }

            if (root.TryGetProperty("Embrace_AutoMemory_TotalBytesUsed", out var totalBytesUsedProp))
            {
                source = source.Replace("TotalBytesUsed = 450000000L, // Default - 450MB", $"TotalBytesUsed = {totalBytesUsedProp}L,");
            }

            if (root.TryGetProperty("Embrace_AutoMemory_GCCollectTimeNanos", out var gcCollectTimeNanosProp))
            {
                source = source.Replace("GCCollectTimeNanos = 5000000L, // Default - 5ms", $"GCCollectTimeNanos = {gcCollectTimeNanosProp}L,");
            }

            if (root.TryGetProperty("Embrace_AutoMemory_BatchIntervalSeconds", out var batchIntervalSecondsProp))
            {
                source = source.Replace("private readonly float _logBatchIntervalSeconds = 10.0f;", $"private readonly float _logBatchIntervalSeconds = {batchIntervalSecondsProp}f;");
            }


            context.AddSource("EmbraceMemoryMonitor.g.cs", source);
        }
    }
}

