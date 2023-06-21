using System;
using System.Linq;
using System.Reflection;

namespace Puerts 
{
    public class DynamicBinderBridge
    {
    }

    public class DynamicBinder
    {
        public static void RegisterDynamicBinder(JsEnv jsEnv) 
        {
            var typeId = jsEnv.TypeRegister.GetTypeId(jsEnv.isolate, typeof(DynamicBinderBridge));
            PuertsDLL.RegisterFunction(jsEnv.isolate, typeId, "DynamicInvoke", false, DynamicInvoke, (long)jsEnv.Idx);
            PuertsDLL.RegisterFunction(jsEnv.isolate, typeId, "DynamicGet", false, DynamicGet, (long)jsEnv.Idx);
            PuertsDLL.RegisterFunction(jsEnv.isolate, typeId, "DynamicSet", false, DynamicSet, (long)jsEnv.Idx);
            PuertsDLL.RegisterFunction(jsEnv.isolate, typeId, "DynamicInvokeStatic", true, DynamicInvoke, (long)jsEnv.Idx);
            PuertsDLL.RegisterFunction(jsEnv.isolate, typeId, "DynamicGetStatic", true, DynamicGet, (long)jsEnv.Idx);
            PuertsDLL.RegisterFunction(jsEnv.isolate, typeId, "DynamicSetStatic", true, DynamicSet, (long)jsEnv.Idx);
            PuertsDLL.RegisterFunction(jsEnv.isolate, typeId, "DynamicGetMemberType", true, DynamicGetMemberType, (long)jsEnv.Idx);
        }

        public static void DynamicGetMemberType(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
                int jsEnvIdx = (int)data;
                JsEnv env = JsEnv.jsEnvs[jsEnvIdx];

                object instance;
                Type type;
                string memberName;
                if (!GetDynamicInfo(isolate, info, env, out instance, out type, out memberName)) return;
                BindingFlags flag = BindingFlags.Public | BindingFlags.DeclaredOnly;
                var isStaticJsValue = PuertsDLL.GetArgumentValue(info, 3);
                var isStatic = PuertsDLL.GetBooleanFromValue(isolate, isStaticJsValue, false);
                flag |= isStatic ? BindingFlags.Static : BindingFlags.Instance;
                var memberInfos = type.GetMember(memberName, flag);
                var translateFunc = env.GeneralSetterManager.GetTranslateFunc(typeof(int));
                if (memberInfos.Length == 0) 
                {
                    translateFunc(env.Idx, isolate, NativeValueApi.SetValueToResult, info, 0);
                }
                else
                {
                    translateFunc(env.Idx, isolate, NativeValueApi.SetValueToResult, info, memberInfos[0].MemberType);
                }
            }
            catch (TargetInvocationException e)
            {
                PuertsDLL.ThrowException(isolate, "c# exception:" + e.InnerException.Message + ",stack:" + e.InnerException.StackTrace);
            }
            catch (Exception e)
            {
                PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }

