/*
 * Tencent is pleased to support the open source community by making Puerts available.
 * Copyright (C) 2020 THL A29 Limited, a Tencent company.  All rights reserved.
 * Puerts is licensed under the BSD 3-Clause License, except for the third-party components listed in the file 'LICENSE' which may be subject to their corresponding license terms. 
 * This file is subject to the terms and conditions defined in file 'LICENSE', which is part of this source code package.
 */

var global = global || globalThis || (function () { return this; }());
// polyfill old code after use esm module.
global.global = global;

let puerts = global.puerts = global.puerts || {};

const disposeChecker = () => { if (puerts.disposed) throw new Error('puerts is disposed') };
puerts.loadType = wrapAndDeletePuertsHook('__tgjsLoadType', disposeChecker);
puerts.getNestedTypes = wrapAndDeletePuertsHook('__tgjsGetNestedTypes', disposeChecker);
puerts.evalScript = wrapAndDeletePuertsHook('__tgjsEvalScript', disposeChecker, function(script, debugPath) {
    return eval(script);
});

function wrapAndDeletePuertsHook(name, checker, polyfill) {
    const fn = global[name] || polyfill;
    delete global[name];

    return function() {
        checker.apply(this, arguments)
        return fn.apply(this, arguments);
    }
}