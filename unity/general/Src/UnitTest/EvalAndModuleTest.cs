/*
* Tencent is pleased to support the open source community by making Puerts available.
* Copyright (C) 2020 THL A29 Limited, a Tencent company.  All rights reserved.
* Puerts is licensed under the BSD 3-Clause License, except for the third-party components listed in the file 'LICENSE' which may be subject to their corresponding license terms. 
* This file is subject to the terms and conditions defined in file 'LICENSE', which is part of this source code package.
*/

using NUnit.Framework;
using System;

namespace Puerts.UnitTest
{
    [TestFixture]
    public class EvalAndModuleTest
    {
        [Test]
        public void EvalError()
        {
            var jsEnv = new JsEnv(new TxtLoader());
            Assert.Catch(() =>
            {
                jsEnv.Eval(@"
                    var obj = {}; obj.func();
                ");
            });
            jsEnv.Dispose();
        }

        [Test]
        public void ESModuleNotFound()
        {
            var jsEnv = new JsEnv(new TxtLoader());
            Assert.Catch(() =>
            {
                jsEnv.ExecuteModule("whatever.mjs");
            });
            jsEnv.Dispose();
        }
        [Test]
        public void ESModuleLoadFailed()
        {
            var loader = new TxtLoader();
            loader.AddMockFileContent("whatever.mjs", null);
            var jsEnv = new JsEnv(loader);
            Assert.Catch(() =>
            {
                jsEnv.ExecuteModule("whatever.mjs");
            });
            jsEnv.Dispose();
        }
        [Test]
        public void ESModuleCompileError()
        {
            var loader = new TxtLoader();
            loader.AddMockFileContent("whatever.mjs", @"export delete;");
            var jsEnv = new JsEnv(loader);
            Assert.Catch(() =>
            {
                jsEnv.ExecuteModule("whatever.mjs");
            });
            jsEnv.Dispose();
        }
        [Test]
        public void ESModuleEvaluateError()
        {
            var loader = new TxtLoader();
            loader.AddMockFileContent("whatever.mjs", @"var obj = {}; obj.func();");
            var jsEnv = new JsEnv(loader);
            Assert.Catch(() =>
            {
                jsEnv.ExecuteModule("whatever.mjs");
            });
            jsEnv.Dispose();
        }
        [Test]
        public void ESModuleEvaluateRelativeFile()
        {
            var loader = new TxtLoader();
            loader.AddMockFileContent("business/whatever.mjs", @"
                import lib from '../library/lib.mjs';

                const add3Num = function(a, b, c) {
                    return lib.add(lib.add(a, b), c);
                }

                export { add3Num };
            ");
            loader.AddMockFileContent("library/lib.mjs", @"
                export default { add(a, b) { return a + b } }
            ");
            var jsEnv = new JsEnv(loader);
            jsEnv.UsingFunc<int, int, int, int>();
            Func<int, int, int, int> func = jsEnv.ExecuteModule<Func<int, int, int, int>>("business/whatever.mjs", "add3Num");

            Assert.True(func(1, 2, 3) == 6);

            jsEnv.Dispose();
        }
        [Test]
        public void ESModuleImportCSharp()
        {
            var loader = new TxtLoader();
            loader.AddMockFileContent("whatever.mjs", @"
                import csharp from 'csharp';
                const func = function() { return csharp.System.String.Join(' ', 'hello', 'world') }
                export { func };
            ");
            var jsEnv = new JsEnv(loader);
            Func<string> func = jsEnv.ExecuteModule<Func<string>>("whatever.mjs", "func");

            Assert.True(func() == "hello world");

            jsEnv.Dispose();
        }
        [Test]
        public void ESModuleImportCSharpNamespace()
        {
            var loader = new TxtLoader();
            loader.AddMockFileContent("whatever.mjs", @"
                import csharp from 'csharp';
                const func = function() { return csharp.System.String.Join(' ', 'hello', 'world') }
                export { func };
            ");
            var jsEnv = new JsEnv(loader);
            var ns = jsEnv.ExecuteModule<JSObject>("whatever.mjs");

            Assert.True(ns != null);
            Assert.True(ns.GetType() == typeof(JSObject));

            jsEnv.Dispose();
        }
    }
}

