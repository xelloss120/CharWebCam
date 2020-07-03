using System;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VRM;

public class UI : MonoBehaviour
{
    public Text Text;
    public Text DetectedValue;
    public RawImage RawImage;

    /// <summary>
    /// 同梱するVRMファイルの絶対パスを取得する
    /// </summary>
    /// <returns></returns>
    public static string GetDefaultModelPath()
    {
        return Path.Combine(Path.GetDirectoryName(Application.dataPath), "DefaultModel", "unitychan.vrm");
    }

    async void Start()
    {
        GameObject avatar = null;
        if (!string.IsNullOrWhiteSpace(CommandLineArgs.VRM))
        {
            try
            {
                avatar = await Load(CommandLineArgs.VRM);
            }
            catch (Exception exception)
            {
                Text.text += "Failed to load the specified VRM file.\n"
                    + $"Path: {CommandLineArgs.VRM}\n"
                    + $"Error message:\n{exception.Message}\n"
                    + "\n";
            }
        }

        if (avatar == null)
        {
            avatar = await Load(GetDefaultModelPath());
        }

        avatar.AddComponent<RS_VRM>().UI = this;
        StartCoroutine("GetKeyEsc");
    }

    /// <summary>
    /// VRMファイルを読み込み、シーンへ配置する
    /// </summary>
    /// <param name="path">VRMファイルの絶対パス</param>
    /// <returns></returns>
    async Task<GameObject> Load(string path)
    {
        var context = new VRMImporterContext();
        try
        {
            context.ParseGlb(File.ReadAllBytes(path));
            await context.LoadAsyncTask();

            var avatar = context.Root;
            avatar.transform.parent = null;

            context.ShowMeshes();

            return avatar;
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }

    /// <summary>
    /// ESCキーが押されたら、キャンバスを非表示に
    /// </summary>
    /// <returns></returns>
    IEnumerator GetKeyEsc()
    {
        while (!Input.GetKey(KeyCode.Escape)) yield return null;
        gameObject.SetActive(false);
    }
}
