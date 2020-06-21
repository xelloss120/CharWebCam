using System.IO;
using UnityEngine;
using UnityEditor;

internal class Building
{
    [InitializeOnLoadMethod]
    static void SetRuntimePath()
    {
        RSPostBuildProcessor.SpecifiedRuntimePath = Path.Combine(Application.dataPath, "RSSDK");
    }
}
