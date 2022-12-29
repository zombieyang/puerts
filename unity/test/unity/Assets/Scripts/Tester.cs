using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using System;
using NUnit.Framework;
using Puerts;

public class Tester : MonoBehaviour {

    public Text m_ContentText;
    public Button m_StartBtn;
    public Button m_StopBtn;

    public bool IsTesting = false;

    void Start() {

        string MockConsoleContent = "";
        IsTesting = true;
        
        JsEnv env = new JsEnv();
        string result = env.Eval<string>(@"
            const ZombieTest = puer.loadType(jsEnv.GetTypeByString('ZombieTest'));
            const ZombieTest2 = puer.loadType(jsEnv.GetTypeByString('ZombieTest2'));
            const zt = new ZombieTest;
            const zt2 = new ZombieTest2;

            let ret = '';
            let start = null;

            for (let i = 0; i < 100; i++) {
                zt2.Add();
            }
            start = Date.now();
            for (let i = 0; i < 1000000; i++) {
                zt2.Add();
            }
            ret += ('zt testcase 2: ' + (Date.now() - start));

            for (let i = 0; i < 100; i++) {
                zt.Add();
            }
            start = Date.now();
            for (let i = 0; i < 1000000; i++) {
                zt.Add();
            }
            ret += ('zt testcase 1: ' + (Date.now() - start)) + '\n';
        ");
        MockConsoleContent += $"ZombieTest2.Add(1) result:{result}";
        m_ContentText.text = MockConsoleContent;
        UnityEngine.Debug.Log(result);

        // StartCoroutine(
        //     RunTest(
        //         (string name) => {
        //             MockConsoleContent += $"Passed: TestCase {name}\n";
        //             UnityEngine.Debug.Log($"Passed: TestCase {name}\n");
        //             m_ContentText.text = MockConsoleContent;
        //         },
        //         (string name, Exception e) => {
        //             MockConsoleContent += $"Failed: TestCase {name} msg: {e.Message}\n";
        //             UnityEngine.Debug.LogError($"Failed: TestCase {name}\n");
        //             UnityEngine.Debug.LogError(e);
        //             m_ContentText.text = MockConsoleContent;
        //         }
        //     )
        // );
    }

    private IEnumerator RunTest(Action<string> OnSuccess, Action<string, Exception> OnFail)
    {
        UnityEngine.Debug.Log("Start RunTest");
        var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    // where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder)
                    from type in assembly.GetTypes()
                    where type.IsDefined(typeof(TestFixtureAttribute), false)
                    select type;

        foreach (var type in types)
        {
            var testInstance = System.Activator.CreateInstance(type);

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                foreach (var ca in method.GetCustomAttributes(false))
                {
                    yield return null;
                    if (IsTesting && ca.GetType() == typeof(TestAttribute)) 
                    {
                        try 
                        {
                            method.Invoke(testInstance, null);
                        } 
                        catch (Exception e) 
                        {
                            OnFail(method.Name, e);
                            continue;
                        }
                        OnSuccess(method.Name);
                    }
                }
            }            
        }
    }
}