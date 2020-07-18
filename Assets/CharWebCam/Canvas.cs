using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Canvas : MonoBehaviour
{
    [Serializable]
    class SerializableStrings
    {
        public IEnumerable<string> Strings;
    }

    public static Canvas Instance;

    public Text Text;
    public Image TextBackround;
    public Text DetectedValue;
    public RawImage RawImage;

    /// <summary>
    /// メッセージを表示
    /// </summary>
    /// <remarks>
    /// 「--hide-text-default」を無効化し、すでに非表示になっていれば再表示する
    /// </remarks>
    /// <param name="message">表示するメッセージ。前後に改行は不要</param>
    public static void DisplayMessage(string message)
    {
        CommandLineArgs.HideTextDefault = false;
        Instance.Text.text += message + "\n\n";
        Instance.TextBackround.gameObject.SetActive(true);
        if (!Instance.gameObject.activeSelf)
        {
            Instance.DetectedValue.gameObject.SetActive(false);
            Instance.RawImage.gameObject.SetActive(false);
            Instance.gameObject.SetActive(true);
            Instance.StartCoroutine("GetKeyEsc");
        }
    }

    /// <summary>
    /// メッセージとメッセージの背景をクリア
    /// </summary>
    public static void ClearMessage()
    {
        Instance.Text.text = "";
        Instance.TextBackround.gameObject.SetActive(false);
    }

    async void Start()
    {
        Instance = this;

        var avatar = await GetComponent<RuntimeVRMLoader>().Load();
        if (avatar == null)
        {
            return;
        }
        avatar.AddComponent<RS_VRM>().Canvas = this;

        StartCoroutine("GetKeyEsc");
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