        protected static bool GetDynamicInfo(IntPtr isolate, IntPtr info, JsEnv env, out object instance, out Type type, out string memberName)
        {
            instance = null;
            type = null;
            memberName = null;

            var instanceJsValue = PuertsDLL.GetArgumentValue(info, 0);
            var typeJsValue = PuertsDLL.GetArgumentValue(info, 1);
            var memberNameJsValue = PuertsDLL.GetArgumentValue(info, 2);
            var instanceJsValueType = PuertsDLL.GetJsValueType(isolate, instanceJsValue, false);
            var typeJsValueType = PuertsDLL.GetJsValueType(isolate, typeJsValue, false);
            var memberNameJsValueType = PuertsDLL.GetJsValueType(isolate, memberNameJsValue, false);

            if (typeJsValueType == JsValueType.String)
            {
                string typeName = StaticTranslate<string>.Get(env.Idx, isolate, NativeValueApi.GetValueFromArgument,
                    typeJsValue, false);
                type = GetType(env, typeName);
                if (type == null)
                {
                    PuertsDLL.ThrowException(isolate, "Type not found: " + typeName);
                    return false;
                }
            }
            else if (typeJsValueType == JsValueType.NativeObject)
                type = StaticTranslate<Type>.Get(env.Idx, isolate, NativeValueApi.GetValueFromArgument, typeJsValue, false);
            else
            {
                PuertsDLL.ThrowException(isolate, "invalid Type");
                return false;
            }

            if (instanceJsValueType == JsValueType.NativeObject)
                instance = StaticTranslate<object>.Get(env.Idx, isolate, NativeValueApi.GetValueFromArgument, instanceJsValue, false); 
            else if (instanceJsValueType == JsValueType.String)
            {
                string instanceName = StaticTranslate<string>.Get(env.Idx, isolate, NativeValueApi.GetValueFromArgument,
                    instanceJsValue, false);
                var memberInfos = type.GetMember(instanceName);
                if (memberInfos.Length == 0) 
                {
                    PuertsDLL.ThrowException(isolate, "instance not found: " + instanceName);
                    return false;
                }

                if (memberInfos[0].MemberType == MemberTypes.Field) 
                {
                    FieldInfo field = (FieldInfo)memberInfos[0];
                    instance = field.GetValue(type);
                }
                else if (memberInfos[0].MemberType == MemberTypes.Property) 
                {
                    MethodInfo getMethodInfo = (memberInfos[0] as PropertyInfo).GetGetMethod();
                    if (getMethodInfo == null)
                    {
                        PuertsDLL.ThrowException(isolate, "instance not found: " + instanceName);
                        return false;
                    }
                    instance = getMethodInfo.Invoke(type, new object[] {});
                }
                else
                {
                    PuertsDLL.ThrowException(isolate, "instance not found: " + instanceName);
                    return false;
                }
            }
            else if (instanceJsValueType == JsValueType.NullOrUndefined) 
                instance = null;
            else {
                PuertsDLL.ThrowException(isolate, "invalid instance");
                return false;
            }

            if (
                memberNameJsValueType != JsValueType.String
            )
            {
                PuertsDLL.ThrowException(isolate, "invalid memberName");
                return false;
            }
            memberName = PuertsDLL.GetStringFromValue(isolate, memberNameJsValue, false);
            return true;
        }
        
        [MonoPInvokeCallback(typeof(V8FunctionCallback))]
        public static void DynamicInvoke(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
                int jsEnvIdx = (int)data;
                JsEnv env = JsEnv.jsEnvs[jsEnvIdx];

                object instance;
                Type type;
                string memberName;
                if (!GetDynamicInfo(isolate, info, env, out instance, out type, out memberName)) return;
                OverloadReflectionWrap[] overloads = Utils
                    .GetMethodAndOverrideMethodByName(type, memberName)
                    .Select(m => new OverloadReflectionWrap(m, env, false))
                    .ToArray();
                
                JSCallInfo callInfo = new JSCallInfo(isolate, info, self, paramLen - 3, 3);
                for (int i = 0; i < overloads.Length; ++i)
                {
                    var overload = overloads[i];
                    if (overload.IsMatch(callInfo))
                    {
                        overload.InvokeWithTarget(callInfo, instance);
                        return;
                    }
                }
                PuertsDLL.ThrowException(isolate, "DynamicInvoke: invalid arguments to " + memberName);
            }
            catch (TargetInvocationException e)
            {
                PuertsDLL.ThrowException(isolate, "c# exception:" + e.InnerException.Message + ",stack:" + e.InnerException.StackTrace);
            }
            catch (Exception e)
            {
                PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }

