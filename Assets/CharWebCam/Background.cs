using System;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour
{
    Image Image;
    Vector2 PreviousScreenSize;

    void Start()
    {
        if (string.IsNullOrEmpty(CommandLineArgs.Background))
        {
            return;
        }

        if (IsHexColorCode(CommandLineArgs.Background))
        {
            ColorUtility.TryParseHtmlString(CommandLineArgs.Background, out var color);
            Camera.main.backgroundColor = color;
            return;
        }

        Image = GetComponentInChildren<Image>(true);
        try
        {
            Image.sprite = LoadSprite(CommandLineArgs.Background);
            Image.gameObject.SetActive(true);
        }
        catch (Exception e)
        {
            Canvas.DisplayMessage("Background Loading Error\n" + e.Message);
        }
        FitImage();
    }

    void Update()
    {
        var screenSize = new Vector2(Screen.width, Screen.height);
        if (PreviousScreenSize != screenSize)
        {
            PreviousScreenSize = screenSize;
            FitImage();
        }
    }

    /// <summary>
    /// アスペクト比を維持しつつ画面を覆うようなサイズへ、<see cref="Image"/> を拡縮
    /// </summary>
    void FitImage()
    {
        if (Image == null || Image.sprite == null)
        {
            return;
        }

        var windowRect = GetComponent<RectTransform>().rect;
        var spriteRect = Image.sprite.rect;
        var magnification = Math.Max(windowRect.width / spriteRect.width, windowRect.height / spriteRect.height);

        Image.GetComponent<RectTransform>().sizeDelta
            = new Vector2(spriteRect.width * magnification, spriteRect.height * magnification);
    }

    /// <summary>
    /// 指定された文字列が「#000000」のような値か否か
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    bool IsHexColorCode(string color)
    {
        return Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$");
    }

    /// <summary>
    /// 画像をスプライトとして読み込む
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Sprite LoadSprite(string path)
    {
        var texture = new Texture2D(2, 2);
        ImageConversion.LoadImage(texture, File.ReadAllBytes(path));
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
