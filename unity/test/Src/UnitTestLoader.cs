using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Puerts;

namespace Puerts.UnitTest 
{
    public class UnitTestLoader2: IResolvableLoader, ILoader, IModuleChecker
    {

        public UnitTestLoader2() 
        {

        }

        public bool IsESM(string filepath)
        {
            return !filepath.EndsWith(".cjs");
        }

        [UnityEngine.Scripting.Preserve]
        public string ResolveAndFind(string specifier, string referrer, out bool exists)
        {
            if (PathHelper.IsRelative(specifier))
            {
                specifier = PathHelper.normalize(PathHelper.Dirname(referrer) + "/" + specifier);
            }

            exists = true;

            string path = UnityEngine.Application.streamingAssetsPath + "/" + specifier;
            if (System.IO.File.Exists(path)) 
            {
                return path;
            }
            else if (mockFileContent.ContainsKey(specifier)) 
            {
                return specifier;
            } 
            else if (mockFileContent.ContainsKey(specifier + "/index.js")) 
            {
                return specifier + "/index.js";
            } 
            else if (UnityEngine.Resources.Load(FixSpecifier(specifier)) != null) 
            {
                return specifier;
            }
            else if (UnityEngine.Resources.Load(FixSpecifier(specifier + "/index.js")) != null) 
            {
                return specifier + "/index.js";
            }

            exists = false;
            return specifier;
        }

        [UnityEngine.Scripting.Preserve]
        public string Resolve(string specifier, string referrer)
        {
            return ResolveAndFind(specifier, referrer, out _);
        }

        [UnityEngine.Scripting.Preserve]
        public bool FileExists(string specifier)
        {
            bool exists;
            ResolveAndFind(specifier, ".", out exists);
            return exists;
        }

        [UnityEngine.Scripting.Preserve]
        public string ReadFile(string specifier, out string debugpath)
        {
            debugpath = "";
            if (specifier != null) {
                if (specifier.StartsWith(UnityEngine.Application.streamingAssetsPath) || File.Exists(UnityEngine.Application.streamingAssetsPath + "/" + specifier)) {
                    return System.IO.File.ReadAllText(UnityEngine.Application.streamingAssetsPath + "/" + specifier);

                } else if (mockFileContent.ContainsKey(specifier)) {
                    return mockFileContent[specifier];

                } else if (UnityEngine.Resources.Load(FixSpecifier(specifier)) != null) {
                    return UnityEngine.Resources.Load<UnityEngine.TextAsset>(FixSpecifier(specifier)).text;

                }
            }
            return "";
        }
        private Dictionary<string, string> mockFileContent = new Dictionary<string, string>();
        private string FixSpecifier(string specifier)
        {
            return 
            // .cjs/.mjs asset is only supported in unity2018+
    #if UNITY_2018_1_OR_NEWER
            specifier.EndsWith(".cjs") || specifier.EndsWith(".mjs")  ? 
                specifier.Substring(0, specifier.Length - 4) : 
    #endif
                specifier;
        }
        [UnityEngine.Scripting.Preserve]
        public void AddMockFileContent(string fileName, string content)
        {
            mockFileContent[fileName] = content;
        }
    }
    public class UnitTestLoader : ILoader
    {
        private string FixSpecifier(string specifier)
        {
            return 
            // .cjs/.mjs asset is only supported in unity2018+
    #if UNITY_2018_1_OR_NEWER
            specifier.EndsWith(".cjs") || specifier.EndsWith(".mjs")  ? 
                specifier.Substring(0, specifier.Length - 4) : 
    #endif
                specifier;
        }

        [UnityEngine.Scripting.Preserve]
        public bool FileExists(string specifier)
        {
            string path = UnityEngine.Application.streamingAssetsPath + "/" + specifier;
            if (System.IO.File.Exists(path))
            {
                return true;
            }
            else if (mockFileContent.ContainsKey(specifier)) 
            {
                return true;
            } 
            else if (UnityEngine.Resources.Load(FixSpecifier(specifier)) != null) 
            {
                return true;
            }
            return false;
        }

        [UnityEngine.Scripting.Preserve]
        public string ReadFile(string specifier, out string debugpath)
        {
            debugpath = "";
            if (specifier != null) {
                if (specifier.StartsWith(UnityEngine.Application.streamingAssetsPath) || File.Exists(UnityEngine.Application.streamingAssetsPath + "/" + specifier)) {
                    return System.IO.File.ReadAllText(UnityEngine.Application.streamingAssetsPath + "/" + specifier);
                } else if (mockFileContent.ContainsKey(specifier)) {
                    return mockFileContent[specifier];
                } else if (UnityEngine.Resources.Load(FixSpecifier(specifier)) != null) {
                    return UnityEngine.Resources.Load<UnityEngine.TextAsset>(FixSpecifier(specifier)).text;
                }
            }
            return "";
        }
        
        private Dictionary<string, string> mockFileContent = new Dictionary<string, string>();
        [UnityEngine.Scripting.Preserve]
        public void AddMockFileContent(string fileName, string content)
        {
            mockFileContent[fileName] = content;
        }
    }
}