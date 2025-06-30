using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using EmbraceSDK.EditorView;
using UnityEditor;
using UnityEditor.Compilation;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace EmbraceSDK.Editor.Weaver
{
    /// <summary>
    /// Hooks into script compilation callbacks provided by Unity and invokes IEmbraceWeaver instances
    /// on the compiled assemblies.
    /// </summary>
    [InitializeOnLoad]
    public static class EmbracePostCompilationProcessor
    {
        // Weavers will run in the order declared here
        private static readonly IEmbraceWeaver[] _weavers = new IEmbraceWeaver[]
        {
            new NetworkCaptureWeaver(),

            // Add new weavers here
        };
        private static readonly List<string> _dirtyAssemblies = new List<string>();
        private static readonly StringBuilder _logBuilder = new StringBuilder();
        private static bool _didForceRecompile;

        public const string EMBRACE_WEAVER_ENABLED = nameof(EMBRACE_WEAVER_ENABLED);
        public const string EMBRACE_WEAVER_BUILDS_ONLY = nameof(EMBRACE_WEAVER_BUILDS_ONLY);
        public const string EMBRACE_WEAVER_INCLUDE_EDITOR_ASSEMBLY = nameof(EMBRACE_WEAVER_INCLUDE_EDITOR_ASSEMBLY);
        public const string EMBRACE_WEAVER_VERBOSE_LOGGING = nameof(EMBRACE_WEAVER_VERBOSE_LOGGING);

        static EmbracePostCompilationProcessor()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        private static void OnCompilationStarted(object obj)
        {
            _dirtyAssemblies.Clear();
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] compilerMessages)
        {
            _dirtyAssemblies.Add(assemblyPath);

        }

        private static void OnCompilationFinished(object obj)
        {
            #if UNITY_ANDROID || UNITY_IOS || UNITY_TVOS
            if (_weavers.Length == 0)
            {
                return;
            }

            bool weaverWasRecompiled = _dirtyAssemblies.Any(assemblyPath =>
                Path.GetFileNameWithoutExtension(assemblyPath)
                    .Equals(typeof(EmbracePostCompilationProcessor).Assembly.GetName().Name));

            Assembly[] assemblies = CompilationPipeline.GetAssemblies();

            // Exit early if no assemblies have the EMBRACE_WEAVER_ENABLED define.
            //
            // This patch was added to avoid invalidating the entire Android build cache every time the weaver assembly
            // is recompiled in projects not using any features which require the weaver. A better approach that would
            // also benefit projects that do utilize the weaver would be to force Unity to only recompile targeted 
            // assemblies, but Unity does not currently provide an API for doing so.
            //
            // WARNING: This patch is not covered by automated tests. Discuss manual testing strategies with the team
            // before editing or removing this patch.
            if (!assemblies.Any(assembly => assembly.defines.Contains(EMBRACE_WEAVER_ENABLED)))
            {
                return;
            }

            // If the weaver assembly was recompiled due to a code change, we need to clean the build cache and
            // recompile all assemblies to get them back to their un-weaved state before running the updated weaver
            // on them.
            if (!_didForceRecompile && weaverWasRecompiled)
            {
                LogVerbose(LogType.Log, "Forcing a full project recompile due to weaver assembly being recompiled.");
                _didForceRecompile = true;
                AssetDatabaseUtil.ForceRecompileScripts();
                return;
            }

            // If the project is being opened for the first time with no Library folder the Assembly array will be empty.
            // In that case, we need to trigger another compile so we can get the proper list of assemblies to filter.
            if (assemblies == null || assemblies.Length == 0)
            {
                LogVerbose(LogType.Log, "Forcing a full project recompile due to empty assembly cache.");
                _didForceRecompile = true;
                AssetDatabaseUtil.ForceRecompileScripts();
                return;
            }

            _didForceRecompile = false;

            // If we get to this point without any dirty assemblies it means that the weaver assembly was not loaded
            // before the compile started. This should only happen when the project is reimported, so we should be clear
            // to weave all assemblies in the project without recompiling again.
            if (_dirtyAssemblies.Count == 0)
            {
                LogVerbose(LogType.Log, "Dirty assembly list empty, weaving all script assemblies.");
                _dirtyAssemblies.AddRange(assemblies.Select(a => a.outputPath));
            }

            _logBuilder.Clear();
            Settings settings = Settings.LoadSettings();

            DefaultAssemblyResolver assemblyResolver = new DefaultAssemblyResolver();
            HashSet<string> searchPaths = new HashSet<string>();
            foreach (Assembly assembly in assemblies)
            {
                // Get file paths for source and compiled assemblies referenced by this assembly and add their
                // containing directory to the assembly resolver search directories list.
                IEnumerable<string> referencedAssemblyPaths = assembly.assemblyReferences.Select(a => a.outputPath)
                    .Concat(assembly.compiledAssemblyReferences);
                foreach (string filePath in referencedAssemblyPaths)
                {
                    DirectoryInfo directory = Directory.GetParent(filePath);
                    if (directory == null)
                    {
                        EmbraceLogger.LogError($"Failed to parse directory name for path: {filePath}");
                        continue;
                    }

                    if (searchPaths.Contains(directory.FullName)) continue;

                    assemblyResolver.AddSearchDirectory(directory.FullName);
                    searchPaths.Add(directory.FullName);
                }
            }

            ReaderParameters readerParams = new ReaderParameters()
            {
                ReadWrite = true,
                InMemory = true,
                AssemblyResolver = assemblyResolver,
                ReadSymbols = true,
            };

            WriterParameters writerParams = new WriterParameters()
            {
                WriteSymbols = true,
            };

            foreach (string assemblyPath in _dirtyAssemblies)
            {
                if (!File.Exists(assemblyPath))
                {
                    LogVerbose(LogType.Warning, $"Weaver skipped an assembly path that did not exist: {assemblyPath}");
                    continue;
                }

                Assembly assembly = assemblies.FirstOrDefault(a => a.outputPath == assemblyPath);
                if (assembly == null)
                {
                    LogVerbose(LogType.Warning, $"Failed to find Assembly with output path: {assemblyPath}");
                    continue;
                }

                if (!assembly.defines.Contains(EMBRACE_WEAVER_ENABLED))
                {
                    LogVerbose(LogType.Log,
                        $"Weaver skipped assembly {assembly.name} because {EMBRACE_WEAVER_ENABLED} was not defined.");
                    continue;
                }

                if (settings.excludedAssemblyNames.Contains(assembly.name))
                {
                    LogVerbose(LogType.Log,
                        $"Weaver skipped assembly {assembly.name} because its name was found in the exclude list.");
                    continue;
                }

                if (!BuildPipeline.isBuildingPlayer && assembly.defines.Contains(EMBRACE_WEAVER_BUILDS_ONLY))
                {
                    LogVerbose(LogType.Log,
                        $"Weaver skipped assembly {assembly.name} because {EMBRACE_WEAVER_BUILDS_ONLY} was defined and we are not compiling for a build.");
                    continue;
                }

                if ((assembly.flags & AssemblyFlags.EditorAssembly) != 0 &&
                    !assembly.defines.Contains(EMBRACE_WEAVER_INCLUDE_EDITOR_ASSEMBLY))
                {
                    LogVerbose(LogType.Log,
                        $"Weaver skipped assembly {assembly.name} because it is an editor assembly and does not define {EMBRACE_WEAVER_INCLUDE_EDITOR_ASSEMBLY}");
                    continue;
                }

                using (ModuleDefinition module = ModuleDefinition.ReadModule(assemblyPath, readerParams))
                {
                    if (module == null)
                    {
                        EmbraceLogger.LogError($"Failed to read module for weaving: {assemblyPath}");
                        continue;
                    }

                    if (module.Assembly.HasCustomAttributes &&
                        module.Assembly.CustomAttributes.ContainsEmbraceWeaverExcludeAttribute())
                    {
                        LogVerbose(LogType.Log,
                            $"Weaver skipped assembly {assembly.name} because it has the {nameof(EmbraceWeaverExcludeAttribute)}");
                        continue;
                    }

                    bool didModifyModule = false;
                    foreach (IEmbraceWeaver weaver in _weavers)
                    {
                        // Unity 2019-2022 log exceptions thrown from this callback at an info level for some reason.
                        // We'll catch them here and manually log them at exception level so that they stand out in
                        // the console.
                        try
                        {
                            didModifyModule |= weaver.WeaveModule(module);
                        }
                        catch (System.Exception e)
                        {
                            EmbraceLogger.LogException(e);
                            throw;
                        }
                    }

                    if (didModifyModule)
                    {
                        LogVerbose(LogType.Log, $"Weaver writing changes to {assemblyPath}");
                        module.Write(assemblyPath, writerParams);

                        _logBuilder.Append("    ");
                        _logBuilder.AppendLine(assemblyPath);
                    }
                    else
                    {
                        LogVerbose(LogType.Log,
                            $"Weaver processed assembly {assembly.name} but did not make any modifications.");
                    }
                }
            }
            
            if (_logBuilder.Length > 0)
            {
                _logBuilder.Insert(0, "Embrace Weaver modified the following assemblies: \n");
                EmbraceLogger.Log(_logBuilder.ToString());
                _logBuilder.Clear();
            }
            #endif
        }

        [Conditional(EMBRACE_WEAVER_VERBOSE_LOGGING)]
        internal static void LogVerbose(LogType logType, string message)
        {
            EmbraceLogger.Log(logType, message);
        }

        public class Settings
        {
            private const string FILE_NAME = "WeaverSettings.json";

            public readonly List<string> excludedAssemblyNames;

            private Settings()
            {
                excludedAssemblyNames = new List<string>();
            }

            public void Reset()
            {
                excludedAssemblyNames.Clear();

                foreach (Assembly assembly in CompilationPipeline.GetAssemblies())
                {
                    if (assembly.name.StartsWith("Unity."))
                    {
                        excludedAssemblyNames.Add(assembly.name);
                    }
                }
            }

            public static Settings LoadSettings()
            {
                string fullPath = Path.Combine(AssetDatabaseUtil.EmbraceDataDirectory, FILE_NAME);

                if (File.Exists(fullPath))
                {
                    string jsonSettings = File.ReadAllText(fullPath);
                    try
                    {
                        return JObject.Parse(jsonSettings).ToObject<Settings>();
                    }
                    catch
                    {
                        EmbraceLogger.LogWarning($"Failed to parse {fullPath}.");
                    }
                }

                Settings settings = new Settings();
                settings.Reset();
                return settings;
            }

            public static void SaveSettings(Settings settings)
            {
                UnityEngine.Debug.Log(AssetDatabaseUtil.EmbraceDataDirectory);
                string path = Path.Combine(AssetDatabaseUtil.EmbraceDataDirectory, FILE_NAME);
                string jsonSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);

                File.WriteAllText(path, jsonSettings);
            }
        }

    }
}
