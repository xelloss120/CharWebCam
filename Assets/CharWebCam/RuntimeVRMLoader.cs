using System;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VRM;
using VRMViewer;
using VRMLoader;

public class RuntimeVRMLoader : MonoBehaviour
{
    [Serializable]
    class SerializableStrings
    {
        public string[] Strings = new string[0];
    }

    public VRMPreviewUI ModalWindowPrefab;

    /// <summary>
    /// 同梱するVRMファイルの絶対パスを取得する
    /// </summary>
    /// <returns></returns>
    public static string GetDefaultModelPath()
    {
        return Path.Combine(Path.GetDirectoryName(Application.dataPath), "DefaultModel", "unitychan.vrm");
    }

    /// <summary>
    /// VRMファイルのパスを取得して読み込みまで行う
    /// </summary>
    /// <returns></returns>
    async public Task<GameObject> Load()
    {
        string path = !string.IsNullOrWhiteSpace(CommandLineArgs.VRM) ? CommandLineArgs.VRM : AskVRMPath();

        VRMImporterContext context = null;
        if (path != null)
        {
            try
            {
                context = await Load(path);
            }
            catch (Exception exception)
            {
                DisplayLoadingError(path, exception);
            }
        }

        if (context == null)
        {
            path = GetDefaultModelPath();
            context = await Load(path);
        }

        context.Root.transform.parent = null;
        context.ShowMeshes();
        return context.Root;
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
        }
        catch
        {
            context.Dispose();
            throw;
        }

        if (await AskLoadingVRM(path, context.ReadMeta(true)))
        {
            return context;
        }
        else
        {
            context.Dispose();
            return null;
        }
    }

    /// <summary>
    /// 初めて開くVRMファイルの場合、VRMのメタ情報の表示して読み込むか否かを選択するダイアログを開く
    /// </summary>
    /// <param name="path"></param>
    /// <param name="meta"></param>
    /// <returns></returns>
    Task<bool> AskLoadingVRM(string path, VRMMetaObject meta)
    {
        // パスの正規化
        path = Path.GetFullPath(path);

        // メタ情報の確認状態を取得
        var json = PlayerPrefs.GetString("paths-accepted-before");
        var strings = !string.IsNullOrEmpty(json)
            ? JsonUtility.FromJson<SerializableStrings>(json)
            : new SerializableStrings();
        if (strings.Strings.Contains(path))
        {
            return Task.FromResult(true);
        }

        // ファイル読み込みモーダルウィンドウの呼び出し
        var modalObject = Instantiate(ModalWindowPrefab.gameObject, transform);

        // 言語設定を取得・反映する
        var modalLocale = modalObject.GetComponentInChildren<VRMPreviewLocale>();
        modalLocale.SetLocale(Application.systemLanguage == SystemLanguage.Japanese ? "ja" : "en");

        // meta情報の反映
        var modalUI = modalObject.GetComponentInChildren<VRMPreviewUI>();
        modalUI.setMeta(meta);

        // ファイルを開くことの許可
        modalUI.setLoadable(true);

        // ロードイベントの追加
        var source = new TaskCompletionSource<bool>();
        modalUI.m_ok.onClick.AddListener(() =>
        {
            strings.Strings = strings.Strings.Concat(new[] { path }).ToArray();
            PlayerPrefs.SetString("paths-accepted-before", JsonUtility.ToJson(strings));
            source.TrySetResult(true);
        });
        modalUI.m_cancel.onClick.AddListener(() => source.TrySetResult(false));
        return source.Task;
    }

    /// <summary>
    /// 利用条件にすでに同意したファイルか
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    bool AcceptedBefore(string path)
    {
        var json = PlayerPrefs.GetString("paths-accepted-before");
        if (json == null)
        {
            return false;
        }

        return JsonUtility.FromJson<SerializableStrings>(json).Strings.Contains(Path.GetFullPath(path));
    }

    /// <summary>
    /// 指定したパスからのVRMファイルの読み込みに失敗したことを表示
    /// </summary>
    /// <param name="path"></param>
    /// <param name="exception"></param>
    void DisplayLoadingError(string path, Exception exception)
    {
        Debug.LogError(exception);
        Canvas.DisplayMessage("Failed to load the specified VRM file.\n"
            + $"Path: {path}\n"
            + $"Error message:\n{exception.Message}");
    }
}
