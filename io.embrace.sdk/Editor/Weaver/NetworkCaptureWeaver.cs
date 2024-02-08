using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using EmbraceSDK.Networking;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.Networking;

namespace EmbraceSDK.Editor.Weaver
{
    /// <summary>
    /// This weaver wraps calls to UnityWebRequest and HttpClient so the requests sent by each
    /// will be logged automatically.
    /// </summary>
    public class NetworkCaptureWeaver : IEmbraceWeaver
    {
        // UnityWebRequests are captured by the native SDK on iOS/tvOS, so we only want to weave capture code for
        // that type if we're on a different platform, or if we're in a version of Unity for which we can
        // also capture data processing errors for specialty request types (eg UnityWebRequestTexture)
        #if (!UNITY_IOS && !UNITY_TVOS) || UNITY_2020_1_OR_NEWER
        private const string UNITY_WEB_REQUEST_SEND_WEB_REQUEST_FULL_NAME = "UnityEngine.Networking.UnityWebRequestAsyncOperation UnityEngine.Networking.UnityWebRequest::SendWebRequest()";
        private const string UNITY_WEB_REQUEST_DISPOSE_FULL_NAME = "System.Void UnityEngine.Networking.UnityWebRequest::Dispose()";
        private const string IDISPOSABLE_DISPOSE_FULL_NAME = "System.Void System.IDisposable::Dispose()";
        #endif

        private const string HTTP_CLIENT_CTOR_FULL_NAME = "System.Void System.Net.Http.HttpClient::.ctor()";
        private const string HTTP_CLIENT_CTOR_WITH_HANDLER_FULL_NAME = "System.Void System.Net.Http.HttpClient::.ctor(System.Net.Http.HttpMessageHandler)";
        private const string HTTP_CLIENT_CTOR_WITH_HANDLER_AND_BOOL_FULL_NAME = "System.Void System.Net.Http.HttpClient::.ctor(System.Net.Http.HttpMessageHandler,System.Boolean)";

        private EmbraceWeaverUtilities.FunctionWrapper[] _wrappedFuncs;
        private readonly Regex _coroutineDisplayClassRegex;

        public NetworkCaptureWeaver()
        {
            _coroutineDisplayClassRegex = new Regex(@"^[<](?<method>\w+)[>][d][_]", RegexOptions.Compiled);
            _wrappedFuncs = new[]
            {
                // UnityWebRequests are captured by the native SDK on iOS/tvOS, so we only want to weave capture code for
                // that type if we're on a different platform, or if we're in a version of Unity that for which we can
                // also capture data processing errors for specialty request types (eg UnityWebRequestTexture)
                #if (!UNITY_IOS && !UNITY_TVOS) || (UNITY_2020_1_OR_NEWER && EMBRACE_CAPTURE_DATA_PROCESSING_ERRORS)
                // Replace all calls to UnityWebRequest.SendWebRequest with NetworkCapture.SendWebRequest
                new EmbraceWeaverUtilities.FunctionWrapper()
                {
                    predicate = (instruction, _) =>
                    {
                        return instruction.Operand is MethodReference m && m.FullName.Equals(UNITY_WEB_REQUEST_SEND_WEB_REQUEST_FULL_NAME);
                    },
                    wrapperFunction = typeof(NetworkCapture).GetMethod(nameof(NetworkCapture.SendWebRequest)),
                    callCode = OpCodes.Call,
                },
                // Replace all calls to UnityWebRequest.Dispose with NetworkCapture.DisposeWebRequest
                new EmbraceWeaverUtilities.FunctionWrapper()
                {
                    predicate = (instruction, containingMethod) =>
                    {
                        if (!(instruction.Operand is MethodReference methodReference))
                        {
                            return false;
                        }

                        bool isDispose = methodReference.FullName.Equals(UNITY_WEB_REQUEST_DISPOSE_FULL_NAME);

                        // When UnityWebRequests are disposed by a using statement or through a polymorphic IDisposable reference,
                        // the signature in IL will actually be IDisposable.Dispose, so we try to catch all calls to that version
                        // where the concrete type of the object is known to be UnityWebRequest or cannot be determined.
                        isDispose = isDispose ||
                                    (methodReference.FullName.Equals(IDISPOSABLE_DISPOSE_FULL_NAME) &&
                                     instruction.Previous.PushesObjectOfType<UnityWebRequest>(containingMethod));

                        return isDispose;
                    },
                    wrapperFunction = typeof(NetworkCapture).GetMethod(nameof(NetworkCapture.DisposeWebRequest)),
                    callCode = OpCodes.Call,
                },
                #endif
                // Replace all calls to the parameterless HttpClient constructor with
                // NetworkCapture.GetHttpClientWithLoggingHandler
                new EmbraceWeaverUtilities.FunctionWrapper()
                {
                    predicate = (instruction, _) =>
                    {
                        return instruction.Operand is MethodReference m && m.FullName.Equals(HTTP_CLIENT_CTOR_FULL_NAME);
                    },
                    wrapperFunction = typeof(NetworkCapture)
                        .GetMethod(nameof(NetworkCapture.GetHttpClientWithLoggingHandler), new System.Type[] {}),
                    callCode = OpCodes.Call,
                },
                // Replace all calls to the HttpClient constructor with the handler parameter with
                // NetworkCapture.GetHttpClientWithLoggingHandler
                new EmbraceWeaverUtilities.FunctionWrapper()
                {
                    predicate = (instruction, _) =>
                    {
                        return instruction.Operand is MethodReference m && m.FullName.Equals(HTTP_CLIENT_CTOR_WITH_HANDLER_FULL_NAME);
                    },
                    wrapperFunction = typeof(NetworkCapture)
                        .GetMethod(nameof(NetworkCapture.GetHttpClientWithLoggingHandler),
                            new System.Type[] {typeof(HttpMessageHandler)}),
                    callCode = OpCodes.Call,
                },
                // Replace all calls to the HttpClient constructor with the handler and bool parameters with
                // NetworkCapture.GetHttpClientWithLoggingHandler
                new EmbraceWeaverUtilities.FunctionWrapper()
                {
                    predicate = (instruction, _) =>
                    {
                        return instruction.Operand is MethodReference m && m.FullName.Equals(HTTP_CLIENT_CTOR_WITH_HANDLER_AND_BOOL_FULL_NAME);
                    },
                    wrapperFunction = typeof(NetworkCapture)
                        .GetMethod(nameof(NetworkCapture.GetHttpClientWithLoggingHandler),
                            new System.Type[] {typeof(HttpMessageHandler), typeof(bool)}),
                    callCode = OpCodes.Call,
                },
            };
        }