        [MonoPInvokeCallback(typeof(V8FunctionCallback))]
        public static void DynamicGet(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
                int jsEnvIdx = (int)data;
                JsEnv env = JsEnv.jsEnvs[jsEnvIdx];

                object instance;
                Type type;
                string memberName;
                if (!GetDynamicInfo(isolate, info, env, out instance, out type, out memberName)) return;
                
                var memberInfos = type.GetMember(memberName);
                if (memberInfos.Length == 0) 
                {
                    PuertsDLL.ThrowException(isolate, "DynamicSet: MemberName not found");
                    return;
                }

                if (memberInfos[0].MemberType == MemberTypes.Field) 
                {
                    FieldInfo field = (FieldInfo)memberInfos[0];
                    var translateFunc = env.GeneralSetterManager.GetTranslateFunc(field.FieldType);
                    translateFunc(env.Idx, isolate, NativeValueApi.SetValueToResult, info, field.GetValue(instance));
                }
                else if (memberInfos[0].MemberType == MemberTypes.Property) 
                {
                    MethodInfo xetMethodInfo = (memberInfos[0] as PropertyInfo).GetGetMethod();
                    OverloadReflectionWrap ow = new OverloadReflectionWrap(xetMethodInfo, env);
                    JSCallInfo callInfo = new JSCallInfo(isolate, info, self, paramLen - 3, 3);
                    ow.InvokeWithTarget(callInfo, instance);
                }
            }
            catch (TargetInvocationException e)
            {
                PuertsDLL.ThrowException(isolate, "c# exception:" + e.InnerException.Message + ",stack:" + e.InnerException.StackTrace);
            }
            catch (Exception e)
            {
                PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }
        
        [MonoPInvokeCallback(typeof(V8FunctionCallback))]
        public static void DynamicSet(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
                int jsEnvIdx = (int)data;
                JsEnv env = JsEnv.jsEnvs[jsEnvIdx];

                object instance;
                Type type;
                string memberName;
                if (!GetDynamicInfo(isolate, info, env, out instance, out type, out memberName)) return;
                
                var memberInfos = type.GetMember(memberName);
                if (memberInfos.Length == 0) 
                {
                    PuertsDLL.ThrowException(isolate, "DynamicSet: MemberName not found");
                    return;
                }

                if (memberInfos[0].MemberType == MemberTypes.Field) 
                {                        
                    FieldInfo field = (FieldInfo)memberInfos[0];
                    var translateFunc = JsEnv.jsEnvs[jsEnvIdx].GeneralGetterManager.GetTranslateFunc(field.FieldType);
                    var typeMask = GeneralGetterManager.GetJsTypeMask(field.FieldType);
                    var valuePtr = PuertsDLL.GetArgumentValue(info, 3);
                    var valueType = PuertsDLL.GetJsValueType(isolate, valuePtr, false);
                    object value = null;
                    if (
                        !Utils.IsJsValueTypeMatchType(valueType, field.FieldType, typeMask, () =>
                        {
                            value = translateFunc(jsEnvIdx, isolate, NativeValueApi.GetValueFromArgument, valuePtr,
                                false);
                            return value;
                        }, value)
                    )
                    {
                        PuertsDLL.ThrowException(isolate, "expect " + typeMask + " but got " + valueType);
                    }
                    else
                    {
                        if (value == null)
                            value = translateFunc(env.Idx, isolate, NativeValueApi.GetValueFromArgument, valuePtr, false);
                        field.SetValue(instance, value);
                    }
                }
                else if (memberInfos[0].MemberType == MemberTypes.Property) 
                {
                    MethodInfo xetMethodInfo = (memberInfos[0] as PropertyInfo).GetSetMethod();

                    JSCallInfo callInfo = new JSCallInfo(isolate, info, self, paramLen - 3, 3);
                    var overload = new OverloadReflectionWrap(xetMethodInfo, env);
                    if (overload.IsMatch(callInfo))
                    {
                        overload.InvokeWithTarget(callInfo, instance);
                        return;
                    }
                    PuertsDLL.ThrowException(isolate, "invalid arguments to set_" + memberName);
                }
            }
            catch (TargetInvocationException e)
            {
                PuertsDLL.ThrowException(isolate, "c# exception:" + e.InnerException.Message + ",stack:" + e.InnerException.StackTrace);
            }
            catch (Exception e)
            {
                PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }
        
        protected static Type GetType(JsEnv jsEnv, string className)
        {
            return jsEnv.TypeManager.GetType(className, true);
        }
    }

}
