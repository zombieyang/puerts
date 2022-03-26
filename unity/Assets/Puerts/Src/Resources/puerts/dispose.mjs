var global = global || globalThis || (function () { return this; }());
global.puerts.disposed = true;

const goodbye = function() { throw new Error('puerts has disposed'); }

puerts.loadType = goodbye
puerts.getNestedTypes = goodbye

setToGoodbyeFuncRecursive(require('csharp'));

function setToGoodbyeFuncRecursive(obj) {
    Object.keys(obj).forEach(key=> {
        if (obj[key] == obj) {
            return; // a member named default is the obj itself which is in the root
        }
        setToGoodbyeFuncRecursive(obj[key])

        if (typeof obj[key] == 'function' && obj[key].prototype) {
            const prototype = obj[key].prototype;
            Object.keys(prototype).forEach((pkey)=> {
                // if (Object.getOwnPropertyDescriptor(prototype, pkey).configurable) {
                    Object.defineProperty(prototype, pkey, {
                        get: goodbye,
                        set: goodbye,
                    })
                // }
            })
            Object.keys(obj[key]).forEach((skey)=> {
                if (Object.getOwnPropertyDescriptor(obj[key], skey).configurable) {
                    Object.defineProperty(obj[key], skey, {
                        get: goodbye,
                        set: goodbye,
                    })
                }
            })
        }
        if (obj[key] instanceof puerts.__$NamespaceType) {
            Object.defineProperty(obj, key, {
                get: goodbye,
                set: goodbye
            })
        }
    });
}