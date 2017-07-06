using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Intel.RealSense;
using Intel.RealSense.Face;
using Intel.RealSense.Utility;

public class RealSense : MonoBehaviour
{
    // オブジェクト
    public GameObject Body;
    public GameObject Head;
    public MeshRenderer EyeL;
    public MeshRenderer EyeR;
    public SkinnedMeshRenderer EYE_DEF;
    public SkinnedMeshRenderer EL_DEF;
    public Text Text;

    // 初期表示位置(オフセット)
    float BodyY;

    // 検出値
    Vector3 BodyPos;
    Vector3 HeadAng;
    Vector2 EyesPos;
    Dictionary<FaceExpression, FaceExpressionResult> Face;

    // 平滑化
    Smoother Smoother;
    Smoother3D SmoothBody;
    Smoother3D SmoothHead;
    Smoother2D SmoothEyes;
    Smoother1D SmoothEyesClose;

    // RealSense
    SenseManager SenseManager;
    FaceModule FaceModule;
    FaceData FaceData;
    FaceConfiguration FaceConfig;

    void Start()
    {
        try
        {
            // RealSense
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

            // 平滑化
            // 参考：https://software.intel.com/sites/landingpage/realsense/camera-sdk/v2016r3/documentation/html/index.html?doc_utils_the_smoother_utility.html
            Smoother = Smoother.CreateInstance(SenseManager.Session);

            SmoothBody = Smoother.Create3DWeighted(20);
            SmoothHead = Smoother.Create3DWeighted(20);
            SmoothEyes = Smoother.Create2DWeighted(5);
            SmoothEyesClose = Smoother.Create1DWeighted(5);

            // 初期位置保持
            BodyY = Body.transform.position.y;
        }
        catch(Exception e)
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
                BodyPos = GetBodyPos(face.Detection.BoundingRect);
            }

            if (face.Landmarks != null)
            {
                HeadAng = GetHeadAng(face.Landmarks.Points);
                EyesPos = GetEyesPos(face.Landmarks.Points);
            }

