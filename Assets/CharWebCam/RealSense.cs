using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Intel.RealSense;
using Intel.RealSense.Face;
using Intel.RealSense.Utility;

public class RealSense : MonoBehaviour
{
    // 例外表示
    public Text Text;

    // 検出値
    protected ImageInfo Resolution;
    protected RectI32 FaceRect;
    protected LandmarkPoint[] Landmark;
    protected Dictionary<FaceExpression, FaceExpressionResult> FaceExp;

    // 平滑化
    protected Smoother3D SmoothBody;
    protected Smoother3D SmoothHead;
    protected Smoother2D SmoothEyes;
    protected Smoother1D SmoothEyesClose;
    Smoother Smoother;

    // RealSense
    SenseManager SenseManager;
    FaceModule FaceModule;
    FaceData FaceData;
    FaceConfiguration FaceConfig;

    protected void Init()
    {
        try
        {
            // RealSense初期化
            // 参考：https://software.intel.com/sites/landingpage/realsense/camera-sdk/v2016r3/documentation/html/index.html?doc_face_general_procedure.html
            SenseManager = SenseManager.CreateInstance();

            FaceModule = FaceModule.Activate(SenseManager);
            FaceModule.FrameProcessed += FaceModule_FrameProcessed;
            FaceData = FaceModule.CreateOutput();

            FaceConfig = FaceModule.CreateActiveConfiguration();
            FaceConfig.TrackingMode = TrackingModeType.FACE_MODE_COLOR;
            FaceConfig.Expressions.Properties.Enabled = true;
            FaceConfig.ApplyChanges();

            SenseManager.Init();
            SenseManager.StreamFrames(false);

            // 解像度取得
            StreamProfileSet profile;
            SenseManager.CaptureManager.Device.QueryStreamProfileSet(out profile);
            Resolution = profile.color.imageInfo;

            // 平滑化初期化
            // 参考：https://software.intel.com/sites/landingpage/realsense/camera-sdk/v2016r3/documentation/html/index.html?doc_utils_the_smoother_utility.html
            Smoother = Smoother.CreateInstance(SenseManager.Session);

            SmoothBody = Smoother.Create3DWeighted(20);
            SmoothHead = Smoother.Create3DWeighted(20);
            SmoothEyes = Smoother.Create2DWeighted(5);
            SmoothEyesClose = Smoother.Create1DWeighted(5);
        }
        catch (Exception e)
        {
            Text.text = "RealSense Error\n";
            Text.text += e.Message;
        }
    }

    /// <summary>
    /// 検出値を取得
    /// </summary>
    void FaceModule_FrameProcessed(object sender, FrameProcessedEventArgs args)
    {
        FaceData.Update();
        var face = FaceData.QueryFaceByIndex(0);
        if (face != null)
        {
            if (face.Detection != null)
            {
                FaceRect = face.Detection.BoundingRect;
            }

            if (face.Landmarks != null && face.Landmarks.Points != null)
            {
                Landmark = face.Landmarks.Points;
            }

            if (face.Expressions != null && face.Expressions.ExpressionResults != null)
            {
                FaceExp = face.Expressions.ExpressionResults;
            }
        }
    }

    /// <summary>
    /// 3点の中間比を求める
    /// </summary>
    /// <param name="v1">端1</param>
    /// <param name="center">中点</param>
    /// <param name="v2">端2</param>
    /// <returns>中点比</returns>
    protected float GetCenterRatio(Vector2 v1, Vector2 center, Vector2 v2)
    {
        return (Vector2.Distance(v1, center) - Vector2.Distance(v2, center)) / Vector2.Distance(v1, v2);
    }

    /// <summary>
    /// 2点感の角度を求める
    /// http://qiita.com/2dgames_jp/items/60274efb7b90fa6f986a
    /// https://gist.github.com/mizutanikirin/e9a71ef994ebb5f0d912
    /// </summary>
    /// <param name="p1">点1</param>
    /// <param name="p2">点2</param>
    /// <returns>角度</returns>
    protected float GetAngle(Vector2 p1, Vector2 p2)
    {
        float dx = p2.x - p1.x;
        float dy = p2.y - p1.y;
        float rad = Mathf.Atan2(dy, dx);
        return rad * Mathf.Rad2Deg;
    }

    void OnDestroy()
    {
        // 平滑化開放
        SmoothEyesClose.Dispose();
        SmoothEyes.Dispose();
        SmoothHead.Dispose();
        SmoothBody.Dispose();
        Smoother.Dispose();

        // RealSense開放
        FaceModule.FrameProcessed -= FaceModule_FrameProcessed;
        FaceConfig.Dispose();
        FaceData.Dispose();
        FaceModule.Dispose();
        SenseManager.Dispose();
    }
}