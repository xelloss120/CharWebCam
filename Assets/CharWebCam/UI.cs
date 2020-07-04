using System;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VRM;
using VRMViewer;

public class UI : MonoBehaviour
{
    public Text Text;
    public Text DetectedValue;
    public RawImage RawImage;
    public VRMPreviewUI ModalWindowPrefab;

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
        string path = null;
        VRMImporterContext context = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(CommandLineArgs.VRM))
            {
                context = await Load(CommandLineArgs.VRM);
            }
            else
            {
                path = AskVRMPath();
                if (path != null)
                {
                    context = await Load(path);
                }
            }
        }
        catch (Exception exception)
        {
            context = null;
            DisplayLoadingError(path, exception);
        }

        if (context == null)
        {
            context = await Load(GetDefaultModelPath());
        }

        Setup(context);

        StartCoroutine("GetKeyEsc");
    }

    /// <summary>
    /// VRMファイルを読み込む
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    async Task<VRMImporterContext> Load(string path)
    {
        var context = new VRMImporterContext();
        try
        {
            context.ParseGlb(File.ReadAllBytes(path));
            await context.LoadAsyncTask();
            return context;
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }

    /// <summary>
    /// VRMファイル選択ダイアログを開く
    /// </summary>
    /// <returns></returns>
    string AskVRMPath()
    {
        var path = FileDialogForWindows.FileDialog("使用するアバターを選択してください", ".vrm");
        return string.IsNullOrEmpty(path) ? null : path;
    }

    /// <summary>
    /// 指定したパスからのVRMファイルの読み込みに失敗したことを表示
    /// </summary>
    /// <param name="path"></param>
    /// <param name="exception"></param>
    void DisplayLoadingError(string path, Exception exception)
    {
        Debug.LogError(exception);

        Text.text += "Failed to load the specified VRM file.\n"
            + $"Path: {path}\n"
            + $"Error message:\n{exception.Message}\n"
            + "\n";
    }

    /// <summary>
    /// VRMプレハブをシーン配置し、セットアップする
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    void Setup(VRMImporterContext context)
    {
        context.Root.transform.parent = null;
        context.Root.AddComponent<RS_VRM>().UI = this;
        context.ShowMeshes();
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
