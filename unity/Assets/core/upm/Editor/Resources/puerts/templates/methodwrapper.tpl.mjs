import { FOR, default as t } from "./tte.mjs"

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
        dict.Add(type.GetFriendlyName() + "_PuerWrap.cs", renderType(type, infoMap.get(type)));
    }

    return dict;
}

function renderType(type, wrapperInfoArr) {
    return t`namespace Puerts {
        public static class ${type.ToString().replaceAll("+", "_").replaceAll(".", "_").replaceAll("`", "_").replaceAll("&", "_").replaceAll("[", "_").replaceAll("]", "_").replaceAll(",", "_")}_PuerWrap {
            ${FOR(wrapperInfoArr, item=> {
                if (item.constructorInfo) {
                    return `
                    public static ${item.constructorInfo.DeclaringType.GetFriendlyName()} ctor (
                        ${
                            item.parameterInfos
                                .map((pi, index)=> (pi.ParameterType.IsByRef ? pi.ParameterType.GetElementType() : pi.ParameterType).GetFriendlyName() + " p" + index)
                                .join(', ')
                        }
                    ) {
                        return new ${item.constructorInfo.DeclaringType.GetFriendlyName()}(${listToJsArray(item.parameterInfos).map((pi, index)=> "p" + index).join(', ')});
                    }
                    `;
    
                } else {
                    return `
                    public static ${item.methodInfo.ReturnType.GetFriendlyName()} ${item.methodInfo.Name}(
                        ${
                            [item.methodInfo.IsStatic ? '' : item.methodInfo.DeclaringType.GetFriendlyName() + " __this"]
                                .concat(item.parameterInfos.map((pi, index)=> (pi.ParameterType.IsByRef ? pi.ParameterType.GetElementType() : pi.ParameterType).GetFriendlyName() + " p" + index))
                                .filter(p=> p)
                                .join(', ')
                        }
                    ) {
                        return ${item.methodInfo.IsStatic ? item.methodInfo.DeclaringType.GetFriendlyName() : '__this'}.${item.methodInfo.Name}(${listToJsArray(item.parameterInfos).map((pi, index)=> "p" + index).join(', ')});
                    }
                    `;
                }
            })}
        }
    }`;
}

export default function Gen(csarr) {
    const jsarr = listToJsArray(csarr)
    return t`namespace Puerts {
    public static class MethodWraps {
        ${FOR(jsarr, item=> {
            if (item.IsConstructor) {
                return `
                public static ${item.ConstructorInfo.DeclaringType.GetFriendlyName()} ctor (
                    ${
                        listToJsArray(item.ParameterInfos)
                            .map((pi, index)=> (pi.ParameterType.IsByRef ? pi.ParameterType.GetElementType() : pi.ParameterType).GetFriendlyName() + " p" + index)
                            .join(', ')
                    }
                ) {
                    return new ${item.ConstructorInfo.DeclaringType.GetFriendlyName()}(${listToJsArray(item.ParameterInfos).map((pi, index)=> "p" + index).join(', ')});
                }
                `;

            } else {
                return `
                public static ${item.MethodInfo.ReturnType.GetFriendlyName()} ${item.MethodInfo.Name}(
                    ${
                        [item.MethodInfo.IsStatic ? '' : item.MethodInfo.DeclaringType.GetFriendlyName() + " __this"]
                            .concat(listToJsArray(item.ParameterInfos).map((pi, index)=> (pi.ParameterType.IsByRef ? pi.ParameterType.GetElementType() : pi.ParameterType).GetFriendlyName() + " p" + index))
                            .filter(p=> p)
                            .join(', ')
                    }
                ) {
                    return ${item.MethodInfo.IsStatic ? item.MethodInfo.DeclaringType.GetFriendlyName() : '__this'}.${item.MethodInfo.Name}(${listToJsArray(item.ParameterInfos).map((pi, index)=> "p" + index).join(', ')});
                }
                `;
            } 
        })}    
    }
}`
}

function listToJsArray(csArr, { take } = {}) {
    let arr = [];
    if (!csArr) return arr;
    for (var i = 0; i < (take || csArr.Length); i++) {
        arr.push(csArr.get_Item(i));
    }
    return arr;
}