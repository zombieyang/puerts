/*
 * Tencent is pleased to support the open source community by making Puerts available.
 * Copyright (C) 2020 THL A29 Limited, a Tencent company.  All rights reserved.
 * Puerts is licensed under the BSD 3-Clause License, except for the third-party components listed in the file 'LICENSE' which may be subject to their corresponding license terms. 
 * This file is subject to the terms and conditions defined in file 'LICENSE', which is part of this source code package.
 */

var global = global || globalThis || (function () { return this; }());


function csTypeToClass(csType) {
    let cls = puer.loadType(csType);
    
    if (cls) {
        let currentCls = cls, parentPrototype = Object.getPrototypeOf(currentCls.prototype);

        // 此处parentPrototype如果是一个泛型，会丢失父父的继承信息，必须循环找下去
        while (parentPrototype) {
            Object.setPrototypeOf(currentCls, parentPrototype.constructor);//v8 api的inherit并不能把静态属性也继承，通过这种方式修复下
            currentCls.__static_inherit__ = true;

            currentCls = parentPrototype.constructor;
            parentPrototype = Object.getPrototypeOf(currentCls.prototype);
            if (currentCls === Object || currentCls === Function || currentCls.__static_inherit__) break;
        }

        let readonlyStaticMembers;
        if (readonlyStaticMembers = cls.__puertsMetadata.get('readonlyStaticMembers')) {
            for (var key in cls) {
                let desc = Object.getOwnPropertyDescriptor(cls, key);
                if (readonlyStaticMembers.has(key) && desc && (typeof desc.get) == 'function' && (typeof desc.value) == 'undefined') {
                    let getter = desc.get;
                    let value;
                    let valueGetted = false;
    
                    Object.defineProperty(
                        cls, key, 
                        Object.assign(desc, {
                            get() {
                                if (!valueGetted) {
                                    value = getter();
                                    valueGetted = true;
                                }
                                
                                return value;
                            },
                            configurable: false
                        })
                    );
                    if (cls.__p_isEnum) {
                        const val = cls[key];
                        if ((typeof val) == 'number') {
                            cls[val] = key;
                        }
                    }
                }
            }
        }

        let nestedTypes = puer.getNestedTypes(csType);
        if (nestedTypes) {
            for(var i = 0; i < nestedTypes.Length; i++) {
                let ntype = nestedTypes.get_Item(i);
                if (ntype.IsGenericType) {
                    let name = ntype.Name.split('`')[0] + '$' + ntype.GetGenericArguments().Length;
                    let fullName = ntype.FullName.split('`')[0]/**.replace(/\+/g, '.') */ + '$' + ntype.GetGenericArguments().Length;
                    let genericTypeInfo = cls[name] = new Map();
                    genericTypeInfo.set('$name', fullName.replace('$', '`'));
                } else {
                    cls[ntype.Name] = csTypeToClass(ntype);
                }
            }
        }
    }
    return cls;
}

function Namespace() {}
puer.__$NamespaceType = Namespace;

function createTypeProxy(namespace) {
    return new Proxy(new Namespace, {
        get: function(cache, name) {
            if (!(name in cache)) {
                let fullName = namespace ? (namespace + '.' + name) : name;
                if (/\$\d+$/.test(name)) {
                    let genericTypeInfo = cache[name] = new Map();
                    genericTypeInfo.set('$name', fullName.replace('$', '`'));

                } else {
                    let cls = csTypeToClass(fullName);
                    if (cls) {
                        cache[name] = cls;
                    } else {
                        cache[name] = createTypeProxy(fullName);
                        //console.log(fullName + ' is a namespace');
                    }
                }
            }
            return cache[name];
        }
    });
}

let csharpModule = createTypeProxy(undefined);
csharpModule.default = csharpModule;
global.CS = csharpModule;

csharpModule.System.Object.prototype.toString = csharpModule.System.Object.prototype.ToString;

function ref(x) {
    return [x];
}

function unref(r) {
    return r[0];
}

function setref(x, val) {
    x[0] = val;
}

