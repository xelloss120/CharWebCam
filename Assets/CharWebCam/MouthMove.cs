using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Intel.RealSense;
using Intel.RealSense.Utility;

public class MouthMove : MonoBehaviour
{
    // 音声入力デバイス選択表示
    public Text Text;
    string DeviceName;

    // 平滑化
    protected Smoother1D SmoothMouth;
    Smoother Smoother;
    SenseManager SenseManager;

    // 不具合対策
    readonly TimeSpan PositionStopLimit = TimeSpan.FromSeconds(1);
    readonly TimeSpan PositionMonitoringInterval = TimeSpan.FromSeconds(0.05f);
    DateTimeOffset? PositionNotMovedStart;
    int PreviousPosition;

    /// <summary>
    /// 音声入力デバイス選択待機
    /// </summary>
    protected void Init()
    {
        // コマンドライン引数
        DeviceName = CommandLineArgs.AudioInputDevice;
        if (DeviceName != null)
        {
            if (DeviceName == "" || Microphone.devices.Contains(DeviceName))
            {
                StartRecording();
            }
            else
            {
                Text.text += $"Device named “{DeviceName}” do not exists.\n\n";
                DeviceName = null;
            }
        }
        else if (CommandLineArgs.HideTextDefault)
        {
            return;
        }

        if (DeviceName == null)
        {
            StartCoroutine("SelectMicrophone");
        }

        // 平滑化初期化
        // 参考：https://software.intel.com/sites/landingpage/realsense/camera-sdk/v2016r3/documentation/html/index.html?doc_utils_the_smoother_utility.html
        SenseManager = SenseManager.CreateInstance();
        Smoother = Smoother.CreateInstance(SenseManager.Session);
        SmoothMouth = Smoother.Create1DWeighted(5);

        MonitorMicrophone();
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
        Text.text += "Device to move mouth.\n\n";
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
                    // 録音開始
                    DeviceName = Microphone.devices[i];
                    StartRecording();
                    Text.text = "";
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// 録音開始
    /// </summary>
    void StartRecording()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = Microphone.Start(DeviceName, true, 10, 44100);
    }

    /// <summary>
    /// 音量取得
    /// </summary>
    /// <returns>音量</returns>
    /// <remarks>
    /// https://docs.unity3d.com/jp/540/ScriptReference/AudioClip.GetData.html
    /// </remarks>
    protected float GetVolume()
    {
        // 録音が開始されていなければ中断
        AudioSource audio = GetComponent<AudioSource>();
        if (audio.clip == null)
        {
            return 0;
        }

        // 入力音量取得
        float[] samples = new float[audio.clip.samples * audio.clip.channels];
        audio.clip.GetData(samples, 0);
        float vol = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            vol += Mathf.Abs(samples[i]);
            samples[i] = samples[i] * 0.5F;
        }
        audio.clip.SetData(samples, 0);

        return vol;
    }

    /// <summary>
    /// デバイスが変化するとマイク入力が取れなくなるUnityの不具合を回避
    /// </summary>
    async void MonitorMicrophone()
    {
        while (true)
        {
            var position = Microphone.GetPosition(DeviceName);

            if (position == PreviousPosition)
            {
                if (PositionNotMovedStart == null)
                {
                    PositionNotMovedStart = DateTimeOffset.Now;
                }
                else if (DateTimeOffset.Now - PositionNotMovedStart > PositionStopLimit)
                {
                    PositionNotMovedStart = null;
                    StartRecording();
                }
            }
            else
            {
                PositionNotMovedStart = null;
                PreviousPosition = position;
            }

            await Task.Delay(PositionMonitoringInterval);
        }
    }
}