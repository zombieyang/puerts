/*
 * Tencent is pleased to support the open source community by making Puerts available.
 * Copyright (C) 2020 THL A29 Limited, a Tencent company.  All rights reserved.
 * Puerts is licensed under the BSD 3-Clause License, except for the third-party components listed in the file 'LICENSE' which may be subject to their corresponding license terms. 
 * This file is subject to the terms and conditions defined in file 'LICENSE', which is part of this source code package.
 */ 

var global = global || globalThis || (function () { return this; }());

let UnityEngine_Debug = puerts.loadType('UnityEngine.Debug');

if (UnityEngine_Debug) {
    const console_org = global.console;
    var console = {}
    
    function toString(args) {
        return Array.prototype.map.call(args, x => {
            try {
                return x+'';
            } catch (err) {
                return err;
            }
        }).join(',');
    }
    
    console.log = function() {
        if (console_org) console_org.log.apply(null, Array.prototype.slice.call(arguments));
<<<<<<< HEAD:unity/Assets/Puerts/Src/Resources/puerts/log.mjs
        UnityEngine_Debug.WriteLine(toString(arguments));
=======
        UnityEngine_Debug.Log(toString(arguments));
>>>>>>> zombie/master:unity/Assets/Puerts/Runtime/Resources/puerts/log.mjs
    }
    
    console.info = function() {
        if (console_org) console_org.info.apply(null, Array.prototype.slice.call(arguments));
<<<<<<< HEAD:unity/Assets/Puerts/Src/Resources/puerts/log.mjs
        UnityEngine_Debug.WriteLine(toString(arguments));
=======
        UnityEngine_Debug.Log(toString(arguments));
>>>>>>> zombie/master:unity/Assets/Puerts/Runtime/Resources/puerts/log.mjs
    }
    
    console.warn = function() {
        if (console_org) console_org.warn.apply(null, Array.prototype.slice.call(arguments));
<<<<<<< HEAD:unity/Assets/Puerts/Src/Resources/puerts/log.mjs
        UnityEngine_Debug.WriteLine(toString(arguments));
=======
        UnityEngine_Debug.LogWarning(toString(arguments));
>>>>>>> zombie/master:unity/Assets/Puerts/Runtime/Resources/puerts/log.mjs
    }
    
    console.error = function() {
        if (console_org) console_org.error.apply(null, Array.prototype.slice.call(arguments));
<<<<<<< HEAD:unity/Assets/Puerts/Src/Resources/puerts/log.mjs
        UnityEngine_Debug.WriteLine(toString(arguments));
=======
        UnityEngine_Debug.LogError(toString(arguments));
    }
    
    console.trace = function() {
        if (console_org) console_org.trace.apply(null, Array.prototype.slice.call(arguments));
        let stack = new Error().stack; // get js stack
        stack = stack.substring(stack.indexOf("\n")+1); // remove first line ("Error")
        stack = stack.replace(/^ {4}/gm, ""); // remove indentation
        UnityEngine_Debug.Log(toString(arguments) + "\n" + stack + "\n");
>>>>>>> zombie/master:unity/Assets/Puerts/Runtime/Resources/puerts/log.mjs
    }
    
    global.console = console;
    puerts.console = console;
}