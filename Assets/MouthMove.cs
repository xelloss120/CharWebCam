using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Intel.RealSense;
using Intel.RealSense.Utility;

public class MouthMove : MonoBehaviour
{
    public SkinnedMeshRenderer Mouth;

    Session Session;
    Smoother Smoother;
    Smoother1D SmoothMouth;

    public Text Text;

    void Start()
    {
        StartCoroutine("SelectMicrophone");
        Session = Session.CreateInstance();
        Smoother = Smoother.CreateInstance(Session);
        SmoothMouth = Smoother.Create1DWeighted(5);
    }

    /// <summary>
    /// 音声入力デバイスの一覧表示と選択
    /// </summary>
    /// <remarks>
    /// https://docs.unity3d.com/jp/540/Manual/Coroutines.html
    /// https://docs.unity3d.com/ja/540/ScriptReference/Microphone-devices.html
    /// https://docs.unity3d.com/ja/540/ScriptReference/Microphone.Start.html
    /// </remarks>
    IEnumerator SelectMicrophone()
    {
        // 一覧表示
        Text.text = "Device to move mouth.\n\n";
        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            Text.text += "[" + i + "]" + Microphone.devices[i] + "\n";
        }
        Text.text += "\nPlease select with number key.";

        // 選択待機
        while (true)
        {
            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                if (Input.GetKey(KeyCode.Alpha0 + i))
                {
                    // 初期化
                    AudioSource audio = GetComponent<AudioSource>();
                    audio.clip = Microphone.Start(Microphone.devices[i], true, 10, 44100);
                    Text.text = "";
                }
            }
            yield return null;
        }
    }

    void Update()
    {
        // 入力音量取得
        // 参考：https://docs.unity3d.com/jp/540/ScriptReference/AudioClip.GetData.html
        AudioSource audio = GetComponent<AudioSource>();
        if(audio != null && audio.clip != null)
        {
            float[] samples = new float[audio.clip.samples * audio.clip.channels];
            audio.clip.GetData(samples, 0);
            float vol = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                vol += Mathf.Abs(samples[i]);
                samples[i] = samples[i] * 0.5F;
            }
            audio.clip.SetData(samples, 0);

            // 口パク
            // 参考：https://docs.unity3d.com/ja/540/Manual/BlendShapes.html
            Mouth.SetBlendShapeWeight(6, SmoothMouth.SmoothValue(vol < 1 ? 0 : vol * 5));
        }
    }

    void OnDestroy()
    {
        SmoothMouth.Dispose();
        Smoother.Dispose();
        Session.Dispose();
    }
}