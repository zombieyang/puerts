using NUnit.Framework;
using System;

namespace Puerts.UnitTest
{
    [TestFixture]
    public class EvalTest
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
            try 
            {
                jsEnv.ExecuteModule("whatever.mjs");
            } 
            catch(Exception e) 
            {
                Assert.True(e.Message.Contains("whatever.mjs"));
                jsEnv.Dispose();
                return;
            }
            Assert.True(false);
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
            try 
            {
                jsEnv.ExecuteModule("whatever.mjs");
            } 
            catch(Exception e) 
            {
                Assert.True(e.Message.Contains("export"));
                jsEnv.Dispose();
                return;
            }
            Assert.True(false);
        }
        [Test]
        public void ESModuleEvaluateError()
        {
            var loader = new TxtLoader();
            loader.AddMockFileContent("whatever.mjs", @"var obj = {}; obj.func();");
            var jsEnv = new JsEnv(loader);
            try 
            {
                jsEnv.ExecuteModule("whatever.mjs");
            } 
            catch(Exception e) 
            {
                Assert.True(e.Message.Contains("not a function"));
                jsEnv.Dispose();
                return;
            }
            Assert.True(false);
        }
        [Test]
        public void ESModuleImportNotFound()
        {
            var loader = new TxtLoader();
            loader.AddMockFileContent("entry.mjs", @"import 'whatever.mjs'");
            var jsEnv = new JsEnv(loader);
            try 
            {
                jsEnv.ExecuteModule("entry.mjs");
            } 
            catch(Exception e) 
            {
                Assert.True(e.Message.Contains("whatever.mjs"));
                jsEnv.Dispose();
                return;
            }
            Assert.True(false);
        }
        [Test]
        public void ESModuleImportCompileError()
        {
            var loader = new TxtLoader();
            loader.AddMockFileContent("whatever.mjs", @"export delete;");
            loader.AddMockFileContent("entry.mjs", @"import 'whatever.mjs'");
            var jsEnv = new JsEnv(loader);
            try 
            {
                jsEnv.ExecuteModule("entry.mjs");
            } 
            catch(Exception e) 
            {
                Assert.True(e.Message.Contains("export"));
                jsEnv.Dispose();
                return;
            }
            Assert.True(false);
        }
        [Test]
        public void ESModuleImportEvaluateError()
        {
            var loader = new TxtLoader();
            loader.AddMockFileContent("whatever.mjs", @"var obj = {}; obj.func();");
            loader.AddMockFileContent("entry.mjs", @"import 'whatever.mjs'");
            var jsEnv = new JsEnv(loader);
            try 
            {
                jsEnv.ExecuteModule("entry.mjs");
            } 
            catch(Exception e) 
            {
                Assert.True(e.Message.Contains("not a function"));
                jsEnv.Dispose();
                return;
            }
            Assert.True(false);
        }
        [Test]
        public void ESModuleExecuteCJS()
        {
            var loader = new TxtLoader();
            loader.AddMockFileContent("whatever.cjs", @"
                module.exports = 'hello world';
            ");
            var jsEnv = new JsEnv(loader);
            string str = jsEnv.ExecuteModule<string>("whatever.cjs", "default");

            Assert.True(str == "hello world");

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