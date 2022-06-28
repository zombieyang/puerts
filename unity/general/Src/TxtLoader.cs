using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using Puerts;

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

    private string root = PathToBinDir("../../Assets/Puerts/Runtime/Resources");
    private string editorRoot = PathToBinDir("../../Assets/Puerts/Editor/Resources");

    public override bool FileExists(string filepath)
    {
        return mockFileContent.ContainsKey(filepath) ||
            File.Exists(Path.Combine(root, filepath)) ||
            File.Exists(Path.Combine(editorRoot, filepath));
    }

    protected override byte[] ReadByte(string filepath, out int length, out string debugpath)
    {
        debugpath = Path.Combine(root, filepath);
        if (File.Exists(Path.Combine(editorRoot, filepath)))
        {
            debugpath = Path.Combine(editorRoot, filepath);
        }


        string mockContent;
        if (mockFileContent.TryGetValue(filepath, out mockContent))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(mockContent);
            length = bytes.Length;
            return bytes;
        }

        using (StreamReader reader = new StreamReader(debugpath))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(reader.ReadToEnd());
            length = bytes.Length;
            return bytes;
        }
    }

    private Dictionary<string, string> mockFileContent = new Dictionary<string, string>();
    public void AddMockFileContent(string fileName, string content)
    {
        mockFileContent.Add(fileName, content);
    }

}