        public bool WeaveModule(ModuleDefinition assembly)
        {
            bool didWeave = false;

            // Weave each type defined in the module
            foreach (TypeDefinition typeDef in assembly.Types)
            {
                didWeave |= WeaveType(typeDef);
            }

            return didWeave;
        }

        private bool WeaveType(TypeDefinition typeDefinition)
        {
            // Skip if type has exclude attribute
            if (typeDefinition.HasCustomAttributes &&
                typeDefinition.CustomAttributes.ContainsEmbraceWeaverExcludeAttribute())
            {
                EmbracePostCompilationProcessor.LogVerbose(LogType.Log, $"Weaver skipped type {typeDefinition.Name} because it has the {nameof(EmbraceWeaverExcludeAttribute)}");
                return false;
            }

            // Skip type if we are in a display class created for a coroutine which has the exclude attribute
            if (typeDefinition.IsNested &&
                _coroutineDisplayClassRegex.Match(typeDefinition.Name) is Match match &&
                match.Success)
            {
                string sourceMethodName = match.Groups["method"].Value;
                foreach(MethodDefinition sourceMethod in typeDefinition.DeclaringType.Methods.Where(m => m.Name.Equals(sourceMethodName)))
                {
                    if (!typeDefinition.IsDisplayClassForMethod(sourceMethod))
                    {
                        continue;
                    }

                    if (sourceMethod.HasCustomAttributes &&
                        sourceMethod.CustomAttributes.ContainsEmbraceWeaverExcludeAttribute())
                    {
                        EmbracePostCompilationProcessor.LogVerbose(LogType.Log, $"Weaver skipped type {typeDefinition.Name} because it is a display class for a coroutine that has the {nameof(EmbraceWeaverExcludeAttribute)}");
                        return false;
                    }

                    break;
                }
            }

            bool didWeave = false;

            // Weave all methods of the type
            foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
            {
                // Skip if method has exclude attribute
                if (methodDefinition.HasCustomAttributes &&
                    methodDefinition.CustomAttributes.ContainsEmbraceWeaverExcludeAttribute())
                {
                    EmbracePostCompilationProcessor.LogVerbose(LogType.Log, $"Weaver skipped {typeDefinition.Name}.{methodDefinition.Name} because it has the {nameof(EmbraceWeaverExcludeAttribute)}");
                    continue;
                }

                // Skip if method is getter/setter and property has exclude attribute
                if (methodDefinition.IsGetter || methodDefinition.IsSetter)
                {
                    PropertyDefinition property = typeDefinition.Properties
                        .FirstOrDefault(p => p.GetMethod == methodDefinition || p.SetMethod == methodDefinition);

                    if(property != null &&
                       property.HasCustomAttributes &&
                       property.CustomAttributes.ContainsEmbraceWeaverExcludeAttribute())
                    {
                        EmbracePostCompilationProcessor.LogVerbose(LogType.Log, $"Weaver skipped ${typeDefinition.Name}.{methodDefinition.Name} because it has the {nameof(EmbraceWeaverExcludeAttribute)}");
                        continue;
                    }
                }

                didWeave |= methodDefinition.ReplaceMatchingCalls(_wrappedFuncs);
            }

            // Recursively weave all nested types
            foreach (TypeDefinition nestedType in typeDefinition.NestedTypes)
            {
                didWeave |= WeaveType(nestedType);
            }

            return didWeave;
        }
    }
}