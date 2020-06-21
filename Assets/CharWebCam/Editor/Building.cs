using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

internal class Building
{
    [InitializeOnLoadMethod]
    static void SetRuntimePath()
    {
        RSPostBuildProcessor.SpecifiedRuntimePath = Path.Combine(Application.dataPath, "RSSDK");
    }

    [PostProcessBuild(2)]
    static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        var directoryPath = Path.GetDirectoryName(pathToBuiltProject);
        RemoveUnnecessaryFiles(directoryPath);
    }

    /// <summary>
    /// ビルド先の不要なファイルを削除
    /// </summary>
    static void RemoveUnnecessaryFiles(string directoryPath)
    {
        File.Delete(Path.Combine(directoryPath, "UnityCrashHandler64.exe"));

        foreach (var group in Directory.GetFiles(directoryPath, "*.dll", SearchOption.AllDirectories)
            .GroupBy(path => Path.GetFileName(path)))
        {
            if (group.Count() == 1)
            {
                continue;
            }

            var (shortPath, longPath) = group.ElementAt(0).Length < group.ElementAt(1).Length
                ? (group.ElementAt(0), group.ElementAt(1))
                : (group.ElementAt(1), group.ElementAt(0));
            File.Delete(group.Key.EndsWith("_c.dll") ? longPath : shortPath);
        }

        foreach (var path in Directory.GetFiles(directoryPath, "*.meta", SearchOption.AllDirectories))
        {
            File.Delete(path);
        }
    }
}
