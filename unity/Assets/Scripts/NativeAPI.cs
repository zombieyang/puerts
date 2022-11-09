﻿using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace PuertsIl2cpp
{
    public sealed class PersistentObjectInfo
    {
        long placeHold0;
        long placeHold1;
        long placeHold2;
        long placeHold3;
        long placeHold4;
        long placeHold5;
        long placeHold6;
        long placeHold7;

        [MethodImpl(MethodImplOptions.InternalCall)]
        void releaseScriptObject()
        {
            throw new NotImplementedException();
        }

        ~PersistentObjectInfo()
        {
            //UnityEngine.Debug.Log("~DelegateInfo start..");
            releaseScriptObject();
            //UnityEngine.Debug.Log("~DelegateInfo end..");
        }
    }

#pragma warning disable 414
    public class MonoPInvokeCallbackAttribute : System.Attribute
    {
        private Type type;
        public MonoPInvokeCallbackAttribute(Type t)
        {
            type = t;
        }
    }
#pragma warning restore 414

    public class NativeAPI
    {
#if (UNITY_IPHONE || UNITY_TVOS || UNITY_WEBGL || UNITY_SWITCH) && !UNITY_EDITOR
        const string DLLNAME = "__Internal";
#else
        const string DLLNAME = "puerts_il2cpp";
#endif

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitialPuerts(IntPtr PesapiImpl);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateNativeJSEnv();

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyNativeJSEnv(IntPtr jsEnv);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetPesapiImpl();

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetPesapiEnvHolder(IntPtr jsEnv);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateCSharpTypeInfo(string name, IntPtr type_id, IntPtr super_type_id, IntPtr klass, bool isValueType, bool isDelegate, string delegateSignature);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReleaseCSharpTypeInfo(IntPtr classInfo);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AddConstructor(IntPtr classInfo, string signature, IntPtr method, IntPtr methodPointer, int typeInfoNum);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AddMethod(IntPtr classInfo, string signature, string name, bool is_static, bool isGetter, bool isSetter, IntPtr method, IntPtr methodPointer, int typeInfoNum);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool AddField(IntPtr classInfo, string signature, string name, bool is_static, IntPtr fieldInfo, int offset, IntPtr fieldTypeInfo);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetTypeInfo(IntPtr wrapData, int index, IntPtr type_id);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ExchangeAPI(IntPtr exports);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RegisterCSharpType(IntPtr classInfo);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetObjectPool(IntPtr jsEnv, IntPtr objectPoolAddMethodInfo, IntPtr objectPoolAdd, IntPtr objectPoolRemoveMethodInfo, IntPtr objectPoolRemove, IntPtr objectPoolInstance);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetTryLoadCallback(IntPtr tryLoadMethodInfo, IntPtr tryLoad);

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetObjectToGlobal(IntPtr jsEnv, string key, IntPtr objPtr);

        public static string GetValueTypeFieldsSignature(Type type)
        {
            if (!type.IsValueType)
            {
                throw new Exception(type + " is not a valuetype");
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (type.BaseType.IsValueType)
            {
                sb.Append(GetValueTypeFieldsSignature(type.BaseType));
            }
            foreach(var field in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                sb.Append((field.FieldType.IsValueType && !field.FieldType.IsPrimitive) ? GetValueTypeFieldsSignature(field.FieldType) : GetTypeSignature(field.FieldType));
            }
            return sb.ToString();
        }

        public static string GetTypeSignature(Type type)
        {
            if (type == typeof(void))
            {
                return "v";
            }
            else if (type == typeof(bool))
            {
                return "b";
            }
            else if (type == typeof(byte))
            {
                return "u1";
            }
            else if (type == typeof(sbyte))
            {
                return "i1";
            }
            else if (type == typeof(short))
            {
                return "i2";
            }
            else if (type == typeof(ushort))
            {
                return "u2";
            }
            else if (type == typeof(int))
            {
                return "i4";
            }
            else if (type == typeof(uint))
            {
                return "u4";
            }
            else if (type == typeof(long))
            {
                return "i8";
            }
            else if (type == typeof(ulong))
            {
                return "u8";
            }
            else if (type == typeof(char))
            {
                return "c";
            }
            else if (type == typeof(double))
            {
                return "r8";
            }
            else if (type == typeof(float))
            {
                return "r4";
            }
            else if (type == typeof(IntPtr) || type == typeof(UIntPtr))
            {
                return "p";
            }
            else if (type == typeof(DateTime)) //是否要支持？
            {
                return "d";
            }
            else if (type == typeof(string))
            {
                return "s";
            }
            else if (type.IsByRef || type.IsPointer)
            {
                return "P" + GetTypeSignature(type.GetElementType());
            }
            //TODO: ArrayBuffer...
            else if (!type.IsValueType)
            {
                return "o";
            }
            else if (type.IsValueType && !type.IsPrimitive)
            {
                //return "s" + Marshal.SizeOf(type);
                return "s_" + GetValueTypeFieldsSignature(type) + "_" ;
            }
            throw new NotSupportedException("no support type: " + type);
        }

        public static string GetParamerterSignature(ParameterInfo parameterInfo)
        {
            return GetTypeSignature(parameterInfo.ParameterType);
        }

        public static string GetMethodSignature(MethodBase methodBase, bool isDelegateInvoke = false)
        {
            string signature = "";
            if (methodBase is ConstructorInfo)
            {
                signature += "vt";
                var constructorInfo = methodBase as ConstructorInfo;
                foreach(var p in constructorInfo.GetParameters())
                {
                    signature += GetParamerterSignature(p);
                }

            }
            else if (methodBase is MethodInfo)
            {
                var methodInfo = methodBase as MethodInfo;
                signature += GetTypeSignature(methodInfo.ReturnType);
                if (!methodInfo.IsStatic && !isDelegateInvoke) signature += "t";
                foreach (var p in methodInfo.GetParameters())
                {
                    signature += GetParamerterSignature(p);
                }

            }
            return signature;
        }

        static bool TypeInfoPassToJsFilter(Type type)
        {
            return type != typeof(void) && !type.IsPrimitive;
        }

        static List<Type> getUsedTypes(MethodBase methodBase)
        {
            List<Type> types = new List<Type>();
            if (methodBase is MethodInfo)
            {
                var returnType = (methodBase as MethodInfo).ReturnType;
                if (TypeInfoPassToJsFilter(returnType))
                {
                    types.Add(returnType);
                }
            }
            types.AddRange(methodBase.GetParameters().Select(m => m.ParameterType.IsByRef ? m.ParameterType.GetElementType() : m.ParameterType).Where(TypeInfoPassToJsFilter));
            return types;
        }

        //call by native, do no throw!!
        public static void RegisterNoThrow(IntPtr typeId, bool includeNonPublic)
        {
            try
            {
                Type type = TypeIdToType(typeId);
                if (type == null) return;
                UnityEngine.Debug.Log(string.Format("try load type {0}", type));
                Register(type, includeNonPublic);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(string.Format("try load type throw {0}", e));
            }
        }

        public static void Register(Type type, bool includeNonPublic, bool throwIfMemberFail = false)
        {
            BindingFlags flag = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
            if (includeNonPublic)
            {
                flag = flag | BindingFlags.NonPublic;
            }

            Register(type, type.GetConstructors(flag), type.GetMethods(flag), type.GetProperties(flag), type.GetFields(flag), throwIfMemberFail);
        }

        public static void Register(Type type, MethodBase[] ctors = null, MethodBase[] methods = null, PropertyInfo[] properties = null, FieldInfo[] fields = null, bool throwIfMemberFail = false)
        {
            IntPtr typeInfo = IntPtr.Zero;
            try
            {
                bool isDelegate = typeof(MulticastDelegate).IsAssignableFrom(type);
                var typeId = GetTypeId(type);
                //UnityEngine.Debug.Log(string.Format("{0} typeId is {1}", type, typeId));
                typeInfo = CreateCSharpTypeInfo(type.ToString(), typeId, IntPtr.Zero, typeId, type.IsValueType, isDelegate, isDelegate ? GetMethodSignature(type.GetMethod("Invoke"), true) : "");
                if (typeInfo == IntPtr.Zero)
                {
                    throw new Exception(string.Format("create TypeInfo for {0} fail", type));
                }
                if (!isDelegate)
                {
                    if (ctors != null && ctors.Length > 0)
                    {
                        foreach (var ctor in ctors)
                        {
                            List<Type> usedTypes = getUsedTypes(ctor);
                            //UnityEngine.Debug.Log(string.Format("add ctor {0}, usedTypes count: {1}", ctor, usedTypes.Count));
                            var wrapData = AddConstructor(typeInfo, GetMethodSignature(ctor), GetMethodInfoPointer(ctor), GetMethodPointer(ctor), usedTypes.Count);
                            if (wrapData == IntPtr.Zero)
                            {
                                if (!throwIfMemberFail)
                                {
#if WARNING_IF_MEMBERFAIL
                                    UnityEngine.Debug.LogWarning(string.Format("add constructor for {0} fail, signature:{1}", type, GetMethodSignature(ctor)));
#endif
                                    continue;
                                }
                                throw new Exception(string.Format("add constructor for {0} fail, signature:{1}", type, GetMethodSignature(ctor)));
                            }
                            for (int i = 0; i < usedTypes.Count; ++i)
                            {
                                var usedTypeId = GetTypeId(usedTypes[i]);
                                //UnityEngine.Debug.Log(string.Format("set used type for ctor {0}: {1}={2}, typeId:{3}", ctor, i, usedTypes[i], usedTypeId));
                                SetTypeInfo(wrapData, i, usedTypeId);
                            }
                        }
                    }

                    Action<string, MethodInfo, bool, bool> AddMethodToType = (string name, MethodInfo method, bool isGeter, bool isSetter) =>
                    {
                        List<Type> usedTypes = getUsedTypes(method);
                        //UnityEngine.Debug.Log(string.Format("add method {0}, usedTypes count: {1}", method, usedTypes.Count));
                        var wrapData = AddMethod(typeInfo, GetMethodSignature(method), name, method.IsStatic, isGeter, isSetter, GetMethodInfoPointer(method), GetMethodPointer(method), usedTypes.Count);
                        if (wrapData == IntPtr.Zero)
                        {
                            if (throwIfMemberFail)
                            {
                                throw new Exception(string.Format("add method for {0}:{1} fail, signature:{2}", type, method, GetMethodSignature(method)));
                            }
                            else
                            {
#if WARNING_IF_MEMBERFAIL
                                UnityEngine.Debug.LogWarning(string.Format("add method for {0}:{1} fail, signature:{2}", type, method, GetMethodSignature(method)));
#endif
                                return;
                            }
                        }
                        for (int i = 0; i < usedTypes.Count; ++i)
                        {
                            var usedTypeId = GetTypeId(usedTypes[i]);
                            //UnityEngine.Debug.Log(string.Format("set used type for method {0}: {1}={2}, typeId:{3}", method, i, usedTypes[i], usedTypeId));
                            SetTypeInfo(wrapData, i, usedTypeId);
                        }
                    };

                    if (methods != null)
                    {
                        foreach (var method in methods)
                        {
                            AddMethodToType(method.Name, method as MethodInfo, false, false);
                        }
                    }

                    if (properties != null)
                    {
                        foreach (var prop in properties)
                        {
                            var getter = prop.GetGetMethod();
                            if (getter != null)
                            {
                                AddMethodToType(prop.Name, getter, true, false);
                            }
                            var setter = prop.GetSetMethod();
                            if (setter != null)
                            {
                                AddMethodToType(prop.Name, setter, false, true);
                            }
                        }
                    }

                    if (fields != null)
                    {
                        foreach (var field in fields)
                        {
                            string sig = (field.IsStatic ? "" : "t") + GetTypeSignature(field.FieldType);
                            if (!AddField(typeInfo, sig, field.Name, field.IsStatic, GetFieldInfoPointer(field), GetFieldOffset(field, type.IsValueType), GetTypeId(field.FieldType)))
                            {
                                if (!throwIfMemberFail)
                                {
#if WARNING_IF_MEMBERFAIL
                                    UnityEngine.Debug.LogWarning(string.Format("add field for {0}:{1} fail, signature:{2}", type, field, sig));
#endif
                                    continue;
                                }
                                throw new Exception(string.Format("add field for {0}:{1} fail, signature:{2}", type, field, sig));
                            }
                            //UnityEngine.Debug.Log(string.Format("AddField {0} of {1} ok offset={2}", field, type, GetFieldOffset(field, type.IsValueType)));
                        }
                    }
                }

                if (!RegisterCSharpType(typeInfo))
                {
                    throw new Exception(string.Format("Register for {0} fail", type));
                }
            }
            catch(Exception e)
            {
                ReleaseCSharpTypeInfo(typeInfo);
                throw e;
            }
        }


        [MethodImpl(MethodImplOptions.InternalCall)]
        public static IntPtr GetMethodPointer(MethodBase methodInfo)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static IntPtr GetMethodInfoPointer(MethodBase methodInfo)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static int GetFieldOffset(FieldInfo fieldInfo, bool isInValueType)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static IntPtr GetFieldInfoPointer(FieldInfo fieldInfo)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static IntPtr GetObjectPointer(Object obj)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static IntPtr GetTypeId(Type type)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static Type TypeIdToType(IntPtr typeId)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static IntPtr GetUnityExports()
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static void SetPersistentObjectInfoType(Type type)
        {
            throw new NotImplementedException();
        }

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static void SetObjectPool(ObjectPool objectPool, MethodBase insert)
        //{
        //    throw new NotImplementedException();
        //}

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static void PesapiCallTest(Type type)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static object EvalInternal(IntPtr envHolder, byte[] code, string path, Type type)
        {
            throw new NotImplementedException();
        }
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || PUERTS_GENERAL || (UNITY_WSA && !UNITY_EDITOR)
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
        public delegate void LogCallback(string content);

        [MonoPInvokeCallback(typeof(LogCallback))]
        public static void LogImpl(string msg)
        {
            UnityEngine.Debug.Log("debug msg: " + msg);
        }

        public static LogCallback Log = LogImpl;

        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLogCallback(IntPtr log);

        //[UnityEngine.Scripting.RequiredByNativeCodeAttribute()]
        public static void SetLogCallback(LogCallback log)
        {
#if PUERTS_GENERAL || (UNITY_WSA && !UNITY_EDITOR) || UNITY_STANDALONE_WIN
            GCHandle.Alloc(log);
#endif
            IntPtr fn1 = log == null ? IntPtr.Zero : Marshal.GetFunctionPointerForDelegate(log);

            SetLogCallback(fn1);
        }
    }
}