function taskToPromise(task) {
    return new Promise((resolve, reject) => {
        task.GetAwaiter().UnsafeOnCompleted(() => {
            let t = task;
            task = undefined;
            if (t.IsFaulted) {
                if (t.Exception) {
                    if (t.Exception.InnerException) {
                        reject(t.Exception.InnerException.Message);
                    } else {
                        reject(t.Exception.Message);
                    }
                } else {
                    reject("unknow exception!");
                }
            } else {
                resolve(t.Result);
            }
        });
    });
}
function genIterator(obj) {
    let it = obj.GetEnumerator();
    return {
        next() {
            if (it.MoveNext())
            {
                return {value: it.Current, done: false}
            }
            it.Dispose();
            return {value: null, done: true}
        }
    };
}

function makeGeneric(genericTypeInfo, ...genericArgs) {
    let p = genericTypeInfo;
    for (var i = 0; i < genericArgs.length; i++) {
        let genericArg = genericArgs[i];
        if (!p.has(genericArg)) {
            p.set(genericArg, new Map());
        }
        p = p.get(genericArg);
    }
    if (!p.has('$type')) {

        let typName = genericTypeInfo.get('$name')
        let typ = puer.loadType(typName, ...genericArgs)
        if (getType(csharpModule.System.Collections.IEnumerable).IsAssignableFrom(getType(typ))) {
            typ.prototype[Symbol.iterator] = function () {
                return genIterator(this);
            }
        }
        p.set('$type', typ);
    }
    return p.get('$type');
}

function makeGenericMethod(cls, methodName, ...genericArgs) {
    if (cls && typeof methodName == 'string' && genericArgs && genericArgs.length > 0) {
        return puer.getGenericMethod(puer.$typeof(cls), methodName, ...genericArgs);
        
    } else {
        throw new Error("invalid arguments for makeGenericMethod");
    }
}

function getType(cls) {
    return cls.__p_innerType;
}

function bindThisToFirstArgument(func, parentFunc) {
    if (parentFunc) {
        return function (...args) {
            try {
                return func.apply(null, [this, ...args]);
            } catch {
                return parentFunc.call(this, ...args);
            };
        }
    }
    return function(...args) {
        return func.apply(null, [this, ...args]);
    }
}

function doExtension(cls, extension) {
    // if you already generate static wrap for cls and extension, then you are no need to invoke this function
    // 如果你已经为extension和cls生成静态wrap，则不需要调用这个函数。
    var parentPrototype = Object.getPrototypeOf(cls.prototype);
    Object.keys(extension).forEach(key=> {
        var func = extension[key];
        if (typeof func == 'function' && key != 'constructor' && !(key in cls.prototype)) {
    var parentFunc = parentPrototype ? parentPrototype[key] : undefined;
            parentFunc = typeof parentFunc === "function" ? parentFunc : undefined;
            Object.defineProperty(cls.prototype, key, {
                value: bindThisToFirstArgument(func, parentFunc),
                writable: false,
                configurable: false
            });
        }
    })
}

puer.$ref = ref;
puer.$unref = unref;
puer.$set = setref;
puer.$promise = taskToPromise;
puer.$generic = makeGeneric;
puer.$genericMethod = makeGenericMethod;
puer.$typeof = getType;
puer.$extension = (cls, extension) => { 
    typeof console != 'undefined' && console.warn(`deprecated! if you already generate static wrap for ${cls} and ${extension}, you are no need to invoke $extension`); 
    return doExtension(cls, extension)
};

