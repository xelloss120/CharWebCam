using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// コマンドライン引数の取得
/// </summary>
public static class CommandLineArgs
{
    public static readonly string AudioInputDevice;
    public static readonly bool DisplayRawCameraImage;
    public static readonly bool HideTextDefault;

    static readonly IEnumerable<string> Args = Environment.GetCommandLineArgs().Skip(1);

    static CommandLineArgs()
    {
        AudioInputDevice = GetMicrophone();
        DisplayRawCameraImage = Args.Contains("--display-raw-camera-image");
        HideTextDefault = Args.Contains("--hide-text-default");
    }

    /// <summary>
    /// コマンドライン引数「--audio-input-device」の値を取得
    /// </summary>
    /// <returns>引数が存在しない場合はnull、値が空の場合は空文字列を返す</returns>
    static string GetMicrophone()
    {
        var arg = Args.FirstOrDefault(a => a.StartsWith("--audio-input-device"));
        if (arg == null)
        {
            return null;
        }

        var keyValuePair = arg.Split(new[] { "=" }, 2, StringSplitOptions.None);
        if (keyValuePair[0] != "--audio-input-device")
        {
            return null;
        }

        if (keyValuePair.Length == 1)
        {
            return "";
        }

        return keyValuePair[1];
    }
}
