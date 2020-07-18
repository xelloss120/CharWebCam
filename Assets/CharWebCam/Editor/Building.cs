using System.Linq;
using System.Collections.Generic;
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
        AddReadmeFile(directoryPath);
        AddRealSenseSDKLicenseFile(directoryPath);
        RemoveUnnecessaryFiles(directoryPath);
        AddDefaultModel(directoryPath);
        AddVirtualCameraDevice(directoryPath);
    }

    /// <summary>
    /// ビルド先へREADMEページへのショートカットを追加
    /// </summary>
    static void AddReadmeFile(string directoryPath)
    {
        File.WriteAllLines(Path.Combine(directoryPath, "README.url"), new[]
        {
            "[InternetShortcut]",
            "URL=https://github.com/xelloss120/CharWebCam#readme",
        });
    }

    /// <summary>
    /// ビルド先へRealSense SDKのライセンスファイルを追加
    /// </summary>
    static void AddRealSenseSDKLicenseFile(string directoryPath)
    {
        var filePath = Path.Combine(Application.dataPath, "RSSDK/Plugins/runtime/Intel RealSense SDK RT EULA.rtf");
        File.Copy(
            filePath,
            Path.Combine(
                Path.GetDirectoryName(
                    Directory.EnumerateDirectories(directoryPath, "runtime", SearchOption.AllDirectories).First()
                ),
                Path.GetFileName(filePath)
            )
        );
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

    /// <summary>
    /// VRMファイルを同梱する
    /// </summary>
    static void AddDefaultModel(string directoryPath)
    {
        CopyDirectoryRecursively(Path.GetDirectoryName(RuntimeVRMLoader.GetDefaultModelPath()), directoryPath);
    }

    /// <summary>
    /// 仮想カメラデバイスのdllを同梱する
    /// </summary>
    static void AddVirtualCameraDevice(string directoryPath)
    {
        CopyDirectoryRecursively(
            Path.Combine(Path.GetDirectoryName(Application.dataPath), "UnityCaptureFilter"),
            directoryPath
        );
    }

    /// <summary>
    /// ディレクトリを再帰的にコピーする
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destinationParentPath"></param>
    static void CopyDirectoryRecursively(string sourcePath, string destinationParentPath)
    {
        var source = new DirectoryInfo(sourcePath);
        CopyDirectoryRecursively(source, Path.Combine(destinationParentPath, source.Name));
    }

    /// <summary>
    /// ディレクトリを再帰的にコピーする
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    static void CopyDirectoryRecursively(DirectoryInfo source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (var sourceSubDirectory in source.GetDirectories())
        {
            CopyDirectoryRecursively(sourceSubDirectory, Path.Combine(destination, sourceSubDirectory.Name));
        }

        foreach (var sourceFile in source.GetFiles())
        {
            sourceFile.CopyTo(Path.Combine(destination, sourceFile.Name), overwrite: true);
        }
    }
}
