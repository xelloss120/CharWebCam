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

    public Text Text;
    public Text DetectedValue;
    public RawImage RawImage;

    async void Start()
    {
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
