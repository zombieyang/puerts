/*
* Tencent is pleased to support the open source community by making Puerts available.
* Copyright (C) 2020 THL A29 Limited, a Tencent company.  All rights reserved.
* Puerts is licensed under the BSD 3-Clause License, except for the third-party components listed in the file 'LICENSE' which may be subject to their corresponding license terms. 
* This file is subject to the terms and conditions defined in file 'LICENSE', which is part of this source code package.
*/

#if PUERTS_GENERAL
using System.IO;
#endif
using System.Text;

namespace Puerts 
{
    public abstract class ILoader 
    {
        public abstract bool FileExists(string filepath);
        public virtual string ReadFile(string filepath, out string debugpath) 
        {
            byte[] bytes = ReadByte(filepath, out debugpath);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
        protected internal abstract byte[] ReadByte(string filepath, out string debugpath);
    }

    public class DefaultLoader : ILoader 
    {
        private string root = "";

        public DefaultLoader()
        {
        }

        public DefaultLoader(string root)
        {
            this.root = root;
        }

        private string PathToUse(string filepath) 
        {
            if (filepath.EndsWith(".bytes")) 
            {
                return filepath.Substring(0, filepath.Length - 6);
            }
            // else if (filepath.EndsWith(".cjs") || filepath.EndsWith(".mjs"))
            // {
            //     return filepath.Substring(0, filepath.Length - 4);
            // }
            else 
            {
                return filepath;
            }
        }

        public override bool FileExists(string filepath) 
        {
#if PUERTS_GENERAL
            return File.Exists(Path.Combine(root, filepath));
#else
            string pathToUse = this.PathToUse(filepath);
            return UnityEngine.Resources.Load(pathToUse) != null;
#endif
        }

        protected internal override byte[] ReadByte(string filepath, out string debugpath) 
        {
#if PUERTS_GENERAL
            debugpath = Path.Combine(root, filepath);
            return File.ReadAllBytes(debugpath);
#else
            string pathToUse = this.PathToUse(filepath);
            UnityEngine.TextAsset file = (UnityEngine.TextAsset)UnityEngine.Resources.Load(pathToUse);
            debugpath = System.IO.Path.Combine(root, filepath);
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            debugpath = debugpath.Replace("/", "\\");
#endif
            return file == null ? null : file.bytes;
#endif
        }
    }
}
