using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Text Text;
    public Text DetectedValue;
    public RawImage RawImage;

    void Start()
    {
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
