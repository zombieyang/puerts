/*
* Tencent is pleased to support the open source community by making Puerts available.
* Copyright (C) 2020 THL A29 Limited, a Tencent company.  All rights reserved.
* Puerts is licensed under the BSD 3-Clause License, except for the third-party components listed in the file 'LICENSE' which may be subject to their corresponding license terms.
* This file is subject to the terms and conditions defined in file 'LICENSE', which is part of this source code package.
*/

class ArgumentCodeGenerator {
    constructor(i) {
        this.index = i;
    }

    declareArgObj() {
        return `object argobj${this.index} = null`;

    }

    argObj() {
        return `argobj${this.index}`
    }

    declareArgJSValueType() {
        return `JsValueType argType${this.index} = JsValueType.Invalid`;
    }

    argJSValueType() {
        return `argType${this.index}`
    }

    declareAndGetV8Value() {
        return `IntPtr v8Value${this.index} = PuertsDLL.GetArgumentValue(info, ${this.index})`
    }

    v8Value() {
        return `v8Value${this.index}`
    }

    arg() {
        return `arg${this.index}`
    }

    getArg(typeInfo) {
        let typeName = typeInfo.TypeName;
        let isByRef = typeInfo.IsByRef ? "true" : "false";
        
        if (typeInfo.IsParams) {
            return `${typeName}[] arg${this.index} = ArgHelper.GetParams<${typeName}>((int)data, isolate, info, ${this.index}, paramLen, ${this.v8Value()})`;
        } else if (typeInfo.IsEnum) {
            return `${typeName} arg${this.index} = (${typeName})StaticTranslate<${typeInfo.UnderlyingTypeName}>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${this.v8Value()}, ${isByRef})`;
        } else if (typeName in fixGet) {
            return `${typeName} arg${this.index} = StaticTranslate<${typeName}>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${this.v8Value()}, ${isByRef})`;
        } else {
            return `argobj${this.index} = argobj${this.index} != null ? argobj${this.index} : StaticTranslate<${typeInfo.TypeName}>.Get((int)data, isolate, NativeValueApi.GetValueFromArgument, v8Value${this.index}, ${typeInfo.IsByRef ? "true" : "false"}); ${typeName} arg${this.index} = (${typeName})argobj${this.index}`
        }
    }

    invokeIsMatch(paramInfo) {
        if (paramInfo.IsParams) {
            return `ArgHelper.IsMatchParams((int)data, isolate, info, ${paramInfo.ExpectJsType}, ${paramInfo.ExpectCsType}, ${this.index}, paramLen, ${this.v8Value()}, ref ${this.argObj()}, ref ${this.argJSValueType()})`

        } else {
            return `ArgHelper.IsMatch((int)data, isolate, ${paramInfo.ExpectJsType}, ${paramInfo.ExpectCsType}, ${paramInfo.IsByRef}, ${paramInfo.IsOut}, ${this.v8Value()}, ref ${this.argObj()}, ref ${this.argJSValueType()})`
        }
    }
}

