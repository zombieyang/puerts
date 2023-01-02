import { FOR, default as t } from "./lib/tte.mjs"

class WrapperInfo {

    constructor(methodInfo, constructorInfo, parameterInfos) {
        this.methodInfo = methodInfo;
        this.constructorInfo = constructorInfo;
        this.parameterInfos = parameterInfos;
    }
}

const DictStrStr = puer.$generic(CS.System.Collections.Generic.Dictionary$2, CS.System.String, CS.System.String);
export function GenByMethodInfoAndConstructorInfo(mis, cis) {
    const misJSArr = listToJsArray(mis)
    const cisJSArr = listToJsArray(cis)

    const infoMap = new Map();
    misJSArr.forEach(mi => {
        const pis = mi.GetParameters();
        for (let i = pis.Length - 1; i >= 0; i--) {
            if (!pis.get_Item(i).HasDefaultValue) break;

            const typeWrapInfos = infoMap.get(mi.DeclaringType) || [];
            typeWrapInfos.push(new WrapperInfo(mi, null, listToJsArray(pis, {take: i})));
            infoMap.set(mi.DeclaringType, typeWrapInfos);
        }
    })
    cisJSArr.forEach(ci => {
        const pis = ci.GetParameters();
        for (let i = pis.Length - 1; i >= 0; i--) {
            if (!pis.get_Item(i).HasDefaultValue) break;

            const typeWrapInfos = infoMap.get(ci.DeclaringType) || [];
            typeWrapInfos.push(new WrapperInfo(null, ci, listToJsArray(pis, {take: i})));
            infoMap.set(ci.DeclaringType, typeWrapInfos);
        }
    });

    const dict = new DictStrStr();
    for (let type of infoMap.keys()) {
        dict.Add(getWrapperName(type) + "_PuerDVAdaptor.cs", renderAdaptor(type, infoMap.get(type)));
    }

    dict.Add("DefaultValueAdaptors.cs", renderRegister(infoMap.keys()))

    return dict;
}

function renderRegister(types) {
    return t`using System;
using System.Collections.Generic;

namespace PuertsStaticWrap
{
    public static class DefaultValueAdaptors
    {
        public static Dictionary<Type, Type> AdaptorsDict = new Dictionary<Type, Type>
        {
            ${FOR(types, type => {
                return `
            {typeof(${friendyNameWithoutGenericArguments(type.GetFriendlyName())}), typeof(${getWrapperName(type) + "_PuerDVAdaptor"})},
            `
            })}
        };
    }
}`
}

function renderAdaptor(type, wrapperInfoArr) {
    const isGenericType = type.IsGenericType;

    return t`namespace PuertsStaticWrap {
    // ${CS.System.IO.Path.GetFileName(type.Assembly.Location)}
    public static class ${getWrapperName(type)}_PuerDVAdaptor {
    ${FOR(wrapperInfoArr, item=> {
        if (item.constructorInfo) {
            return `
        public static ${item.constructorInfo.DeclaringType.GetFriendlyName()} ctor${isGenericType ? `<${listToJsArray(type.GetGenericArguments()).map(ga=> ga.Name).join(', ')}>` : ''} (
            ${
                item.parameterInfos
                    .map((pi, index)=> (pi.ParameterType.IsByRef ? pi.ParameterType.GetElementType() : pi.ParameterType).GetFriendlyName() + " p" + index)
                    .join(', ')
            }
        )${isGenericType ? makeConstraints(listToJsArray(type.GetGenericArguments())) : ''} {
            return new ${item.constructorInfo.DeclaringType.GetFriendlyName()}(${item.parameterInfos.map((pi, index)=> refPrefix(pi) + "p" + index).join(', ')});
        }
            `;

        } else {
            return `
        public static ${item.methodInfo.ReturnType.GetFriendlyName()} ${item.methodInfo.Name}${isGenericType ? `<${listToJsArray(type.GetGenericArguments()).map(ga=> ga.Name).join(', ')}>` : ''} (
            ${
                [item.methodInfo.IsStatic ? '' : item.methodInfo.DeclaringType.GetFriendlyName() + " __this"]
                    .concat(item.parameterInfos.map((pi, index)=> (pi.ParameterType.IsByRef ? pi.ParameterType.GetElementType() : pi.ParameterType).GetFriendlyName() + " p" + index))
                    .filter(p=> p)
                    .join(', ')
            }
        )${isGenericType ? makeConstraints(listToJsArray(type.GetGenericArguments())) : ''} {
            ${item.methodInfo.ReturnType == puer.$typeof(CS.System.Void) ? '' : 'return '}${item.methodInfo.IsStatic ? item.methodInfo.DeclaringType.GetFriendlyName() : '__this'}.${item.methodInfo.Name}(${item.parameterInfos.map((pi, index)=> refPrefix(pi) + "p" + index).join(', ')});
        }
            `;
        }
    })}
    }
}`;
}

function listToJsArray(csArr, { take } = {}) {
    let arr = [];
    if (!csArr) return arr;
    const length = typeof(take) == 'undefined' ? csArr.Length : take;
    for (var i = 0; i < length; i++) {
        arr.push(csArr.get_Item(i));
    }
    return arr;
}
function getWrapperName(type) {
    return type.ToString().replaceAll("+", "_").replaceAll(".", "_").replaceAll("`", "_").replaceAll("&", "_").replaceAll("[", "_").replaceAll("]", "_").replaceAll(",", "_");
}
function refPrefix(paramInfo) {
    return `${paramInfo.IsOut ? "out " : (paramInfo.IsByRef ? (paramInfo.IsIn ? "in " : "ref ") : "")}`
}
const GenericParameterAttributes = CS.System.Reflection.GenericParameterAttributes;
function makeConstraints(gas) {
    const ret = [];
    for (var i = 0; i < gas.length; i++) {
        const ga = gas[i];
        var consstr = [];

        var contraintTypes = listToJsArray(ga.GetGenericParameterConstraints());
        var constraints = ga.GenericParameterAttributes &
        GenericParameterAttributes.SpecialConstraintMask;

        var hasValueTypeConstraint = false;
        for (var j = 0; j < contraintTypes.length; j++)
        {
            if (contraintTypes[j] == puer.$typeof(CS.System.ValueType))
            {
                hasValueTypeConstraint = true;
                continue;
            }
            consstr.push(contraintTypes[j].GetFriendlyName());
        }
        if ((constraints & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
            consstr.unshift("class");
        if (hasValueTypeConstraint && (constraints & GenericParameterAttributes.DefaultConstructorConstraint) != 0 && (constraints & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            consstr.push("struct");
        else if ((constraints & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
            consstr.push("new()");

        consstr.length && ret.push(`where ${ga.Name} : ` + consstr.join(', '))
    }
    return ' ' + ret.join(' ');
}
function friendyNameWithoutGenericArguments(name) {
    return name.replace(/<([\w\s,])*>/, '<>');
}