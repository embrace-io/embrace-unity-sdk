using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace EmbraceSDK.Editor.Weaver
{
    /// <summary>
    /// Utility and extension methods for weaving CIL using Mono.Cecil
    /// </summary>
    public static class EmbraceWeaverUtilities
    {
        /// <summary>
        /// Defines a wrapper function which wraps another function call
        /// </summary>
        public class FunctionWrapper
        {
            /// <summary>
            /// The weaver will call this delegate for each instruction. If the delegate returns true, the instruction
            /// will be replaced with a call to the wrapper function.
            /// </summary>
            public Func<Instruction, MethodDefinition, bool> predicate;
            public MethodInfo wrapperFunction;
            public OpCode callCode;
        }

        /// <summary>
        /// Returns true if the list of attributes contains an instance of EmbraceWeaverExcludeAttribute
        /// </summary>
        public static bool ContainsEmbraceWeaverExcludeAttribute(this IList<CustomAttribute> attributes) =>
            attributes.Any(a => a.AttributeType.Name.Equals(nameof(EmbraceWeaverExcludeAttribute)));

        /// <summary>
        /// Iterates through all instructions in the body of the method and replaces any call instructions that match
        /// the predicates defined by the list of FunctionWrappers
        /// </summary>
        /// <returns>True if any instructions were replaced, false otherwise.</returns>
        public static bool ReplaceMatchingCalls(this MethodDefinition method, IList<FunctionWrapper> wrappers)
        {
            bool didWeave = false;

            if (!method.HasBody) { return false; }

            ILProcessor il = method.Body.GetILProcessor();

            for(int i = 0; i < method.Body.Instructions.Count; ++i)
            {
                Instruction instruction = method.Body.Instructions[i];

                foreach (var wrapper in wrappers)
                {
                    if (instruction.OpCode.Code.IsFunctionCall() &&
                        (wrapper.predicate?.Invoke(instruction, method) ?? false))
                    {
                        MethodReference moduleWrappedMethodReference = method.Module.ImportReference(wrapper.wrapperFunction);
                        Instruction newInstruction = il.Create(wrapper.callCode, moduleWrappedMethodReference);

                        EmbracePostCompilationProcessor.LogVerbose(LogType.Log, $"Weaver replacing \"{(instruction.Operand as MethodReference).FullName}\" with \"{(newInstruction.Operand as MethodReference).FullName}\" in \"{method.FullName}\"");

                        il.Replace(instruction, newInstruction);
                        didWeave = true;


                        break;
                    }
                }
            }

            return didWeave;
        }

        /// <summary>
        /// Checks if the instruction pushes an object of type T onto the stack. If it pushes a base class of type T or
        /// an interface that type T implement, recursively walks back through the method body to try to determine the
        /// concrete type of the object. Returns true if the object is of type T, or if it is assignable from T and the
        /// concrete type of the object cannot be determined within the body of the function. Your wrapper methods
        /// should check the type of the T parameter to handle these cases.
        /// </summary>
        public static bool PushesObjectOfType<T>(this Instruction instruction, MethodDefinition containingMethod)
        {
            if (instruction == null)
            {
                return false;
            }

            string targetTypeName = typeof(T).FullName;

            switch (instruction.OpCode.Code)
            {
                case Code.Call:
                case Code.Calli:
                case Code.Callvirt:
                    return (instruction.Operand as MethodDefinition)?.ReturnType.IsAssignableFrom<T>() ?? false;

                case Code.Ldfld:
                case Code.Ldsfld:
                case Code.Ldloc:
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                case Code.Ldloc_S:
                case Code.Ldarg:
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                case Code.Ldarg_S:
                {
                    if (!TryGetLoadStoreTargetFromInstruction(instruction, containingMethod, out object loadTarget) ||
                        !TryGetTypeReferenceFromLoadTarget(loadTarget, out TypeReference loadType))
                    {
                        return false;
                    }

                    if (loadType.FullName.Equals(targetTypeName))
                    {
                        return true;
                    }

                    if (loadType.IsAssignableFrom<T>())
                    {
                        if (TryFindCorrespondingStoreInstruction(instruction, containingMethod, out Instruction storeInstruction))
                        {
                            return storeInstruction.Previous == null || PushesObjectOfType<T>(storeInstruction.Previous, containingMethod);
                        }

                        // We can't determine the exact type of the object, but we know it is assignable from T
                        return true;
                    }

                    return false;
                }

                default: return false;
            }
        }

        /// <summary>
        /// Returns true if target == type T, target is a base class of T, or target is an interface implemented by T.
        /// </summary>
        public static bool IsAssignableFrom<T>(this TypeReference target)
        {
            string otherTypeName = typeof(T).FullName;
            return target.FullName.Equals(otherTypeName)
                   || target.IsBaseClassOf<T>()
                   || target.IsInterfaceImplementedBy<T>();
        }

        /// <summary>
        /// Returns true if T derives from thisType.
        /// </summary>
        public static bool IsBaseClassOf<T>(this TypeReference thisType)
        {
            for (Type immediateBase = typeof(T).BaseType; immediateBase != null; immediateBase = immediateBase.BaseType)
            {
                if (thisType.FullName.Equals(immediateBase.FullName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if thisType is an interface implemented by T.
        /// </summary>
        public static bool IsInterfaceImplementedBy<T>(this TypeReference thisType)
        {
            Type[] interfaces = typeof(T).GetInterfaces();
            for (int i = 0; i < interfaces.Length; ++i)
            {
                if (thisType.FullName.Equals(interfaces[i].FullName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFunctionCall(this Code code)
        {
            switch (code)
            {
                case Code.Call:
                case Code.Calli:
                case Code.Callvirt:
                case Code.Newobj:
                    return true;

                default: return false;
            }
        }

        /// <summary>
        /// Checks whether the type is a display class for the given method. For async methods, this checks for a local
        /// variable that matches the type. For coroutines, it checks if the parameters of the method match the public
        /// fields of the type.
        /// </summary>
        public static bool IsDisplayClassForMethod(this TypeDefinition type, MethodDefinition method)
        {
            if (!type.Name.Contains(method.Name))
            {
                return false;
            }

            foreach (VariableDefinition local in method.Body.Variables)
            {
                if (local.VariableType.FullName == type.FullName)
                {
                    return true;
                }
            }

            if (!method.HasParameters)
            {
                if (method.HasThis)
                {
                    return type.Fields.Count(f => f.IsPublic) == 1;
                }

                return !type.Fields.Any(f => f.IsPublic);
            }

            foreach (ParameterDefinition parameter in method.Parameters)
            {
                if (!type.Fields.Any(f => f.IsPublic && f.Name.Equals(parameter.Name)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Searches back through the method body for the most recent store instruction which stores a value into
        /// the same target as the provided load instruction.
        /// </summary>
        private static bool TryFindCorrespondingStoreInstruction(Instruction loadInstruction, MethodDefinition containingMethod, out Instruction storeInstruction)
        {
            if (!TryGetLoadStoreTargetFromInstruction(loadInstruction, containingMethod, out object loadTarget))
            {
                storeInstruction = null;
                return false;
            }

            for (Instruction i = loadInstruction.Previous; i != null; i = i.Previous)
            {
                if (TryGetLoadStoreTargetFromInstruction(i, containingMethod, out object storeTarget) &&
                    storeTarget == loadTarget)
                {
                    storeInstruction = i;
                    return true;
                }
            }

            storeInstruction = null;
            return false;
        }

        /// <summary>
        /// Gets the type of the object being loaded by a load instruction operand.
        /// </summary>
        private static bool TryGetTypeReferenceFromLoadTarget(object loadTarget, out TypeReference typeReference)
        {
            switch (loadTarget)
            {
                case ParameterReference parameter:
                    typeReference = parameter.ParameterType;
                    return true;

                case FieldReference field:
                    typeReference = field.FieldType;
                    return true;

                case VariableReference variable:
                    typeReference = variable.VariableType;
                    return true;

                default:
                    typeReference = null;
                    return false;
            }
        }

        /// <summary>
        /// Gets the target that the load instruction will load from (ie field, variable, parameter, etc).
        /// </summary>
        private static bool TryGetLoadStoreTargetFromInstruction(Instruction instruction, MethodDefinition methodDefinition, out object target)
        {
            object operand = null;
            switch (instruction.OpCode.Code)
            {
                case Code.Ldarg_0:
                    operand = methodDefinition.HasThis
                        ? methodDefinition.Body.ThisParameter
                        : methodDefinition.Parameters[0];
                    break;

                case Code.Ldarg_1:
                    operand = methodDefinition.HasThis
                        ? methodDefinition.Parameters[0]
                        : methodDefinition.Parameters[1];
                    break;

                case Code.Ldarg_2:
                    operand = methodDefinition.HasThis
                        ? methodDefinition.Parameters[1]
                        : methodDefinition.Parameters[2];
                    break;

                case Code.Ldarg_3:
                    operand = methodDefinition.HasThis
                        ? methodDefinition.Parameters[2]
                        : methodDefinition.Parameters[3];
                    break;


                case Code.Ldloc_0: operand = methodDefinition.Body.Variables[0]; break;
                case Code.Ldloc_1: operand = methodDefinition.Body.Variables[1]; break;
                case Code.Ldloc_2: operand = methodDefinition.Body.Variables[2]; break;
                case Code.Ldloc_3: operand = methodDefinition.Body.Variables[3]; break;

                case Code.Stloc_0: operand = methodDefinition.Body.Variables[0]; break;
                case Code.Stloc_1: operand = methodDefinition.Body.Variables[1]; break;
                case Code.Stloc_2: operand = methodDefinition.Body.Variables[2]; break;
                case Code.Stloc_3: operand = methodDefinition.Body.Variables[3]; break;

                case Code.Ldfld:
                case Code.Ldloc_S:
                case Code.Ldloc:
                case Code.Ldarg:
                case Code.Ldarg_S:
                case Code.Stfld:
                case Code.Stloc:
                case Code.Stloc_S:
                case Code.Starg:
                case Code.Starg_S:
                    operand = instruction.Operand;
                    break;
            }

            target = operand;
            return target != null;
        }
    }
}