(function() {
    const csDynamicBinderBridge = CS.Puerts.DynamicBinderBridge;

    var MemberTypes = {
        Invalid: 0,
        Constructor: 1,
        Event: 2,
        Field: 4,
        Method: 8,
        Property: 16,
        TypeInfo: 32,
        Custom: 64,
        NestedType: 128,
        All: 191
    }

    const ENABLE_LOG = true;

    function add_reflection_api(
        js_class, api_name, is_static, member_types, level = 0,
    ) {
        const tryLevel = level;
        function add_reflection_api_log(info, is_error = false) {
            if (!ENABLE_LOG) return;
            const class_name = js_class.name.split(',')[0];
            const log_func = is_error ? global.console.error :  global.console.log;
            log_func(`[reflection_api] ${class_name}::${api_name} ${is_static ? 'static' : 'instance'} ${info}\n ${new Error().stack}`);
        }

        while (level > 0) {
            js_class = Object.getPrototypeOf(js_class.prototype).constructor;
            level = level - 1;
        }
        if (js_class === CS.System.Object) {
            add_reflection_api_log(`register api failed! tried parent search level: ${tryLevel}`, true);
            return false;
        }
        const cs_type = puerts.$typeof(js_class);
        const target = is_static ? js_class : js_class.prototype;
        const member_type = csDynamicBinderBridge.DynamicGetMemberType(null, cs_type, api_name, is_static);
        add_reflection_api_log(`get member type: ${MemberTypes[member_type]}`);
        if (member_type === 0) {
            // return null will search parent
            return null;
        }
        const member_type_matched = (member_type & member_types) > 0;
        if (!member_type_matched) {
            add_reflection_api_log(`register api failed! tried parent search level: ${tryLevel}, member filter: ${member_types}`, true);
            return false;
        }
        if (member_type === MemberTypes.Field || member_type === MemberTypes.Property) {
            const getter_setter = function (val = undefined) {
                if (typeof val !== 'undefined') {
                    add_reflection_api_log('called setter');
                    return csDynamicBinderBridge.DynamicSetStatic(is_static ? null : this, cs_type, api_name, val);
                }
                add_reflection_api_log('called getter');
                return csDynamicBinderBridge.DynamicGetStatic(is_static ? null : this, cs_type, api_name);
            };
            Object.defineProperty(target, api_name, {
                get: getter_setter,
                set: getter_setter,
            });
            add_reflection_api_log('register api success');
            return true;
        } 
        if (member_type === MemberTypes.Method) {
            Object.defineProperty(target, api_name, {
                value(...args) {
                    add_reflection_api_log('called function');
                    return csDynamicBinderBridge.DynamicInvokeStatic(is_static ? null : this, cs_type, api_name, ...args);
                },
            });
            add_reflection_api_log('register api success');
            return true;
        }
        // return null will search parent
        return null;
    }

    function add_reflection_api_hierarchy(classOrObj, api_name, member_types) {
        const is_static = Object.prototype.hasOwnProperty.call(classOrObj, '__p_innerType');
        const js_class = is_static ? classOrObj : Object.getPrototypeOf(classOrObj).constructor;
        let level = 0;
        while (true) {
            const result = add_reflection_api(js_class, api_name, is_static, member_types, level);
            if (result !== null) {
                if (result === false) {
                    const target = is_static ? js_class : js_class.prototype;
                    Reflect.defineProperty(target, api_name, { value: null });
                }
                return result;
            }
            level = level + 1;
        }
    }

    Object.setPrototypeOf(CS.System.Object.prototype, new Proxy({}, {
        get(t, p, r) {
            if (typeof(p) === 'string' && add_reflection_api_hierarchy(r, p, MemberTypes.All)) {
                return r[p];
            }
        },
        set(t, p, v, r) {
            if (typeof(p) === 'string' && add_reflection_api_hierarchy(r, p, MemberTypes.Field | MemberTypes.Property)) {
                r[p] = v;
                return true;
            }
            return Reflect.set(t, p, v, r);
        },
    }));

    Object.setPrototypeOf(CS.System.Object, new Proxy({}, {
        get(t, p, r) {
            if (p === '__p_isEnum' || p === '__static_inherit__') {
                return null;
            }
            if (typeof(p) === 'string' && add_reflection_api_hierarchy(r, p, MemberTypes.All)) {
                return r[p];
            }
        },
        set(t, p, v, r) {
            if (p === '__static_inherit__' || p === '__puertsMetadata' || Object.prototype.hasOwnProperty.call(v, '__p_innerType') || r.__p_isEnum) {
                return Reflect.set(t, p, v, r);
            }
            if (typeof(p) === 'string' && add_reflection_api_hierarchy(r, p, MemberTypes.Field | MemberTypes.Property)) {
                r[p] = v;
                return true;
            }
            return Reflect.set(t, p, v, r);
        },
    }));
})()