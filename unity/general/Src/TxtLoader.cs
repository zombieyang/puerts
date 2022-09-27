using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Puerts;
using System.Linq;

public class TxtLoader : ILoader
{
    public static string PathToBinDir(string appendix)
    {
        return Path.Combine(
            System.Text.RegularExpressions.Regex.Replace(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase), "^file:(\\\\)?", ""
            ),
            appendix
        );
    }
    private List<string> ResourcesPaths = new List<string> {
        PathToBinDir("../../../Assets/Puerts/Runtime/Resources"),
        PathToBinDir("../../../Assets/Puerts/Editor/Resources"),
        PathToBinDir("../../../packages/commonjs/upm/Runtime/Resources"),
    };

    public bool FileExists(string filepath)
    {
        return mockFileContent.ContainsKey(filepath) || ResourcesPaths
                   .Where((root) => File.Exists(Path.Combine(root, filepath)))
                   .ToArray().Length > 0;
    }

    public string ReadFile(string filepath, out string debugpath)
    {
        string mockContent;
        if (mockFileContent.TryGetValue(filepath, out mockContent))
        {
            debugpath = filepath;
            return mockContent;
        }

        string rootpath = ResourcesPaths.Where((root) => File.Exists(Path.Combine(root, filepath)))
            .ToArray()[0];
        debugpath = Path.Combine(rootpath, filepath);
        

        using (StreamReader reader = new StreamReader(debugpath))
        {
            return reader.ReadToEnd();
        }
    }

    private Dictionary<string, string> mockFileContent = new Dictionary<string, string>();
    public void AddMockFileContent(string fileName, string content)
    {
        mockFileContent.Add(fileName, content);
    }
}