const fixGet = {
    char: (argHelperName, isByRef) => `StaticTranslate<char>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    sbyte: (argHelperName, isByRef) => `StaticTranslate<sbyte>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    byte: (argHelperName, isByRef) => `StaticTranslate<byte>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    short: (argHelperName, isByRef) => `StaticTranslate<short>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    ushort: (argHelperName, isByRef) => `StaticTranslate<ushort>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    int: (argHelperName, isByRef) => `StaticTranslate<int>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    uint: (argHelperName, isByRef) => `StaticTranslate<uint>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    long: (argHelperName, isByRef) => `StaticTranslate<long>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    ulong: (argHelperName, isByRef) => `StaticTranslate<ulong>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    double: (argHelperName, isByRef) => `StaticTranslate<double>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    float: (argHelperName, isByRef) => `StaticTranslate<float>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    bool: (argHelperName, isByRef) => `StaticTranslate<bool>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    string: (argHelperName, isByRef) => `StaticTranslate<string>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
    DateTime: (argHelperName, isByRef) => `StaticTranslate<DateTime>.Get((int)data, isolate, Puerts.NativeValueApi.GetValueFromArgument, ${argHelperName}.value, ${isByRef});`,
};

const fixReturn = {
    char: 'Puerts.StaticTranslate<char>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    sbyte: 'Puerts.StaticTranslate<sbyte>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    byte: 'Puerts.StaticTranslate<byte>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    short: 'Puerts.StaticTranslate<short>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    ushort: 'Puerts.StaticTranslate<ushort>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    int: 'Puerts.StaticTranslate<int>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    uint: 'Puerts.StaticTranslate<uint>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    long: 'Puerts.StaticTranslate<long>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    ulong: 'Puerts.StaticTranslate<ulong>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    double: 'Puerts.StaticTranslate<double>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    float: 'Puerts.StaticTranslate<float>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    bool: 'Puerts.StaticTranslate<bool>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    string: 'Puerts.StaticTranslate<string>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
    DateTime: 'Puerts.StaticTranslate<DateTime>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToResult, info, result)',
};
const operatorMap = {
    op_Equality: '==',
    op_Inequality: '!=',
    op_GreaterThan: '>',
    op_LessThan: '<',
    op_GreaterThanOrEqual: '>=',
    op_LessThanOrEqual: '<=',
    op_BitwiseAnd: '&',
    op_BitwiseOr: '|',
    op_Addition: '+',
    op_Subtraction: '-',
    op_Division: '/',
    op_Modulus: '%',
    op_Multiply: '*',
    op_LeftShift: '<<',
    op_RightShift: '>>',
    op_ExclusiveOr: '^',
    op_UnaryNegation: '-',
    op_UnaryPlus: '+',
    op_LogicalNot: '!',
    op_OnesComplement: '~',
    op_False: '',
    op_True: '',
    op_Increment: '++',
    op_Decrement: '--',
};
let csharpKeywords = {};
[
    "abstract", "as", "base", "bool",
    "break", "byte", "case", "catch",
    "char", "checked", "class", "const",
    "continue", "decimal", "default", "delegate",
    "do", "double", "else", "enum",
    "event", "explicit", "extern", "false",
    "finally", "fixed", "float", "for",
    "foreach", "goto", "if", "implicit",
    "in", "int", "interface",
    "internal", "is", "lock", "long",
    "namespace", "new", "null", "object",
    "operator", "out", "override",
    "params", "private", "protected", "public",
    "readonly", "ref", "return", "sbyte",
    "sealed", "short", "sizeof", "stackalloc",
    "static", "string", "struct", "switch",
    "this", "throw", "true", "try",
    "typeof", "uint", "ulong", "unchecked",
    "unsafe", "ushort", "using", "virtual",
    "void", "volatile", "while"
].forEach(keywold => {
    csharpKeywords[keywold] = '@' + keywold;
});

/**
 * this template is for generating the c# wrapper class
 * @param {GenClass.TypeGenInfo} data 
 * @returns 
 */
export default function TypingTemplate(data) {
    let ret = '';
    function getSelf(type) {
        if (data.BlittableCopy) {
            return `(${type.Name}*)self`;
        } else if (type.IsValueType) {
            return `(${type.Name})Puerts.Utils.GetSelf((int)data, self)`;
        } else {
            return `Puerts.Utils.GetSelf((int)data, self) as ${type.Name}`;
        }
    }
    function refSelf() {
        return data.BlittableCopy ? "(*obj)" : "obj";
    }

    function _es6tplJoin(str, ...values) {
        return str.map((strFrag, index) => {
            if (index == str.length - 1) {
                return strFrag;

            } else {
                return strFrag + values[index];
            }
        }).join('');
    }
    function tt(str, ...values) {
        // just append all estemplate values.
        const appendtext = _es6tplJoin(str, ...values)

        ret += appendtext;
    }
    function t(str, ...values) {
        // just append all estemplate values. and indent them;
        const appendtext = _es6tplJoin(str, ...values)

        // indent
        let lines = appendtext.split(/[\n\r]/);
        let newLines = [lines[0]];
        let append = " ".repeat(t.indent);
        for (var i = 1; i < lines.length; i++) {
            if (lines[i]) newLines.push(append + lines[i].replace(/^[\t\s]*/, ''));
        }

        ret += newLines.join('\n');
    }

    t.indent = 0;
    toJsArray(data.Namespaces).forEach(name => {
        t`
        using ${name};
        `
    });

    tt`using Puerts;

namespace PuertsStaticWrap
{
    public static class ${data.WrapClassName}${data.IsGenericWrapper ? `<${makeGenericAlphaBet(data.GenericArgumentsInfo)}>` : ''} ${data.IsGenericWrapper ? makeConstraints(data.GenericArgumentsInfo) : ''}
    {
`
    data.BlittableCopy && tt`
        static ${data.Name} HeapValue;
    `

    // ==================== constructor start ====================
    tt`
        [Puerts.MonoPInvokeCallback(typeof(Puerts.V8ConstructorCallback))]
        ${data.BlittableCopy ? 'unsafe ' : ''}private static IntPtr Constructor(IntPtr isolate, IntPtr info, int paramLen, long data)
        {
            try
            {
`
    if (data.Constructor) {
        toJsArray(data.Constructor.OverloadGroups).forEach(overloadGroup => {
            if (data.Constructor.HasOverloads) {
                tt`
                if (${paramLenCheck(overloadGroup)})
                `
            }
            tt`
                {
            `
            var argumentCodeGenerators = [];
            for (var i = 0; i < overloadGroup.get_Item(0).ParameterInfos.Length; i++) {
                var acg = new ArgumentCodeGenerator(i);
                argumentCodeGenerators.push(acg)
                tt`
                    ${acg.declareAndGetV8Value()};
                    ${acg.declareArgObj()};
                    ${acg.declareArgJSValueType()};
                `
            }
            toJsArray(overloadGroup).forEach(overload => {

                if (data.Constructor.HasOverloads && overload.ParameterInfos.Length > 0) {
                tt`
                    if (${argumentCodeGenerators.map((acg, idx) => {
                        return acg.invokeIsMatch(overload.ParameterInfos.get_Item(idx))
                    }).join(' && ')})
                `;
                }

                tt`
                    {
                `
                argumentCodeGenerators.forEach((acg, idx) => {
                    tt`
                        ${acg.getArg(overload.ParameterInfos.get_Item(idx))};
                    `
                })
                tt`
                        ${data.BlittableCopy ? "HeapValue" : "var result"} = new ${data.Name}(${argumentCodeGenerators.map((acg, idx) => {
                            var paramInfo = overload.ParameterInfos.get_Item(idx);
                            return `${paramInfo.IsOut ? "out " : (paramInfo.IsByRef ? (paramInfo.IsIn ? "in " : "ref ") : "")}${acg.arg()}`
                        }).join(', ')});
                `
                argumentCodeGenerators.forEach((acg, idx) => {
                    var paramInfo = overload.ParameterInfos.get_Item(idx)
                    paramInfo.IsByRef && tt`
                        StaticTranslate<${paramInfo.TypeName}>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToByRefArgument, ${acg.v8Value()}, ${acg.arg()});
                    `
                })
                if (data.BlittableCopy) {
                    tt` 
                        fixed (${data.Name}* result = &HeapValue)
                        {
                            return new IntPtr(result);
                        }
                    `
                } else {
                    tt`
                        return Puerts.Utils.GetObjectPtr((int)data, typeof(${data.Name}), result);
                    `
                }
                tt`
                    }
                `
            })
            tt`
                }
            `
        })
    }
    !data.Constructor || (data.Constructor.OverloadCount != 1) && tt`
                Puerts.PuertsDLL.ThrowException(isolate, "invalid arguments to " + typeof(${data.Name}).GetFriendlyName() + " constructor");
    `
    tt`
    
            } catch (Exception e) {
                Puerts.PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
            return IntPtr.Zero;
        }
    `
    // ==================== constructor end ====================


    // ==================== methods start ====================
    toJsArray(data.Methods).filter(item => !item.IsLazyMember).forEach(method => {
        tt`
        [Puerts.MonoPInvokeCallback(typeof(Puerts.V8FunctionCallback))]
        ${data.BlittableCopy && !method.IsStatic ? 'unsafe ' : ''}private static void ${(method.IsStatic ? "F" : "M")}_${method.Name}(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
                ${!method.IsStatic ? `var obj = ${getSelf(data)};` : ''}
        `
        toJsArray(method.OverloadGroups).forEach(overloadGroup => {
            if (method.HasOverloads) {
                tt`
                if (${paramLenCheck(overloadGroup)})
                `
            }
            tt`
                {
            `
            var argumentCodeGenerators = [];

            // 这里取0可能是因为第一个重载参数肯定是最全的？
            for (var i = 0; i < overloadGroup.get_Item(0).ParameterInfos.Length; i++) {
                var acg = new ArgumentCodeGenerator(i);
                argumentCodeGenerators.push(acg)
                tt`
                    ${acg.declareAndGetV8Value()};
                    ${acg.declareArgObj()};
                    ${acg.declareArgJSValueType()};
                `
            }
            toJsArray(overloadGroup).forEach(overload => {
                if (method.HasOverloads && overload.ParameterInfos.Length > 0) {
                tt`
                    if (${argumentCodeGenerators.map((acg, idx) => {
                        return acg.invokeIsMatch(overload.ParameterInfos.get_Item(idx))
                    }).join(' && ')})
                `
                }
                tt`
                    {
                `
                argumentCodeGenerators.forEach((acg, idx) => {
                    tt`
                        ${acg.getArg(overload.ParameterInfos.get_Item(idx))};
                    `
                })
                tt`
                        ${overload.IsVoid ? "" : "var result = "}${method.IsStatic ? data.Name : refSelf()}.${UnK(method.Name)}(${argumentCodeGenerators.map((acg, idx) => {
                            var paramInfo = overload.ParameterInfos.get_Item(idx);
                            return `${paramInfo.IsOut ? "out " : (paramInfo.IsByRef ? (paramInfo.IsIn ? "in " : "ref ") : "")}${acg.arg()}`
                        }).join(', ')});
                `
                argumentCodeGenerators.forEach((acg, idx) => {
                    overload.ParameterInfos.get_Item(idx).IsByRef && tt`
                        StaticTranslate<${overload.ParameterInfos.get_Item(idx).TypeName}>.Set((int)data, isolate, Puerts.NativeValueApi.SetValueToByRefArgument, ${acg.v8Value()}, ${acg.arg()});
                    `
                })
                tt`
                        ${!overload.IsVoid ? setReturn(overload) + ';' : ''}
                        ${!data.BlittableCopy && !method.IsStatic ? setSelf(data) : ""}
                        ${method.HasOverloads ? 'return;' : ''}
                    }
                `
            })
            tt`
                }
            `
        })
        method.HasOverloads && tt`
                Puerts.PuertsDLL.ThrowException(isolate, "invalid arguments to ${method.Name}");
        `
        tt`
            }
            catch (Exception e)
            {
                Puerts.PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }
        `
    });
    // ==================== methods end ====================

    // ==================== properties start ====================
    toJsArray(data.Properties).filter(property => !property.IsLazyMember).forEach(property => {
        if (property.HasGetter) {
            tt`
        [Puerts.MonoPInvokeCallback(typeof(Puerts.V8FunctionCallback))]
        ${data.BlittableCopy && !property.IsStatic ? 'unsafe ' : ''}private static void G_${property.Name}(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
                ${!property.IsStatic ? `var obj = ${getSelf(data)};` : ''}
                var result = ${property.IsStatic ? data.Name : refSelf()}.${UnK(property.Name)};
                ${setReturn(property)};
            }
            catch (Exception e)
            {
                Puerts.PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }
            `
        }
        if (property.HasSetter) {
            var acg = new ArgumentCodeGenerator(0);
            tt`
        [Puerts.MonoPInvokeCallback(typeof(Puerts.V8FunctionCallback))]
        ${data.BlittableCopy && !property.IsStatic ? 'unsafe ' : ''}private static void S_${property.Name}(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
                ${!property.IsStatic ? `var obj = ${getSelf(data)};` : ''}
                ${acg.declareAndGetV8Value()};
                ${acg.declareArgObj()};
                ${acg.getArg(property)};
                ${property.IsStatic ? data.Name : refSelf()}.${UnK(property.Name)} = ${acg.arg()};
                ${!data.BlittableCopy && !property.IsStatic ? setSelf(data) : ''}
            }
            catch (Exception e)
            {
                Puerts.PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }
            `
        }
    })
    // ==================== properties end ====================

    // ==================== array item get/set start ====================
    if (data.GetIndexs.Length > 0) {
        var acg = new ArgumentCodeGenerator(0);
        tt`
        [Puerts.MonoPInvokeCallback(typeof(Puerts.V8FunctionCallback))]
        ${data.BlittableCopy ? 'unsafe ' : ''}private static void GetItem(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
                var obj = ${getSelf(data)};
                ${acg.declareAndGetV8Value()};
                ${acg.declareArgObj()};
                ${acg.declareArgJSValueType()};
        `
        toJsArray(data.GetIndexs).forEach(indexInfo => {
            tt`
                if (${acg.invokeIsMatch(indexInfo.IndexParameter)})
                {
                    ${acg.getArg(indexInfo.IndexParameter)};
                    var result = ${refSelf()}[${acg.arg()}];
                    ${setReturn(indexInfo)};
                    return;
                }
            `;
        })

        tt`
            }
            catch (Exception e)
            {
                Puerts.PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }
        `
    }
    if (data.SetIndexs.Length > 0) {
        var keyAcg = new ArgumentCodeGenerator(0);
        tt`
        [Puerts.MonoPInvokeCallback(typeof(Puerts.V8FunctionCallback))]
        ${data.BlittableCopy ? 'unsafe ' : ''}private static void SetItem(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
                var obj = ${getSelf(data)};
                ${keyAcg.declareAndGetV8Value()};
                ${keyAcg.declareArgObj()};
                ${keyAcg.declareArgJSValueType()};
        `
        toJsArray(data.SetIndexs).forEach(indexInfo => {
            var valueAcg = new ArgumentCodeGenerator(1);
            tt`
                if (${keyAcg.invokeIsMatch(indexInfo.IndexParameter)})
                {
                    ${keyAcg.getArg(indexInfo.IndexParameter)};

                    ${valueAcg.declareAndGetV8Value()};
                    ${valueAcg.declareArgObj()};
                    ${valueAcg.getArg(indexInfo)};

                    ${refSelf()}[${keyAcg.arg()}] = ${valueAcg.arg()};
                    return;
                }
            `;
        })
        tt`
            }
            catch (Exception e)
            {
                Puerts.PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }
        `
    }
    // ==================== array item get/set end ====================

    // ==================== operator start ====================
    toJsArray(data.Operators).filter(oper => !oper.IsLazyMember).forEach(operator => {
        tt`
        [Puerts.MonoPInvokeCallback(typeof(Puerts.V8FunctionCallback))]
        private static void O_${operator.Name}(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
        `
        toJsArray(operator.OverloadGroups).forEach(overloadGroup => {
            operator.HasOverloads && tt`
                if (${paramLenCheck(overloadGroup)})
            `
            tt`
                {
            `
            var argumentCodeGenerators = [];
            for (var i = 0; i < overloadGroup.get_Item(0).ParameterInfos.Length; i++) {
                var acg = new ArgumentCodeGenerator(i);
                argumentCodeGenerators.push(acg)
                tt`
                    ${acg.declareAndGetV8Value()};
                    ${acg.declareArgObj()};
                    ${acg.declareArgJSValueType()};
                `
            }
            toJsArray(overloadGroup).forEach(overload => {
                if (operator.HasOverloads && overload.ParameterInfos.Length > 0) {
                    tt`
                    if (${argumentCodeGenerators.map((acg, idx) => {
                        return acg.invokeIsMatch(overload.ParameterInfos.get_Item(idx))
                    }).join(' && ')})
                    `
                }
                tt`
                    {
                `
                argumentCodeGenerators.forEach((acg, idx) => {
                    tt` 
                        ${acg.getArg(overload.ParameterInfos.get_Item(idx))};
                    `
                })
                tt`
                        var result = ${operatorCall(operator.Name, argumentCodeGenerators, overload)};
                        ${!overload.IsVoid ? setReturn(overload) + ';' : ''}
                        ${operator.HasOverloads ? 'return;' : ''}
                    }
                `
            })
            tt`
                }
            `
        })
        operator.HasOverloads && tt`
                Puerts.PuertsDLL.ThrowException(isolate, "invalid arguments to ${operator.Name}");
        `
        tt`
            }
            catch (Exception e)
            {
                Puerts.PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }
        `
    })
    // ==================== operator end ====================

    // ==================== events start ====================
    toJsArray(data.Events).filter(ev => !ev.IsLazyMember).forEach(eventInfo => {
        if (eventInfo.HasAdd) {
            var acg = new ArgumentCodeGenerator(0);
            tt`
        [Puerts.MonoPInvokeCallback(typeof(Puerts.V8FunctionCallback))]
        ${data.BlittableCopy && !eventInfo.IsStatic ? 'unsafe ' : ''}private static void A_${eventInfo.Name}(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
                ${!eventInfo.IsStatic ? `var obj = ${getSelf(data)};` : ''}
                ${acg.declareAndGetV8Value()};
                ${acg.declareArgObj()};
                ${acg.getArg(eventInfo)};
                ${eventInfo.IsStatic ? data.Name : refSelf()}.${eventInfo.Name} += ${acg.arg()};
            }
            catch (Exception e)
            {
                Puerts.PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }
            `
        }
        if (eventInfo.HasRemove) {
            var acg = new ArgumentCodeGenerator(0);
            tt`
        [Puerts.MonoPInvokeCallback(typeof(Puerts.V8FunctionCallback))]
        ${data.BlittableCopy && !eventInfo.IsStatic ? 'unsafe' : ''}private static void R_${eventInfo.Name}(IntPtr isolate, IntPtr info, IntPtr self, int paramLen, long data)
        {
            try
            {
                ${!eventInfo.IsStatic ? `var obj = ${getSelf(data)};` : ''}
                ${acg.declareAndGetV8Value()};
                ${acg.declareArgObj()};
                ${acg.getArg(eventInfo)};
                ${eventInfo.IsStatic ? data.Name : refSelf()}.${eventInfo.Name} -= ${acg.arg()};
            }
            catch (Exception e)
            {
                Puerts.PuertsDLL.ThrowException(isolate, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
            }
        }
            `
        }
    })
    // ==================== events end ====================
    tt`    
        public static Puerts.TypeRegisterInfo GetRegisterInfo()
        {
            return new Puerts.TypeRegisterInfo()
            {
                BlittableCopy = ${data.BlittableCopy},
                Constructor = Constructor,
                Methods = new System.Collections.Generic.Dictionary<Puerts.MethodKey, Puerts.V8FunctionCallback>()
                {   ${[
            toJsArray(data.Methods).filter(p => !p.IsLazyMember).map(method => `
                    { new Puerts.MethodKey { Name = "${method.Name}", IsStatic = ${method.IsStatic}}, ${(method.IsStatic ? "F" : "M")}_${method.Name} }`).join(','),
            data.GetIndexs.Length > 0 ? `
                    { new Puerts.MethodKey { Name = "get_Item", IsStatic = false}, GetItem }\n` : '',
            data.SetIndexs.Length > 0 ? `
                    { new Puerts.MethodKey { Name = "set_Item", IsStatic = false}, SetItem }\n` : '',
            toJsArray(data.Operators).filter(p => !p.IsLazyMember).map(operator => `
                    { new Puerts.MethodKey { Name = "${operator.Name}", IsStatic = true}, O_${operator.Name} }`).join(',\n'),
            toJsArray(data.Events).filter(p => !p.IsLazyMember).map(eventInfo => {
                const ret = [];
                if (eventInfo.HasAdd) {
                    ret.push(`
                    { new Puerts.MethodKey { Name = "add_${eventInfo.Name}", IsStatic = ${eventInfo.IsStatic}}, A_${eventInfo.Name} }`)
                }
                if (eventInfo.HasRemove) {
                    ret.push(`
                    { new Puerts.MethodKey { Name = "remove_${eventInfo.Name}", IsStatic = ${eventInfo.IsStatic}},  R_${eventInfo.Name} }`)
                }
                return ret.join(',')
            }).join(',\n')
        ].filter(str => str.trim()).join(',\n')}
                },
                Properties = new System.Collections.Generic.Dictionary<string, Puerts.PropertyRegisterInfo>()
                {
                    ${toJsArray(data.Properties).filter(p => !p.IsLazyMember).map(property => `
                    {"${property.Name}", new Puerts.PropertyRegisterInfo(){ IsStatic = ${property.IsStatic}, Getter = ${property.HasGetter ? "G_" + property.Name : "null"}, Setter = ${property.HasSetter ? "S_" + property.Name : "null"}} }`).join(',\n')}
                },
                LazyMembers = new System.Collections.Generic.List<Puerts.LazyMemberRegisterInfo>()
                {   ${toJsArray(data.LazyMembers).map(item => {
            return `
                    new Puerts.LazyMemberRegisterInfo() { Name = "${item.Name}", IsStatic = ${item.IsStatic}, Type = (Puerts.LazyMemberType)${item.Type}, HasGetter = ${item.HasGetter}, HasSetter = ${item.HasSetter} }`
        })}
                }
            };
        }
    `
    if (data.BlittableCopy) {
        tt`
        unsafe private static ${data.Name} StaticGetter(int jsEnvIdx, IntPtr isolate, Puerts.IGetValueFromJs getValueApi, IntPtr value, bool isByRef)
        {
            ${data.Name}* result = (${data.Name}*)getValueApi.GetNativeObject(isolate, value, isByRef);
            return result == null ? default(${data.Name}) : *result;
        }

        unsafe private static void StaticSetter(int jsEnvIdx, IntPtr isolate, Puerts.ISetValueToJs setValueApi, IntPtr value, ${data.Name} val)
        {
            HeapValue = val;
            fixed (${data.Name}* result = &HeapValue)
            {
                var typeId = Puerts.JsEnv.jsEnvs[jsEnvIdx].GetTypeId(typeof(${data.Name}));
                setValueApi.SetNativeObject(isolate, value, typeId, new IntPtr(result));
            }
        }
        
        public static void InitBlittableCopy(Puerts.JsEnv jsEnv)
        {
            Puerts.StaticTranslate<${data.Name}>.ReplaceDefault(StaticSetter, StaticGetter);
            jsEnv.RegisterGeneralGetSet(typeof(${data.Name}), (int jsEnvIdx, IntPtr isolate, Puerts.IGetValueFromJs getValueApi, IntPtr value, bool isByRef) =>
            {
                return StaticGetter(jsEnvIdx, isolate, getValueApi, value, isByRef);
            }, (int jsEnvIdx, IntPtr isolate, Puerts.ISetValueToJs setValueApi, IntPtr value, object obj) => 
            {
                StaticSetter(jsEnvIdx, isolate, setValueApi, value, (${data.Name})obj);
            });
        }
        `
    }

    tt`
    }
}
`
    return ret;
}

function toJsArray(csArr) {
    let arr = [];
    for (var i = 0; i < csArr.Length; i++) {
        arr.push(csArr.get_Item(i));
    }
    return arr;
}
function UnK(identifier) {
    return csharpKeywords.hasOwnProperty(identifier) ? csharpKeywords[identifier] : identifier;
}

function setReturn(typeInfo) {
    let typeName = typeInfo.TypeName;
    if (typeName in fixReturn) {
        return fixReturn[typeName];
    } else if (typeInfo.IsEnum) {
        return fixReturn[typeInfo.UnderlyingTypeName].replace('result', `(${typeInfo.UnderlyingTypeName})result`);
    } else {
        return `Puerts.ResultHelper.Set((int)data, isolate, info, result)`;
    }
}
function operatorCall(methodName, acgList, typeInfo) {
    if (methodName == 'op_Implicit') {
        return `(${typeInfo.TypeName})${acgList[0].arg()}`;
    }
    if (acgList.length == 1) {
        return operatorMap[methodName] + acgList[0].arg();
    } else if (acgList.length == 2) {
        return [acgList[0].arg(), operatorMap[methodName], acgList[1].arg()].join(' ')
    }
}

function setSelf(type) {
    if (type.IsValueType) {
        return `Puerts.Utils.SetSelf((int)data, self, obj);`
    } else {
        return '';
    }
}


function paramLenCheck(group) {
    let len = group.get_Item(0).ParameterInfos.Length;
    return group.get_Item(0).HasParams ? `paramLen >= ${len - 1}` : `paramLen == ${len}`;
}

function makeGenericAlphaBet(info) {
    const arr = [];
    for (var i = 0; i < info.Length; i++) {
        arr.push(info.get_Item(i).Name);
    }
    return arr.join(',')
}

function makeConstraints(info) {
    const ret = [];
    if (info.Length == 0) {
        return '';
    }
    for (var i = 0; i < info.Length; i++) {
        const item = info.get_Item(i);
        if (item.Constraints.Length == 0) {
            continue;
        }
        var consstr = [];
        for (var j = 0; j < item.Constraints.Length; j++) {
            consstr.push(item.Constraints.get_Item(j));
        }
        ret.push(`where ${item.Name} : ` + consstr.join(', '))
    }
    return ret.join(' ');
}