            if (face.Expressions != null)
            {
                Face = face.Expressions.ExpressionResults;
            }
        }
    }

    /// <summary>
    /// 体位置を取得
    /// </summary>
    /// <param name="rect">顔の矩形</param>
    /// <returns>体位置</returns>
    /// <remarks>
    /// https://software.intel.com/sites/landingpage/realsense/camera-sdk/v2016r3/documentation/html/index.html?doc_face_face_location_data.html
    /// </remarks>
    Vector3 GetBodyPos(RectI32 rect)
    {
        // 体位置に利用するため頭位置を取得
        StreamProfileSet profile;
        SenseManager.CaptureManager.Device.QueryStreamProfileSet(out profile);
        float xMax = profile.color.imageInfo.width;
        float yMax = profile.color.imageInfo.height;
        float xPos = rect.x + (rect.w / 2);
        float yPos = rect.h + (rect.h / 2);
        float zPos = (yMax - rect.h);

        // 末尾の除算で調整
        xPos = (xPos - (xMax / 2)) / (xMax / 2) / 3;
        yPos = (yPos - (yMax / 2)) / (yMax / 2) / 3;
        zPos = zPos / 500;

        // 初期位置のオフセットを適用
        yPos += BodyY;

        // 顔の大きさと中心から初期位置分ずらして体位置に利用
        return new Vector3(-xPos, yPos, zPos);
    }

    /// <summary>
    /// 頭角度を取得
    /// </summary>
    /// <param name="points">顔の検出点</param>
    /// <returns>頭角度</returns>
    /// <remarks>
    /// https://software.intel.com/sites/landingpage/realsense/camera-sdk/v2016r3/documentation/html/index.html?doc_face_face_landmark_data.html
    /// </remarks>
    Vector3 GetHeadAng(LandmarkPoint[] points)
    {
        // 頭向きに利用するため顔の中心と左右端、唇下、顎下を取得
        Vector2 center = points[29].image;
        Vector2 left = points[68].image;
        Vector2 right = points[54].image;
        Vector2 mouth = points[42].image;
        Vector2 chin = points[61].image;

        // 末尾で調整(0.2は顔幅に対する唇下から顎までの比 / 300はその値に対する倍率 / 10.416はUnityちゃん初期値)
        float xAng = (Vector2.Distance(left, center) - Vector2.Distance(right, center)) / Vector2.Distance(left, right) * 70;
        float yAng = GetAngle(mouth, chin) - 90;
        float zAng = (Vector2.Distance(mouth, chin) / Vector2.Distance(left, right) - 0.2f) * 300 + 10.416f;

        // 唇下と顎下の点から角度計算して頭向きに利用
        return new Vector3(xAng, -yAng, zAng);
    }

    /// <summary>
    /// 瞳位置を取得
    /// </summary>
    /// <param name="points">顔の検出点</param>
    /// <returns>瞳位置</returns>
    /// <remarks>
    /// https://software.intel.com/sites/landingpage/realsense/camera-sdk/v2016r3/documentation/html/index.html?doc_face_face_landmark_data.html
    /// </remarks>
    Vector2 GetEyesPos(LandmarkPoint[] points)
    {
        // 左右の目の瞳と上下左右端を取得
        Vector2 lEye = points[77].image;
        Vector2 lLeft = points[22].image;
        Vector2 lRight = points[18].image;
        Vector2 lTop = points[20].image;
        Vector2 lBottom = points[24].image;
        Vector2 rEye = points[76].image;
        Vector2 rLeft = points[10].image;
        Vector2 rRight = points[14].image;
        Vector2 rTop = points[12].image;
        Vector2 rBottom = points[16].image;

        // 末尾で調整
        float tmp1, tmp2;
        tmp1 = GetCenterRatio(lRight, lEye, lLeft) * 0.8f;
        tmp2 = GetCenterRatio(rRight, rEye, rLeft) * 0.8f;
        float xPos = (tmp1 + tmp2) / 2;
        tmp1 = GetCenterRatio(lTop, lEye, lBottom) * 0.2f;
        tmp2 = GetCenterRatio(rTop, rEye, rBottom) * 0.2f;
        float yPos = (tmp1 + tmp2) / 2;

        // 唇下と顎下の点から角度計算して頭向きに利用
        return new Vector2(xPos, yPos);
    }

    /// <summary>
    /// 3点の中間比を求める
    /// </summary>
    /// <param name="v1">端1</param>
    /// <param name="center">中点</param>
    /// <param name="v2">端2</param>
    /// <returns>中点比</returns>
    float GetCenterRatio(Vector2 v1, Vector2 center, Vector2 v2)
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
    float GetAngle(Vector2 p1, Vector2 p2)
    {
        float dx = p2.x - p1.x;
        float dy = p2.y - p1.y;
        float rad = Mathf.Atan2(dy, dx);
        return rad * Mathf.Rad2Deg;
    }

    void Update()
    {
        // 体移動
        Body.transform.position = SmoothBody.SmoothValue(BodyPos);

        // 頭向き
        Head.transform.localEulerAngles = SmoothHead.SmoothValue(HeadAng);

        // 視線
        Vector2 eyesPos = SmoothEyes.SmoothValue(EyesPos);
        EyeL.material.SetTextureOffset("_MainTex", eyesPos);
        EyeR.material.SetTextureOffset("_MainTex", eyesPos);

        if(Face != null)
        {
            // 目パチ
            float close = Mathf.Max(Face[FaceExpression.EXPRESSION_EYES_CLOSED_LEFT].intensity, Face[FaceExpression.EXPRESSION_EYES_CLOSED_RIGHT].intensity);
            close = SmoothEyesClose.SmoothValue(close);
            EYE_DEF.SetBlendShapeWeight(6, close);
            EL_DEF.SetBlendShapeWeight(6, close);

            // 笑顔
            EYE_DEF.SetBlendShapeWeight(0, Face[FaceExpression.EXPRESSION_SMILE].intensity);
            EL_DEF.SetBlendShapeWeight(0, Face[FaceExpression.EXPRESSION_SMILE].intensity);
        }
    }

    void OnDestroy()
    {
        // 平滑化
        SmoothEyesClose.Dispose();
        SmoothEyes.Dispose();
        SmoothHead.Dispose();
        SmoothBody.Dispose();
        Smoother.Dispose();

        // RealSense
        FaceModule.FrameProcessed -= FaceModule_FrameProcessed;
        FaceConfig.Dispose();
        FaceData.Dispose();
        FaceModule.Dispose();

        SenseManager.Dispose();
    }
}